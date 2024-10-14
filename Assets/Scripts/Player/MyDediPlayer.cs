using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MyDediPlayer : MonoBehaviour
{
    //현재 데디서버의 playerId는 독자적인 값(=데디서버의 sessionID)으로 처리하고 있음
    public int PlayerId { get; set; }
    public string Name { get; set; }

    public bool _isKiller = false; //킬러 여부
    public int _killerType = -1; //어떤 킬러타입인지를 나타내는 ID
    public string _killerEngName; //킬러의 영문 이름
    
    public float _gauge = 0; //생명력 게이지
    public float _gaugeDecreasePerSecond = 0; //생명력 게이지 감소량

    public int _currentItemID = -1; //현재 가지고 있는 아이템 ID

    public PlayerStatus _playerStatus; //플레이어의 상태

    public void Init(int playerId, string name)
    {
        PlayerId = playerId;
        Name = name;
        _isKiller = false;
        _killerType = -1;
        _gauge = 0;
        _playerStatus = new PlayerStatus();
    }
}