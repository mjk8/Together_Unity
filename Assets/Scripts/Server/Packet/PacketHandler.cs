using System;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.Protocol;
using ServerCore;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class PacketHandler
{
    //서버한테 방 리스트를 받고 갱신함
    public static void SC_RoomListHandler(PacketSession session, IMessage packet)
    {
        SC_RoomList roomListPacket = packet as SC_RoomList;
        ServerSession serverSession = session as ServerSession;
        
        Debug.Log($"SC_RoomListHandler, {roomListPacket.Rooms.Count}개의 방 존재");
        
        //방 목록 새로 받은정보로 갱신
        Managers.Room.RefreshRoomList(roomListPacket.Rooms.ToList(), callback: UIPacketHandler.RoomListOnReceivePacket); 
    }
    
    //본인이 생성한 방 정보를 서버로부터 받음
    public static void SC_MakeRoomHandler(PacketSession session, IMessage packet)
    {
        SC_MakeRoom makeRoomPacket = packet as SC_MakeRoom;
        ServerSession serverSession = session as ServerSession;
        
        Debug.Log("SC_MakeRoomHandler");

        if (makeRoomPacket.Room != null)
        {
            //방 매니저에 방 추가
            GameRoom gameRoom = new GameRoom();
            gameRoom.Info = makeRoomPacket.Room;
            Managers.Room.AddRoom(makeRoomPacket.Room.RoomId,gameRoom);
            
            //방 생성이 성공했다면, 해당 방 입장을 요청
            Managers.Room.RequestEnterRoom(makeRoomPacket.Room.RoomId, makeRoomPacket.Password, name: Managers.Player._myRoomPlayer.Name);
        }
    }
    
    //'나'의 방 입장을 허가or거부 받음
    public static void SC_AllowEnterRoomHandler(PacketSession session, IMessage packet)
    {
        SC_AllowEnterRoom allowEnterRoomPacket = packet as SC_AllowEnterRoom;
        ServerSession serverSession = session as ServerSession;
        
        Debug.Log("SC_AllowEnterRoomHandler");

        if (allowEnterRoomPacket.CanEnter == true)
        {
            Managers.Room.ProcessEnterRoom(allowEnterRoomPacket, callback: UIPacketHandler.EnterRoomReceivePacket); 
        }
        else
        {
            UIPacketHandler.OnReceivePacket();
            Debug.Log(allowEnterRoomPacket.ReasonRejected.ToString());
            //거부된 이유에 해당하는 팝업 띄우기
            if(allowEnterRoomPacket.ReasonRejected==ReasonRejected.RoomIsFull)
            {
                Managers.UI.LoadPopupPanel<RoomIsFull>();
            }
            else if(allowEnterRoomPacket.ReasonRejected==ReasonRejected.CurrentlyPlaying)
            {
                Managers.UI.LoadPopupPanel<CurrentlyPlaying>();
            }
            else if(allowEnterRoomPacket.ReasonRejected==ReasonRejected.RoomNotExist)
            {
                Managers.UI.LoadPopupPanel<RoomNotExist>();
            }
            else if(allowEnterRoomPacket.ReasonRejected==ReasonRejected.WrongPassword)
            {
                Managers.UI.LoadPopupPanel<WrongPassword>();
            }
        }
    }
    
    //'내'가 있는 방에 새로운 유저가 들어왔을때
    public static void SC_InformNewFaceInRoomHandler(PacketSession session, IMessage packet)
    {
        SC_InformNewFaceInRoom informNewFaceInRoomPacket = packet as SC_InformNewFaceInRoom;
        ServerSession serverSession = session as ServerSession;
        
        Debug.Log("SC_InformNewFaceInRoomHandler");
        
        Managers.Room.ProcessNewFaceInRoom(informNewFaceInRoomPacket, callback: UIPacketHandler.NewFaceEnterReceivePacket);
    }
    
    //방에서 누군가가 나갔을때 (본인포함)
    public static void SC_LeaveRoomHandler(PacketSession session, IMessage packet)
    {
        SC_LeaveRoom leaveRoomPacket = packet as SC_LeaveRoom;
        ServerSession serverSession = session as ServerSession;
        
        Debug.Log("SC_LeaveRoomHandler");
        
        if(leaveRoomPacket.PlayerId == Managers.Player._myRoomPlayer.PlayerId)
        {
            Managers.Room.ProcessLeaveRoom(leaveRoomPacket, callback: UIPacketHandler.RequestLeaveRoomReceivePacket);
        }
        else
        {
            Managers.Room.ProcessLeaveRoom(leaveRoomPacket, callback: UIPacketHandler.OthersLeftRoomReceivePacket);
        }
    }
    
    //방 안 플레이어들의 레디 관련 정보가 왔을때
    public static void SC_ReadyRoomHandler(PacketSession session, IMessage packet)
    {
        SC_ReadyRoom readyRoomPacket = packet as SC_ReadyRoom;
        ServerSession serverSession = session as ServerSession;
        
        Debug.Log("SC_ReadyRoomHandler");
        
        Managers.Room.ProcessReadyRoom(readyRoomPacket, callback: UIPacketHandler.UpdateRoomReadyStatus);
    }
    
    public static void SC_PingPongHandler(PacketSession session, IMessage packet)
    {
        SC_PingPong pingPongPacket = packet as SC_PingPong;
        ServerSession serverSession = session as ServerSession;
        
        Debug.Log("SC_PingPongHandler");
        
        //서버로부터 핑을 받았다는 의미로, 살아있다는 응답으로 퐁을 보냄
        CS_PingPong sendPacket = new CS_PingPong();
        serverSession.Send(sendPacket);
    }
    
    //지금 session이 데디서버세션이아니라 서버세션으로 등록되는것이 문제임.
    public static void DSC_PingPongHandler(PacketSession session, IMessage packet)
    {
        DSC_PingPong pingPongPacket = packet as DSC_PingPong;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        //Debug.Log("DSC_PingPongHandler");
        
        //데디케이티드 서버로부터 핑을 받았다는 의미로, 살아있다는 응답으로 퐁을 보냄
        CDS_PingPong sendPacket = new CDS_PingPong();
        dedicatedServerSession.Send(sendPacket);
    }
    
    
    
    /****** 이 아래는 데디케이티드 서버로 구현하면서 변경될 예정 ***********/
    
    //데디케이트서버와 연결을 시도하고, 연결되었다면 입장요청 패킷을 보냄
    public static void SC_ConnectDedicatedServerHandler(PacketSession session, IMessage packet)
    {
        SC_ConnectDedicatedServer connectDedicatedServerPacket = packet as SC_ConnectDedicatedServer;
        ServerSession serverSession = session as ServerSession;
        Debug.Log("SC_ConnectDedicatedServerHandler");
        
        //데디케이티드 서버가 정상적으로 생성됨
        if(connectDedicatedServerPacket.Ip != null && connectDedicatedServerPacket.Port != -1) 
        {
            string dediIP = connectDedicatedServerPacket.Ip;
            int dediPort = connectDedicatedServerPacket.Port;
            
            //해당 데디케이티드 서버와 연결, 전용 세션 생성
            //세션이 생성되었을때만 입장요청 패킷(CDS_AllowEnterGame) 보냄을 보장함.
            Managers.Dedicated.ConnectToDedicatedServer(dediIP, dediPort);
            Managers.UI.ClosePopup(); // 로딩 팝업 닫기
            //게임씬 변경
            Managers.Scene.LoadScene(Define.Scene.InGame);
        }
        else//데디케이티드 서버 연결 실패
        {
            Debug.Log("데디케이티드 서버 연결 실패");
        }
    }
    
    //데디케이트서버로부터 게임에 입장을 허가받았을때의 처리
    public static void DSC_AllowEnterGameHandler(PacketSession session, IMessage packet)
    {
        DSC_AllowEnterGame allowEnterGamePacket = packet as DSC_AllowEnterGame;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_AllowEnterGameHandler");
        
        //TODO : 데디케이티드 서버로부터 게임에 입장을 허가받았을때의 처리
        Managers.Dedicated.AllowEnterGame(allowEnterGamePacket);
    }
    
    //데디케이트서버로부터 새로운 유저가 들어왔을때의 처리
    public static void DSC_InformNewFaceInDedicatedServerHandler(PacketSession session, IMessage packet)
    {
        DSC_InformNewFaceInDedicatedServer informNewFaceInDedicatedServerPacket = packet as DSC_InformNewFaceInDedicatedServer;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_InformNewFaceInDedicatedServerHandler");
        
        Managers.Dedicated.InformNewFaceInDedicatedServer(informNewFaceInDedicatedServerPacket, callback:()=>{});
    }

    //데디케이티드 서버로부터 유저가 나갔을때의 처리
    public static void DSC_InformLeaveDedicatedServerHandler(PacketSession session, IMessage packet)
    {
        DSC_InformLeaveDedicatedServer informLeaveDedicatedServerPacket = packet as DSC_InformLeaveDedicatedServer;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_InformLeaveDedicatedServerHandler");
        
        Managers.Dedicated.InformLeaveDedicatedServer(informLeaveDedicatedServerPacket, callback:()=>{});

        if (Managers.UI.SceneUI.name.Equals(Define.SceneUIType.ObserveUI.ToString()))
        {
            ObserveUI observeUI = Managers.UI.GetComponentInSceneUI<ObserveUI>();
            if (observeUI != null && informLeaveDedicatedServerPacket != null)
            {
                observeUI.ChangeIfObservingThisPlayer(informLeaveDedicatedServerPacket.LeavePlayerId);
            }
        }
        else
        {
            //TODO: Show that player left room message
        }
    }
    
    //데디케이트서버로부터 게임 시작을 알려받았을때의 처리 (이 패킷을 받은 클라는  3,2,1 카운트 후 게임을 시작함)
    public static void DSC_StartGameHandler(PacketSession session, IMessage packet)
    {
        DSC_StartGame startGamePacket = packet as DSC_StartGame;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        string itemDataJson = startGamePacket.Items;
        //Managers.Item.SaveJsonData(itemDataJson); //아이템 데이터 저장
        Managers.Item.LoadItemData(); //저장했으면 아이템 데이터 로드
        
        string killerDataJson = startGamePacket.Killers;
        //Managers.Killer.SaveJsonData(killerDataJson); //킬러 데이터 저장
        Managers.Killer.LoadKillerData(); //저장했으면 킬러 데이터 로드
        
        Debug.Log("DSC_StartGameHandler");
        Managers.UI.LoadScenePanel(Define.SceneUIType.InGameUI); //Timer를 포함한 인게임UI 부르기.
        Managers.Player.DeactivateInput();
    }
    
    //데디케이트서버로부터 유저의 움직임을 받았을때의 처리
    public static void DSC_MoveHandler(PacketSession session, IMessage packet)
    {
        DSC_Move movePacket = packet as DSC_Move;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        //Debug.Log("DSC_MoveHandler");
        
        Managers.Player._syncMoveController.SyncOtherPlayerMove(movePacket);
        
        //밤이고 인풋이 가능할 시 (킬러 생존자 시작 다름) 킬러 소리 체크
        Managers.Game.PlayChaseSound(); //킬러가 근처에 있으면 심장소리 재생
    }
    
    //데디케이트서버로부터 낮 타이머 시작을 받았을때의 처리
    public static void DSC_DayTimerStartHandler(PacketSession session, IMessage packet)
    {
        DSC_DayTimerStart dayTimerStartPacket = packet as DSC_DayTimerStart;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_DayTimerStartHandler");
        
        int daySeconds = dayTimerStartPacket.DaySeconds; //낮 시간(초)
        float estimatedCurrentServerTimer = daySeconds - Managers.Time.GetEstimatedLatency(); //현재 서버 타이머 시간(예측)

        Managers.Game.ChangeToDay(estimatedCurrentServerTimer); //낮임을 설정
       
        
        UIPacketHandler.StartGameHandler(); //게임 시작 팝업

        //낮->일몰 효과를 설정함 (낮 시간의 2/3초동안은 낮상태 유지. 남은 낮 시간의 1/3초동안 일몰로 천천히 전환됨)
        Managers.Scene.SimulateDayToSunset(daySeconds*2/3, daySeconds/3);
        
        //낮 시작할때 플레이어 정보 초기화
        Managers.Player.ResetPlayerOnDayStart();

        //씬에 생성돼있는 아이템오브젝트 제거
        Managers.Item.Clear();
    }

    //데디케이트서버로부터 낮 타이머 싱크를 받았을때의 처리
    public static void DSC_DayTimerSyncHandler(PacketSession session, IMessage packet)
    {
        DSC_DayTimerSync dayTimerSyncPacket = packet as DSC_DayTimerSync;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_DayTimerSyncHandler");

        float currentServerTimer = dayTimerSyncPacket.CurrentServerTimer; 
        float estimatedCurrentServerTimer = currentServerTimer + Managers.Time.GetEstimatedLatency(); //현재 서버 타이머 시간(예측)
        
        if (!Managers.UI.SceneUI.name.Equals(Define.SceneUIType.ObserveUI.ToString()))
        {
            Managers.Game._clientTimer.CompareTimerValue(estimatedCurrentServerTimer); //클라이언트 타이머 시간 동기화
        }
        else
        {
            Managers.UI.GetComponentInSceneUI<ObserveUI>().InitObserveTimer(estimatedCurrentServerTimer);
        }
    }

    //데디케이트서버로부터 낮 타이머 종료를 받았을때의 처리
    public static void DSC_DayTimerEndHandler(PacketSession session, IMessage packet)
    {
        DSC_DayTimerEnd dayTimerEndPacket = packet as DSC_DayTimerEnd;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_DayTimerEndHandler");
        

        int killerId = dayTimerEndPacket.KillerPlayerId;
        int killerType = dayTimerEndPacket.KillerType;
        Managers.Player.OnKillerAssigned(killerId,1, callback:()=>{}); //킬러 설정 + 그 이후 실행될 callback함수

        //일몰->밤 효과를 설정함(0초동안 일몰 유지, 3초 동안 밤으로 천천히 전환됨)
        Managers.Scene.SimulateSunsetToNight(0,3);
        
        Managers.Object._chestController.ClearAllChest();
        Managers.Player.DeactivateInput();
        if(!Managers.UI.SceneUI.name.Equals(Define.SceneUIType.ObserveUI.ToString()))
        {
            Managers.Game._clientTimer.EndTimer();
            Managers.UI.LoadPopupPanel<DayToNightPopup>(true,false); //눈 감는 팝업 띄우기
        }
        else
        {
            Managers.UI.GetComponentInSceneUI<ObserveUI>().EndTimer();
            Managers.UI.LoadPopupPanel<BlurryBackgroundPopup>(true, false);
        }
    }
    
    //데디케이트서버로부터 밤 타이머 시작을 받았을때의 처리
    public static void DSC_NightTimerStartHandler(PacketSession session, IMessage packet)
    {
        DSC_NightTimerStart nightTimerStartPacket = packet as DSC_NightTimerStart;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_NightTimerStartHandler");

        int nightSeconds = nightTimerStartPacket.NightSeconds; //밤 시간(초)
        float estimatedCurrentServerTimer = nightSeconds - Managers.Time.GetEstimatedLatency(); //현재 서버 타이머 시간(예측)
        Managers.Game.ChangeToNight(estimatedCurrentServerTimer); //밤임을 설정

        if (!Managers.UI.SceneUI.name.Equals(Define.SceneUIType.ObserveUI.ToString()))
        {
            Managers.UI.GetComponentInPopup<DayToNightPopup>().StartNight();
            float gaugeMax = nightTimerStartPacket.GaugeMax; //게이지 최대값 및 초기값
            MapField<int, float>
                playerGaugeDecreasePerSecond = nightTimerStartPacket.PlayerGaugeDecreasePerSecond; //플레이어별 게이지 감소량

            Managers.Game._clientGauge._gaugeMax = gaugeMax; //게이지 최대값 설정
            Managers.Game._clientGauge
                .SetAllGaugeDecreasePerSecond(playerGaugeDecreasePerSecond); //플레이어별 게이지 감소량을 모든 플레이어에게 적용
            Managers.Game._clientGauge.SetAllGauge(gaugeMax); //모든 플레이어의 gauge값을 gaugeMax로 초기화

            //dediplayerId를 key로, value로 estimatedgauge로 해서 map형식으로 구함. 만약 value가 0보다 작으면 0으로 설정
            MapField<int, float> estimatedGauge = new MapField<int, float>();
            foreach (int dediPlayerId in playerGaugeDecreasePerSecond.Keys)
            {
                float estimatedValue = gaugeMax -
                                       playerGaugeDecreasePerSecond[dediPlayerId] * Managers.Time.GetEstimatedLatency();
                if (estimatedValue <= 0)
                    estimatedValue = 0;

                estimatedGauge.Add(dediPlayerId, estimatedValue);
            }
            Managers.Game._clientGauge.Init();
        }
        else
        {
            Managers.UI.CloseAllPopup();
            Managers.UI.GetComponentInSceneUI<ObserveUI>().InitObserveTimer(estimatedCurrentServerTimer);
        }
        Managers.Game.SetUpKillerSound(); //킬러 두근두근 소리 Init
    }
    
    //데디케이트서버로부터 밤 타이머 싱크를 받았을때의 처리
    public static void DSC_NightTimerSyncHandler(PacketSession session, IMessage packet)
    {
        DSC_NightTimerSync nightTimerSyncPacket = packet as DSC_NightTimerSync;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_NightTimerSyncHandler");

        float currentServerTimer = nightTimerSyncPacket.CurrentServerTimer;
        float estimatedCurrentServerTimer = currentServerTimer + Managers.Time.GetEstimatedLatency(); //현재 서버 타이머 시간(예측)
        if (!Managers.UI.SceneUI.name.Equals(Define.SceneUIType.ObserveUI.ToString()))
        {
            Managers.Game._clientTimer.CompareTimerValue(estimatedCurrentServerTimer); //클라이언트 타이머 시간 동기화
        }
        else
        {
            Managers.UI.GetComponentInSceneUI<ObserveUI>().InitObserveTimer(estimatedCurrentServerTimer);
        }
    }
    
    //데디케이트서버로부터 밤 타이머 종료를 받았을때의 처리
    public static void DSC_NightTimerEndHandler(PacketSession session, IMessage packet)
    {
        DSC_NightTimerEnd nightTimerEndPacket = packet as DSC_NightTimerEnd;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_NightTimerEndHandler");
        
        DeathCause deathCause = nightTimerEndPacket.DeathCause; //죽은 이유
        int deathPlayerId = nightTimerEndPacket.DeathPlayerId; //죽은 플레이어의 id
        int killerPlayerId = nightTimerEndPacket.KillerPlayerId; //마지막 킬러의 id
        
        Managers.Sound.Stop(Define.Sound.Heartbeat); //심장소리 중지
        
        if (Managers.UI.SceneUI.name.Equals(Define.SceneUIType.InGameUI.ToString()))
        {
            Managers.Inventory.Clear(); //인벤토리 초기화
            //Managers.Input._objectInput.Clear();
            Managers.Game._playKillerSound._checkForSound = false;
            Managers.Game._clientGauge.EndGauge();
            Managers.Game._clientTimer.EndTimer();
            Managers.Inventory._hotbar.ChangeSelected(0); //선택된 아이템 초기화
        }
        else
        {
            Managers.UI.GetComponentInSceneUI<ObserveUI>().EndTimer();
        }

        //플레이어 죽음 처리
        Managers.Player.ProcessPlayerDeath(deathPlayerId, killerPlayerId);
        
        Managers.Player.DeactivateInput();
        Managers.Object._cleanseController.NightIsOver();

        //낮 되기 전에 미리 한번 플레이어 정보 초기화
        Managers.Player.ResetPlayerOnDayStart();


        //밤->낮 효과를 설정함(0초동안 밤 유지, 3초 동안 낮으로 천천히 전환됨)
        Managers.Scene.SimulateNightToSunrise(0, 3);

        //이제 낮이니까 클린즈 안보이게 처리
        Managers.Object._cleanseController._cleanseParent.SetActive(false);
    }

    //데디케이트서버로부터 새로운 상자 정보를 받았을때의 처리
    public static void DSC_NewChestsInfoHandler(PacketSession session, IMessage packet)
    {
        DSC_NewChestsInfo newChestsInfoPacket = packet as DSC_NewChestsInfo;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_NewChestsInfoHandler");
        
        Managers.Object._chestController.ChestSetAllInOne(newChestsInfoPacket);
    }

    //데디케이티드서버가 누군가가 상자를 여는데 성공했음을 알려줄때의 처리
    public static void DSC_ChestOpenSuccessHandler(PacketSession session, IMessage packet)
    {
        DSC_ChestOpenSuccess chestOpenSuccessPacket = packet as DSC_ChestOpenSuccess;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_ChestOpenSuccessHandler");
        
        int playerId = chestOpenSuccessPacket.PlayerId;
        
        
        //여는데 성공한것이 나인가? 남인가?
        if(playerId == Managers.Player._myDediPlayerId)
        {
            
            //나의 상자 열기 성공 처리
            Managers.Object._chestController.OnMyPlayerOpenChestSuccess(chestOpenSuccessPacket);
        }
        else
        {
            //다른 유저의 상자 열기 성공 처리
            Managers.Object._chestController.OnOtherPlayerOpenChestSuccess(chestOpenSuccessPacket);
        }
    }
    
    //데디케이티드서버로부터 타임스탬프를 받았을때의 처리
    public static void DSC_ResponseTimestampHandler(PacketSession session, IMessage packet)
    {
        DSC_ResponseTimestamp responseTimestampPacket = packet as DSC_ResponseTimestamp;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        //Debug.Log("DSC_ResponseTimestampHandler");
        
        Managers.Time.OnRecvDediServerTimeStamp(responseTimestampPacket);
    }
    
    //데디케이티드서버로부터 게이지싱크를 받았을때의 처리
    public static void DSC_GaugeSyncHandler(PacketSession session, IMessage packet)
    {
        DSC_GaugeSync gaugeSyncPacket = packet as DSC_GaugeSync;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
            
        MapField<int,float> playerGauges = gaugeSyncPacket.PlayerGauges;
        MapField<int, float> playerGaugeDecreasePerSecond = gaugeSyncPacket.PlayerGaugeDecreasePerSecond;
        float hardSnapMargin = Managers.Game._clientGauge._hardSnapMargin;
        
        //dediplayerId를 key로, value로 estimatedgauge로 해서 map형식으로 구함
        //MapField<int,float> estimatedGauge = new MapField<int, float>();
        foreach (int dediPlayerId in playerGauges.Keys)
        {
            float estimatedValue = playerGauges[dediPlayerId] -
                                   playerGaugeDecreasePerSecond[dediPlayerId] * Managers.Time.GetEstimatedLatency();
            if (estimatedValue<=0)
                estimatedValue = 0;

            Managers.Game._clientGauge.CheckHardSnap(dediPlayerId,estimatedValue);
        }
        Debug.Log("DSC_GaugeSyncHandler");
    }
    
    
    //데디케이티드서버로부터 초기 클린즈 정보를 받았을때의 처리
    public static void DSC_NewCleansesInfoHandler(PacketSession session, IMessage packet)
    {
        DSC_NewCleansesInfo newCleansesInfoPacket = packet as DSC_NewCleansesInfo;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_NewCleansesInfoHandler");
        
        Managers.Object._cleanseController.SpawnAllCleanse(newCleansesInfoPacket);
    }
    
    //데디케이티드서버로부터 클린즈 사용 허락을 받았을때의 처리(나or다른플레이어)
    public static void DSC_GiveCleansePermissionHandler(PacketSession session, IMessage packet)
    {
        DSC_GiveCleansePermission giveCleansePermissionPacket = packet as DSC_GiveCleansePermission;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_GiveCleansePermissionHandler");
        
        int playerId = giveCleansePermissionPacket.PlayerId;
        int cleanseId = giveCleansePermissionPacket.CleanseId;
        
        Managers.Object._cleanseController.OnGetPermission(playerId, cleanseId);
    }

    //데디케이티드서버로부터 클린즈 사용을 포기했을때의 처리
    public static void DSC_CleanseQuitHandler(PacketSession session, IMessage packet)
    {
        DSC_CleanseQuit cleanseQuitPacket = packet as DSC_CleanseQuit;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_CleanseQuitHandler");
        
        int playerId = cleanseQuitPacket.PlayerId;
        int cleanseId = cleanseQuitPacket.CleanseId;
        
        Managers.Object._cleanseController.OnOtherClientQuitCleanse(playerId, cleanseId);
    }

    //데디케이티드서버로부터 클린즈 사용 성공했을때의 처리
    public static void DSC_CleanseSuccessHandler(PacketSession session, IMessage packet)
    {
        DSC_CleanseSuccess cleanseSuccessPacket = packet as DSC_CleanseSuccess;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_CleanseSuccessHandler");
        
        int playerId = cleanseSuccessPacket.PlayerId;
        int cleanseId = cleanseSuccessPacket.CleanseId;
        float gauge = cleanseSuccessPacket.Gauge;

        float estimatedGauge;
        //내 데디
        if(playerId == Managers.Player._myDediPlayerId)
        {
            estimatedGauge = gauge - Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._gaugeDecreasePerSecond * Managers.Time.GetEstimatedLatency();
            if (estimatedGauge<=0)
                estimatedGauge = 0;
            Managers.Game._clientGauge.CheckHardSnap(playerId,estimatedGauge);
        }
        else
        {
            estimatedGauge = gauge - Managers.Player._otherDediPlayers[playerId].GetComponent<OtherDediPlayer>()._gaugeDecreasePerSecond * Managers.Time.GetEstimatedLatency();
            if (estimatedGauge<=0)
                estimatedGauge = 0;
            Managers.Game._clientGauge.CheckHardSnap(playerId,estimatedGauge);
        }
        
        Managers.Object._cleanseController.OnClientCleanseSuccess(playerId, cleanseId);
    }

    //데디케이티드서버로부터 클린즈 사용 쿨타임 시작했을때의 처리
    public static void DSC_CleanseCooltimeFinishHandler(PacketSession session, IMessage packet)
    {
        DSC_CleanseCooltimeFinish cleanseCooltimeFinishPacket = packet as DSC_CleanseCooltimeFinish;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_CleanseCooltimeFinishHandler");
        
        int cleanseId = cleanseCooltimeFinishPacket.CleanseId;
        
        Managers.Object._cleanseController.OnCleanseCooltimeFinish(cleanseId);
    }

    //데디케이티드서버로부터 아이템 구매 결과를 받았을때의 처리
    public static void DSC_ItemBuyResultHandler(PacketSession session, IMessage packet)
    {
        DSC_ItemBuyResult itemBuyResultPacket = packet as DSC_ItemBuyResult;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_ItemBuyResultHandler");
        
        int playerId = itemBuyResultPacket.PlayerId;
        int itemId = itemBuyResultPacket.ItemId;
        int itemTotalCount = itemBuyResultPacket.ItemTotalCount;
        bool isBuySuccess = itemBuyResultPacket.IsSuccess;
        int remainPoint = itemBuyResultPacket.RemainMoney;

        if (playerId == Managers.Player._myDediPlayerId)
        {
            Debug.Log("BuyItemSuccess");
            Managers.Inventory.BuyItemSuccess(itemId, itemTotalCount,isBuySuccess,remainPoint);
        }
    }

    //데디케이티드서버로부터 다른 플레이어가 특정 아이템을 들었다는 정보를 받았을때의 처리
    public static void DSC_OnHoldItemHandler(PacketSession session, IMessage packet)
    {
        DSC_OnHoldItem onHoldItemPacket = packet as DSC_OnHoldItem;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_OnHoldItemHandler");
        
        int playerId = onHoldItemPacket.PlayerId;
        int itemId = onHoldItemPacket.ItemId;
        
        Managers.Item.HoldItem(itemId,playerId);
    }

    //데디케이티드서버로부터 대시아이템을 사용했다는 정보를 받았을때의 처리
    public static void DSC_UseDashItemHandler(PacketSession session, IMessage packet)
    {
        DSC_UseDashItem useDashItemPacket = packet as DSC_UseDashItem;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_UseDashItemHandler");
        
        int playerId = useDashItemPacket.PlayerId;
        int itemId = useDashItemPacket.ItemId;

        if (Managers.Player._myDediPlayerId != playerId) //다른 플레이어의 대시 사용소식일 경우
        {
            Managers.Item.UseItem(playerId, itemId);
        }
    }

    //데디케이티드서버로부터 대시아이템 사용완료했다는 정보를 받았을때의 처리
    public static void DSC_EndDashItemHandler(PacketSession session, IMessage packet)
    {
        DSC_EndDashItem endDashItemPacket = packet as DSC_EndDashItem;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;
        
        Debug.Log("DSC_EndDashItemHandler");
        
        int playerId = endDashItemPacket.PlayerId;
        int itemId = endDashItemPacket.ItemId;

        //하드스냅 재개!
        GameObject player = Managers.Player.GetPlayerObject(playerId);
        if (player != null)
        {
            Managers.Player._syncMoveController.ToggleHardSnap(playerId, true);
        }
    }

    //데디케이티드서버로부터 플레이어가 불꽃놀이 아이템을 사용했다는 정보를 받았을때의 처리
    public static void DSC_UseFireworkItemHandler(PacketSession session, IMessage packet)
    {
        DSC_UseFireworkItem useFireworkItemPacket = packet as DSC_UseFireworkItem;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_UseFireworkItemHandler");
        int playerId = useFireworkItemPacket.PlayerId;
        int itemId = useFireworkItemPacket.ItemId;

        if (Managers.Player._myDediPlayerId != playerId) //다른 플레이어의 대시 사용소식일 경우
        {
            Managers.Item.UseItem(playerId, itemId, useFireworkItemPacket);
        }
    }

    //데디케이티드서버로부터 플레이어가 투명 아이템을 사용했다는 정보를 받았을때의 처리
    public static void DSC_UseInvisibleItemHandler(PacketSession session, IMessage packet)
    {
        DSC_UseInvisibleItem useInvisibleItemPacket = packet as DSC_UseInvisibleItem;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_UseInvisibleItemHandler");

        int playerId = useInvisibleItemPacket.PlayerId;
        int itemId = useInvisibleItemPacket.ItemId;

        if (Managers.Player._myDediPlayerId != playerId) //다른 플레이어의 투명 아이템 사용 소식일 경우
        {
            Managers.Item.UseItem(playerId, itemId);
        }
    }

    //데디케이티드서버로부터 플레이어가 플래시라이트 아이템을 사용했다는 정보를 받았을때의 처리
    public static void DSC_UseFlashlightItemHandler(PacketSession session, IMessage packet)
    {
        DSC_UseFlashlightItem useFlashlightItemPacket = packet as DSC_UseFlashlightItem;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_UseFlashlightItemHandler");

        int playerId = useFlashlightItemPacket.PlayerId;
        int itemId = useFlashlightItemPacket.ItemId;

        if (Managers.Player._myDediPlayerId != playerId) //다른 플레이어의 플래시라이트 아이템 사용 소식일 경우
        {
            Managers.Item.UseItem(playerId, itemId);
        }
    }

    //데디케이티드서버로부터 플레이어가 플래시라이트 아이템에 당했다는 정보를 받았을때의 처리
    public static void DSC_OnHitFlashlightItemHandler(PacketSession session, IMessage packet)
    {
        DSC_OnHitFlashlightItem onHitFlashlightItemPacket = packet as DSC_OnHitFlashlightItem;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_OnHitFlashlightItemHandler");
        
        int dediPlayerId = onHitFlashlightItemPacket.PlayerId;
        int itemId = onHitFlashlightItemPacket.ItemId;

        //플래시라이트에 걸렸을때 플레이어 처리
        GameObject dediPlayerGameObject = Managers.Player.GetPlayerObject(dediPlayerId);
        if (dediPlayerGameObject != null)
        {
            dediPlayerGameObject.GetComponent<ObjectInput>().ProcessFlashed(dediPlayerId);
        }
    }

    //데디케이티드서버로부터 플레이어가 트랩 아이템을 사용했다는 정보를 받았을때의 처리
    public static void DSC_UseTrapItemHandler(PacketSession session, IMessage packet)
    {
        DSC_UseTrapItem useTrapItemPacket = packet as DSC_UseTrapItem;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_UseTrapItemHandler");

        int playerId = useTrapItemPacket.PlayerId;
        int itemId = useTrapItemPacket.ItemId;
        string trapId = useTrapItemPacket.TrapId;

        if (Managers.Player._myDediPlayerId != playerId) //다른 플레이어의 트랩 아이템 사용 소식일 경우
        {
            Managers.Item.UseItem(playerId, itemId, useTrapItemPacket);
        }
    }

    //데디케이티드서버로부터 플레이어가 트랩 아이템에 걸렸을때 정보를 받았을때의 처리
    public static void DSC_OnHitTrapItemHandler(PacketSession session, IMessage packet)
    {
        DSC_OnHitTrapItem onHitTrapItemPacket = packet as DSC_OnHitTrapItem;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_OnHitTrapItemHandler");

        int dediPlayerId = onHitTrapItemPacket.PlayerId;
        int itemId = onHitTrapItemPacket.ItemId;
        string trapId = onHitTrapItemPacket.TrapId;

        //트랩 걸렸을때 덫 처리
        GameObject trapObject = Managers.Item._root.transform.Find("Trap" + trapId).gameObject;
        if (trapObject != null)
        {
            trapObject.GetComponent<Trap>().OnHit();
        }

        //트랩에 걸렸을때 플레이어 처리
        GameObject dediPlayerGameObject = Managers.Player.GetPlayerObject(dediPlayerId);
        if (dediPlayerGameObject != null)
        {
            dediPlayerGameObject.GetComponent<ObjectInput>().ProcessTrapped(dediPlayerId);
        }
    }

    //데디케이티드서버로부터 Heartless 킬러가 스킬을 사용했다는 정보를 받았을때의 처리
    public static void DSC_UseHeartlessSkillHandler(PacketSession session, IMessage packet)
    {
        DSC_UseHeartlessSkill useHeartlessSkillPacket = packet as DSC_UseHeartlessSkill;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_UseHeartlessSkill");

        int killerPlayerId = useHeartlessSkillPacket.PlayerId;
        int skillId = useHeartlessSkillPacket.KillerId;

        //내가 쓴 스킬은 브로드캐스트 받으면 안됨
        if (killerPlayerId != Managers.Player._myDediPlayerId)
        {
            Managers.Killer.UseSkill(killerPlayerId, skillId);
        }
    }
    
    //데디케이티드서버로부터 Detector 킬러가 스킬을 사용했다는 정보를 받았을때의 처리
    public static void DSC_UseDetectorSkillHandler(PacketSession session, IMessage packet)
    {
        DSC_UseDetectorSkill useDetectorSkillPacket = packet as DSC_UseDetectorSkill;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_UseDetectorSkill");

        int killerPlayerId = useDetectorSkillPacket.PlayerId;
        int skillId = useDetectorSkillPacket.KillerId;
        
        //내가 쓴 스킬은 브로드캐스트 받으면 안됨
        if (killerPlayerId != Managers.Player._myDediPlayerId)
        {
            Managers.Killer.UseSkill(killerPlayerId, skillId);
        }
    }
    
    //데디케이티드서버로부터 Detector가 감지한 생존자의 정보를 받았을 때의 처리
    public static void DSC_DetectedPlayerHandler(PacketSession session, IMessage packet)
    {
        DSC_DetectedPlayer detectedPlayerPacket = packet as DSC_DetectedPlayer;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_DetectedPlayerHandler");

        int killerPlayerId = detectedPlayerPacket.KillerId;
        int detectedPlayerId = detectedPlayerPacket.DetectedPlayerId;

        if (detectedPlayerId == Managers.Player._myDediPlayerId)
        {
            Managers.Sound.Play("Detected");
            Managers.Effects.DetectedPPPlay();
        }
    }

    //데디케이티드서버로부터 승자가 정해졌다는 정보를 받았을때의 처리
    public static void DSC_EndGameHandler(PacketSession session, IMessage packet)
    {
        DSC_EndGame endGamePacket = packet as DSC_EndGame;
        DedicatedServerSession dedicatedServerSession = session as DedicatedServerSession;

        Debug.Log("DSC_EndGameHandler");

        int winnerPlayerId = endGamePacket.WinnerPlayerId;
        string winnerPlayerName = endGamePacket.WinnerName;
        
        Managers.UI.CloseAllPopup();
        Managers.UI.LoadScenePanel(Define.SceneUIType.WinnerUI);
        Managers.UI.GetComponentInSceneUI<WinnerUI>().SetWinner(winnerPlayerId,winnerPlayerName);
    }




    //룸서버로부터 세팅정보를 받았을때 처리(저장된 세팅정보가 없을수도 있음)
    public static void SC_GetSettingHandler(PacketSession session, IMessage packet)
    {
        SC_GetSetting getSettingPacket = packet as SC_GetSetting;
        ServerSession roomSession = session as ServerSession;

        Debug.Log("SC_GetSettingHandler");

        //세팅정보를 받아서 처리(정보가 있을경우에 json으로 저장하고 적용시키기)
        Managers.Data.ApplyServerSavedSetting(getSettingPacket);

    }
}