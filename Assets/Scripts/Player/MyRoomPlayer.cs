using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRoomPlayer : RoomPlayer
{
    public GameRoom Room { get; set; } //현재 내가 참가하고 있는 게임룸
    
    public void Clear()
    {
        Room = null;
        IsReady = false;
    }
}
