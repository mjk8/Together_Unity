using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterPasswordPopup : InputFieldPopup
{
    public GameRoom thisRoom { get; set; }
    public void Init(GameRoom gameRoom)
    {
        thisRoom = gameRoom;
        Init<EnterPasswordPopup>();
    }

    protected override void Submit()
    {
        UIPacketHandler.WaitForPacket();
        Managers.Room.RequestEnterRoom(thisRoom.Info.RoomId,transform.GetChild(2).GetChild(0).GetComponent<UI_InputField>().GetInputText(),Managers.Player._myRoomPlayer.Name);
        ClosePopup();
    }
}
