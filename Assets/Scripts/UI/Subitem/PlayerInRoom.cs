using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInRoom : MonoBehaviour
{
    private RoomPlayer _thisPlayer;
    GameObject _readyIcon;
    GameObject _masterIcon;
    TMP_Text _playerName;

    private void Awake()
    {
        _readyIcon = transform.Find("ReadyIcon").gameObject;
        _masterIcon = transform.Find("MasterIcon").gameObject;
        _playerName = transform.Find("PlayerName").GetComponent<TMP_Text>();
    }

    public void Init(RoomPlayer player)
    {
        _thisPlayer = player;
        _playerName.text = player.Name;
        _readyIcon.SetActive(player.IsReady);
        _masterIcon.SetActive(Managers.Room.IsMaster(Managers.Room.GetMyPlayerRoomId(),player.PlayerId));
        SetAlpha(1);
    }
    
    public void ToggleReady()
    {
        CS_ReadyRoom readyRoomPacket = new CS_ReadyRoom();
        readyRoomPacket.RoomId = Managers.Room.GetMyPlayerRoomId();
        readyRoomPacket.PlayerId = Managers.Player._myRoomPlayer.PlayerId;
        readyRoomPacket.IsReady = !_thisPlayer.IsReady;
        Managers.Network._roomSession.Send(readyRoomPacket);
    }

    public void ClearPlayer()
    {
        _thisPlayer = null;
        transform.Find("PlayerName").GetComponent<TMP_Text>().text = "";
        _readyIcon.SetActive(false);
        _masterIcon.SetActive(false);
        SetAlpha(0.5f);
    }
    
    public void SetAlpha (float alpha)
    {
        transform.GetComponent<Image>().color = new Color(1,1,1,alpha);
    }
}
