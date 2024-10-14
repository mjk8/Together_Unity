using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : UI_scene
{
    private GameRoom thisRoom;
    GameObject myPlayer;
    UI_Button _startReadyButton;
    List<PlayerInRoom> _playerTiles = new List<PlayerInRoom>();
    private Transform PanelOne;
    private Transform PanelTwo;

    private enum Buttons
    {
        BackToLobbyButton,
        RefreshButton
    }

    void Init()
    {
        thisRoom = Managers.Player._myRoomPlayer.Room;
        InitButtons<Buttons>(gameObject);
        IfMaster();
        GetPlayerList();
    }
    void Awake()
    {
        //추후 접근이 필요한 오브젝트들을 찾아서 저장
        _startReadyButton = transform.Find("StartReadyButton").GetComponent<UI_Button>();
        PanelOne = transform.Find("PlayersPanel").GetChild(0);
        PanelTwo = transform.Find("PlayersPanel").GetChild(1);
        PanelOne.GetComponent<VerticalLayoutGroup>().spacing = Screen.height / 180;
        PanelTwo.GetComponent<VerticalLayoutGroup>().spacing = Screen.height / 180;
        
        //각각의 패널들에서 PlayerInRoom을 가지고 와서 _playerTiles에 저장
        foreach (Transform child in PanelOne)
        {
            _playerTiles.Add(child.GetComponent<PlayerInRoom>());
        }
        foreach (Transform child in PanelTwo)
        {
            _playerTiles.Add(child.GetComponent<PlayerInRoom>());
        }
        Init();
    }
    
    //만약 방장이라면 StartGameButton을, 아니라면 ReadyButton
    public void IfMaster()
    {
        if (Managers.Room.IsMyPlayerMaster())
        {
            
            _startReadyButton.GetComponentInChildren<UI_Text>().SetString("StartGame");
            _startReadyButton.RemoveAllOnClick();
            _startReadyButton.SetOnClick(StartGame);
            _startReadyButton.SetButtonActivation(Managers.Room.IsMyRoomAllPlayerReady());
        }
        else
        { 
            _startReadyButton.GetComponentInChildren<UI_Text>().SetString("ReadyButton");
            _startReadyButton.RemoveAllOnClick();
            _startReadyButton.SetOnClick(ReadyButton);
        }
    }

    /// <summary>
    /// 플레이어에 대한 PlayerInRoom 생성. 방장 표시 포함.
    /// </summary>
    public void GetPlayerList()
    {
        //방이 없으면 로비로 돌아감
        if (thisRoom == null)
        {
            Managers.UI.LoadScenePanel(Define.SceneUIType.LobbyUI);
        }
        
        IfMaster(); //방장인지 확인
        
        //Max Player가 8명이라는 가정하에 짜인 코드
        for(int i =0;i<_playerTiles.Count;i++)
        {
            if(i<thisRoom._players.Count)
            {
                _playerTiles[i].Init(thisRoom._players[i]);
                if (thisRoom._players[i].PlayerId == Managers.Player._myRoomPlayer.PlayerId)
                {
                    myPlayer = _playerTiles[i].gameObject;
                }
            }
            else
            {
                _playerTiles[i].ClearPlayer();
            }
        }
    }

    void BackToLobbyButton()
    {
        UIPacketHandler.LeaveRoomSendPacket(thisRoom.Info.RoomId);
    }

    void ReadyButton()
    {
        _startReadyButton.PlayButtonClick();
        myPlayer.GetComponent<PlayerInRoom>().ToggleReady();
    }

    public void RefreshButton()
    {
        if (thisRoom == null)
        {
            
        }
        GetPlayerList();
    }

    
    public void StartGame()
    {
        if (!Managers.Room.IsMyRoomAllPlayerReady())
        {
            return;
        }
        Debug.Log("겜시작버튼 눌림");
        CS_ConnectDedicatedServer sendPacket = new CS_ConnectDedicatedServer();
        sendPacket.RoomId = thisRoom.Info.RoomId;
        Managers.Network._roomSession.Send(sendPacket);
    }

    void ClearPlayerListPanel()
    {
        foreach (var player in _playerTiles)
        {
            player.ClearPlayer();
        }
    }
}