using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;

public class RoomPlayer
{
    //현재 룸서버의 playerId는 sessionId와 같게 처리하고 있음
    public int PlayerId { get; set; }
    public string Name { get; set; } //내 이름의 경우 스팀매니저에서 스팀이름 넣어줌.
    public bool IsReady { get; set; }
    
}
