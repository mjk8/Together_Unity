using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Room_Info : MonoBehaviour
{
    public GameRoom myroom { get; set; }

    private TMP_Text _roomName;
    private TMP_Text _count;
    private Image _lockIcon;
    private UI_Text _roomStatus;

    private void Start()
    {
        _roomName = transform.Find("RoomName").GetComponent<TMP_Text>();
        _count = transform.Find("Count").GetComponent<TMP_Text>();
        _lockIcon = transform.Find("LockIcon").GetComponent<Image>();
        _roomStatus = transform.Find("RoomStatus").GetComponent<UI_Text>();
        Clear();
    }

    public void Init(GameRoom gameRoom)
    {
        myroom = gameRoom;
        _roomName.GetComponent<TMP_Text>().text = gameRoom.Info.Title;
        _count.GetComponent<TMP_Text>().text = (gameRoom.Info.CurrentCount + "/" + gameRoom.Info.MaxCount);
        if (gameRoom.Info.IsPlaying)
        {
            _roomStatus.GetComponent<UI_Text>().SetString("InGameStatus");
        }
        else
        {
            _roomStatus.GetComponent<UI_Text>().SetString("WaitingStatus");
        }
        
        _lockIcon.GetComponent<Image>().enabled = gameRoom.Info.IsPrivate;
        transform.GetComponent<UI_Button>().SetOnClick(EnterRoomUI);
    }

    public void EnterRoomUI()
    {
        if (myroom.Info.IsPrivate)
        {
            EnterPasswordPopup popup = Managers.UI.LoadPopupPanel<EnterPasswordPopup>();
            popup.Init(myroom);
        }
        else
        {
            UIPacketHandler.WaitForPacket();
            Managers.Room.RequestEnterRoom(myroom.Info.RoomId,"",Managers.Player._myRoomPlayer.Name);
        }
    }

    public void Clear()
    {
        _roomName.text = "";
        _count.text = "";
        _lockIcon.enabled = false;
        _roomStatus.SetString(null) ;
    }
}
