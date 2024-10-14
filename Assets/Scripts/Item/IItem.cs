using Google.Protobuf;
using UnityEngine;

public interface IItem
{
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }

    /// <summary>
    /// 아이템이 생성될 때 필수로 설정되어야 하는 것들을 설정함
    /// </summary>
    public abstract void Init(int itemId, int playerId, string englishName);

    /// <summary>
    /// 아이템 사용시 기능 구현
    /// </summary>
    /// <param name="recvPacket">서버한테 받은 패킷정보(안쓰면 null로 들어감)</param>
    /// /// <returns>아이템 사용이 성공했다면 true, 아니면 false</returns>
    public abstract bool Use(IMessage recvPacket=null);

    /// <summary>
    /// 아이템을 들었을때 효과가 필요한 경우 구현
    /// </summary>
    public abstract void OnHold();

    /// <summary>
    /// 아이템 맞았을 때의 기능
    /// </summary>
    public abstract void OnHit();
}