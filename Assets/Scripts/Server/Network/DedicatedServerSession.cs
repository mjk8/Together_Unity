using System;
using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;

public class DedicatedServerSession : PacketSession
{
    public void Send(IMessage packet)
    {
        string[] parts = packet.Descriptor.Name.Split('_');
        parts[0] = char.ToUpper(parts[0][0]) + parts[0].Substring(1).ToLower();
        string msgName = string.Join("_", parts);
        msgName = msgName.Replace("_", "");
        MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId),msgName);

        ushort size = (ushort)packet.CalculateSize();
        byte[] sendBuffer = new byte[size + 4];
        Array.Copy(BitConverter.GetBytes((ushort)size + 4), 0, sendBuffer, 0, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
        Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

        Send(new ArraySegment<byte>(sendBuffer));
    }

    public override void OnConnected(EndPoint endPoint)
    {
        Debug.Log($"OnConnected DediServer : {endPoint}");

        if (PacketManager.Instance.CustomHandler == null)
        {
            PacketManager.Instance.CustomHandler = (s, m, i) =>
            {
                PacketQueue.Instance.Push(s, i, m);
            };
        }
        
        //데디서버 매니저에 ip와 port번호를 저장함
        string ip = (endPoint as IPEndPoint).Address.ToString();
        int port = (endPoint as IPEndPoint).Port;
        Managers.Dedicated.SaveIpPort(ip, port);
        
        //데디서버에 입장 요청 패킷 보냄
        if(Managers.Player._myRoomPlayer != null) //내 플레이어가 방에 있는 상태라면
        {
            CDS_AllowEnterGame sendPacket = new CDS_AllowEnterGame();
            sendPacket.RoomId = Managers.Room.GetMyPlayerRoomId();
            sendPacket.Name = Managers.Player._myRoomPlayer.Name;
            Send(sendPacket);
        }
        
        //데디서버 연결중이라는 flag를 flase로 설정해줌 (연결이 완료되었기 때문)
        Managers.Dedicated._inConnectingDediProcess= false;
        
        //내가 방장이라면, 데디서버에게 방 정보(인원 수)를 전달함
        if(Managers.Room.IsMyPlayerMaster())
        {
            CDS_InformRoomInfo sendPacket = new CDS_InformRoomInfo();
            sendPacket.RoomId = Managers.Room.GetMyPlayerRoomId();
            sendPacket.PlayerNum = Managers.Room.GetMyPlayerRoomPlayerCount();
            Send(sendPacket);
        }
        
        //데디서버와 시간 동기화를 시작함
        Managers.Time.Init();
    }
    
    public override void OnDisconnected(EndPoint endPoint)
    {
        Debug.Log($"OnDisconnected DediServer : {endPoint}");
        
        //데디서버 세션 연결이 끊겼을 때 처리
        Managers.Dedicated.LeaveDedicatedServer();
        
    }
    
    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnSend(int numOfBytes)
    {
        //Console.WriteLine($"Transferred bytes: {numOfBytes}");
    }
}