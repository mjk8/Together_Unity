using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;

public class Invisible : MonoBehaviour, IItem
{
    //IItem 인터페이스 구현
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }

    //이 아이템만의 속성
    public float InvisibleSeconds { get; set; }


    private GameObject _player;
    private GameObject _rootM; //투명 처리를 위해서 껐다 킬 오브젝트
    private float _animationSeconds = 0.7f;
    private bool _isInvisibleNow = false;
    private Coroutine _currentPlayingCoroutine;

    public void Init(int itemId, int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }

    public void Init(int itemId, int playerId, string englishName, float invisibleSeconds)
    {
        Init(itemId,playerId, englishName);
        InvisibleSeconds = invisibleSeconds;
    }
    
    public bool Use(IMessage recvPacket = null)
    {
        if (PlayerID == Managers.Player._myDediPlayerId)
        {
            //이미 사용중인데 또 사용하려고 하면, 기존 코루틴 종료하고 코루틴 다시시작
            if (_isInvisibleNow)
            {
                StopCoroutine(_currentPlayingCoroutine);
                _rootM.SetActive(false);
                _currentPlayingCoroutine = StartCoroutine(ToggleRootM());
                return true;
            }

            //애니메이션 재생
            Managers.Player.GetAnimator(PlayerID).SetTriggerByString("Invisible");

            _player = Managers.Player._myDediPlayer;
            _rootM = Util.FindChild(_player, "Root_M", true);

            _currentPlayingCoroutine = StartCoroutine(ToggleRootM());
            return true;
        }
        else
        {
            //이미 사용중인데 또 사용하려고 하면, 기존 코루틴 종료하고 코루틴 다시시작
            if (_isInvisibleNow)
            {
                StopCoroutine(_currentPlayingCoroutine);
                _rootM.SetActive(false);
                _currentPlayingCoroutine = StartCoroutine(ToggleRootM());
                return true;
            }

            //애니메이션 재생
            Managers.Player.GetAnimator(PlayerID).SetTriggerByString("Invisible");

            _player = Managers.Player.GetPlayerObject(PlayerID);
            _rootM = Util.FindChild(_player, "Root_M", true);

            _currentPlayingCoroutine = StartCoroutine(ToggleRootM());
            return true;
        }

    }


    IEnumerator ToggleRootM()
    {
        if (PlayerID == Managers.Player._myDediPlayerId)
        {
            // 투명 아이템 사용 패킷 서버로 보내기
            CDS_UseInvisibleItem cdsUseInvisibleItem = new CDS_UseInvisibleItem();
            cdsUseInvisibleItem.MyDediplayerId = PlayerID;
            cdsUseInvisibleItem.ItemId = ItemID;
            Managers.Network._dedicatedServerSession.Send(cdsUseInvisibleItem);
        }

        if (_isInvisibleNow == false)
        {
            //_animationSeconds만큼 대기(애니메이션 재생시간)
            yield return new WaitForSeconds(_animationSeconds);
        }

        _isInvisibleNow = true;

        // Root_M을 비활성화
        _rootM.SetActive(false);
        Debug.Log("Root_M has been turned off.");

        // 2초 대기
        yield return new WaitForSeconds(InvisibleSeconds);

        // Root_M을 활성화
        _rootM.SetActive(true);

        //투명 끝났으므로 오브젝트 삭제
        Destroy(gameObject);
    }

    public void OnHold()
    {

    }

    public void OnHit()
    {
        
    }
}
