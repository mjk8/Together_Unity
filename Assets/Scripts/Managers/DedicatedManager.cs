using System;
using System.Net;
using Google.Protobuf.Protocol;
using ServerCore;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// 데디서버 정보 저장 및 접속 관련 매니저
/// </summary>
public class DedicatedManager
{
    public string Ip { get; set; }
    public int Port { get; set; }

    //데디서버 세션은네트워크매니저에 있음
    //데디 플레이어 정보는 플레이에매니저에 있음

    public void SaveIpPort(string ip, int port)
    {
        Ip = ip;
        Port = port;
    }

    /// <summary>
    /// 소켓shutdown은 안하고 관련 정보만 초기화
    /// </summary>
    public void LeaveDedicatedServer()
    {
        //데디서버로 나가기 패킷 보내기
        CDS_InformLeaveDedicatedServer informLeaveDedicatedServerPacket = new CDS_InformLeaveDedicatedServer();
        informLeaveDedicatedServerPacket.DediplayerId = Managers.Player._myDediPlayerId;
        Managers.Network._dedicatedServerSession.Send(informLeaveDedicatedServerPacket);

        //로비로 돌아가기 위해서 룸서버로 해당 게임방에서 나갔다는 패킷 보내기
        CS_LeaveRoom leaveRoomPacket = new CS_LeaveRoom();
        leaveRoomPacket.RoomId = Managers.Room.GetMyPlayerRoomId();
        Managers.Network._roomSession.Send(leaveRoomPacket);

        //데디 관련 초기화
        Managers.Player.ClearDedi();
        Managers.Pool.Clear();
        Managers.Inventory.Clear();
        Managers.Item.Clear();

        Ip = null;
        Port = -1;
    }

    /// <summary>
    /// 데디케이티드 서버로부터 게임에 입장을 허가받았을때, 플레이어들을 스폰하고 플레이어매니저에 저장
    /// </summary>
    /// <param name="packet">입장 허용패킷</param>
    public void AllowEnterGame(DSC_AllowEnterGame packet, Action callback=null)
    {
        //다른 데디플레이어정보들도 플레이어매니저에 저장
        foreach (PlayerInfo playerInfo in packet.Players)
        {
            if (playerInfo.PlayerId == packet.MyDedicatedPlayerId) //내 데디플레이어를 스폰하고 플레이어 매니저에 정보 저장
            {
                Managers.Player.SpawnPlayer(playerInfo, packet.PlayerTransforms[packet.MyDedicatedPlayerId], true);
            }
            else //다른 데디플레이어 생성하고 고스트를 생성하고 플레이어 매니저에 정보 저장
            {
                Managers.Player.SpawnPlayer(playerInfo, packet.PlayerTransforms[playerInfo.PlayerId], false);
            }
        }
        
        //콜백함수 실행
        if (callback != null)
            callback.Invoke();
    }

    public bool _inConnectingDediProcess = false;

    /// <summary>
    /// 데디서버와 연결하고 전용세션을 생성.
    /// 세션이 생성되었을때만 입장요청 패킷(CDS_AllowEnterGame) 보냄을 보장함.
    /// </summary>
    /// <param name="ip">데디서버 ip</param>
    /// <param name="port">데디서버 포트번호</param>
    public void ConnectToDedicatedServer(string ip, int port)
    {
        //connector가 접속중이고, 접속완료가 아직 안되었다면 바로 return
        if (_inConnectingDediProcess)
            return;

        //접속프로세스 시작
        _inConnectingDediProcess = true;

        //TODO : 이미 접속이 완료되었는데 또 접속을 시도한다면 기존 정보로 접속을 시도하는  코드를 추가해야함...
        if (Managers.Network._dedicatedServerSession._socket != null)
            return;

        Debug.Log("실제 커넥터쩜커넥트 호출");

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        Connector connector = new Connector();
        connector.Connect(endPoint, () => { return Managers.Network._dedicatedServerSession; }, 1);
    }

    /// <summary>
    /// 새로운 들어온 플레이어를 스폰하고 플레이어매니저에 저장
    /// </summary>
    /// <param name="informNewFaceInDedicatedServerPacket"></param>
    /// <param name="callback"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void InformNewFaceInDedicatedServer(DSC_InformNewFaceInDedicatedServer packet, Action callback)
    {
        //다른 데디플레이어 생성하고 고스트를 생성하고 플레이어 매니저에 정보 저장
        Managers.Player.SpawnPlayer(packet.NewPlayer, packet.SpawnTransform, false);
        
        //콜백함수 실행
        if (callback != null)
            callback.Invoke();
    }

    /// <summary>
    /// '나'를 제외한 다를 플레이어가 나갔을때 게임에서 제거하고 플레이어매니저에서도 제거
    /// </summary>
    /// <param name="informLeaveDedicatedServerPacket"></param>
    /// <param name="callback"></param>
    public void InformLeaveDedicatedServer(DSC_InformLeaveDedicatedServer informLeaveDedicatedServerPacket, Action callback)
    {
        int leavePlayerId = informLeaveDedicatedServerPacket.LeavePlayerId;
        if (Managers.Player._otherDediPlayers.ContainsKey(leavePlayerId))
        {
            GameObject leavePlayerObj = Managers.Player._otherDediPlayers[leavePlayerId];
            Managers.Player.DespawnPlayer(leavePlayerObj);
            Managers.Player._otherDediPlayers.Remove(leavePlayerId);
        }
        if(Managers.Player._ghosts.ContainsKey(leavePlayerId))
        {
            GameObject leaveGhostObj = Managers.Player._ghosts[leavePlayerId];
            Managers.Player.DespawnGhost(leaveGhostObj);
            Managers.Player._ghosts.Remove(leavePlayerId);
        }
        
        //콜백함수 실행
        if (callback != null)
            callback.Invoke();
    }
    
}