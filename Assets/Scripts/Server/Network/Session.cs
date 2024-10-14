﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public abstract class PacketSession : Session
{
    public static readonly int HeaderSize = 2;

    //sealed를 쓰면, 다른 클래스가 PacketSession을 상속받은다음, OnRecv을 오버라이딩 하려고 하면 에러가 남
    //현재 정책은 size는 본인을 포함한, 즉 [size(2)][packetId(2)][...] 전체의 사이즈를 저장하고 있음
    //[size(2)][packetId(2)][...][size(2)][packetId(2)][...]
    public sealed override int OnRecv(ArraySegment<byte> buffer)
    {
        int processLen = 0;

        while (true)
        {
            //최소한 헤더(사이즈)는 파싱할 수 있는지 확인
            if (buffer.Count < HeaderSize)
                break;

            //패킷이 완전체로 도착했는지 확인 (ushort만큼 긁어서 뱉어줌=데이터 사이즈가 들어잇는 부분)
            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            if (buffer.Count < dataSize)
                break;

            //여기까지 왔으면 패킷 조립 가능
            OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

            processLen += dataSize;
            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
        }

        return processLen; //처리한 바이트수를 리턴
    }

    public abstract void OnRecvPacket(ArraySegment<byte> buffer);
}

public abstract class Session
{
    public Socket _socket;
    private int _disconnected = 0;

    private RecvBuffer _recvBuffer = new RecvBuffer(65535);

    object _lock = new object();
    private Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
    List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
    SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

    public abstract void OnConnected(EndPoint endPoint);
    public abstract void OnDisconnected(EndPoint endPoint);
    public abstract int OnRecv(ArraySegment<byte> buffer); //얼마만큼의 데이터를 처리했는지 반환해줌

    public abstract void OnSend(int numOfBytes);

    void Clear()
    {
        lock (_lock)
        {
            _sendQueue.Clear();
            _pendingList.Clear();
        }
    }

    public void Start(Socket socket)
    {
        _socket = socket;
        //네이글 알고리즘을 끔
        _socket.NoDelay = true;

        _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

        RegisterRecv();
    }

    public void Send(List<ArraySegment<byte>> sendBuffList)
    {
        if (sendBuffList.Count == 0)
            return;

        lock (_lock)
        {
            foreach (ArraySegment<byte> sendBuff in sendBuffList)
                _sendQueue.Enqueue(sendBuff);

            if (_pendingList.Count == 0)
            {
                RegisterSend();
            }
        }
    }

    public void Send(ArraySegment<byte> sendBuffer)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuffer);
            if (_pendingList.Count == 0)
            {
                RegisterSend();
            }
        }
    }

    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, 1) == 1)
            return;

        OnDisconnected(_socket.RemoteEndPoint);
        _socket.Shutdown(SocketShutdown.Both); //소켓 수신,송신 기능 종료
        _socket.Close(); //현재 연결을 종료하고 리소스를 정리함
        Clear();
    }

    #region 네트워크 통신

    void RegisterSend()
    {
        if (_disconnected == 1)
            return;

        while (_sendQueue.Count > 0)
        {
            ArraySegment<byte> buff = _sendQueue.Dequeue();
            _pendingList.Add(buff);
        }

        _sendArgs.BufferList = _pendingList;

        try
        {
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"RegisterSend Failed {e}");
        }
    }

    void OnSendCompleted(object sender, SocketAsyncEventArgs args)
    {
        lock (_lock)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    _sendArgs.BufferList = null;
                    _pendingList.Clear();

                    OnSend(_sendArgs.BytesTransferred);

                    if (_sendQueue.Count > 0)
                        RegisterSend();
                }
                catch (Exception e)
                {
                    Debug.Log($"OnSendCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
    }

    void RegisterRecv()
    {
        if (_disconnected == 1)
            return;

        _recvBuffer.Clean();
        ArraySegment<byte> segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        try
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (pending == false)
            {
                OnRecvCompleted(null, _recvArgs);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"RegisterRecv Failed {e}");
        }
    }

    void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            //TODO
            try
            {
                //Write 커서 이동
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }

                //컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다 (tcp 특성상 바이트 전체가 아닌 일부가 왔을수도 있음)
                int processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0 || _recvBuffer.DataSize < processLen)
                {
                    Disconnect();
                    return;
                }

                //Read 커서 이동
                if (_recvBuffer.OnRead(processLen) == false)
                {
                    Disconnect();
                    return;
                }

                RegisterRecv();
            }
            catch (Exception e)
            {
                Debug.Log($"OnRecvCompleted Failed {e}");
            }
        }
        else
        {
            //TODO
            Disconnect();
        }
    }

    #endregion
}

