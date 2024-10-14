using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;

public class Invisible : MonoBehaviour, IItem
{
    //IItem �������̽� ����
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }

    //�� �����۸��� �Ӽ�
    public float InvisibleSeconds { get; set; }


    private GameObject _player;
    private GameObject _rootM; //���� ó���� ���ؼ� ���� ų ������Ʈ
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
            //�̹� ������ε� �� ����Ϸ��� �ϸ�, ���� �ڷ�ƾ �����ϰ� �ڷ�ƾ �ٽý���
            if (_isInvisibleNow)
            {
                StopCoroutine(_currentPlayingCoroutine);
                _rootM.SetActive(false);
                _currentPlayingCoroutine = StartCoroutine(ToggleRootM());
                return true;
            }

            //�ִϸ��̼� ���
            Managers.Player.GetAnimator(PlayerID).SetTriggerByString("Invisible");

            _player = Managers.Player._myDediPlayer;
            _rootM = Util.FindChild(_player, "Root_M", true);

            _currentPlayingCoroutine = StartCoroutine(ToggleRootM());
            return true;
        }
        else
        {
            //�̹� ������ε� �� ����Ϸ��� �ϸ�, ���� �ڷ�ƾ �����ϰ� �ڷ�ƾ �ٽý���
            if (_isInvisibleNow)
            {
                StopCoroutine(_currentPlayingCoroutine);
                _rootM.SetActive(false);
                _currentPlayingCoroutine = StartCoroutine(ToggleRootM());
                return true;
            }

            //�ִϸ��̼� ���
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
            // ���� ������ ��� ��Ŷ ������ ������
            CDS_UseInvisibleItem cdsUseInvisibleItem = new CDS_UseInvisibleItem();
            cdsUseInvisibleItem.MyDediplayerId = PlayerID;
            cdsUseInvisibleItem.ItemId = ItemID;
            Managers.Network._dedicatedServerSession.Send(cdsUseInvisibleItem);
        }

        if (_isInvisibleNow == false)
        {
            //_animationSeconds��ŭ ���(�ִϸ��̼� ����ð�)
            yield return new WaitForSeconds(_animationSeconds);
        }

        _isInvisibleNow = true;

        // Root_M�� ��Ȱ��ȭ
        _rootM.SetActive(false);
        Debug.Log("Root_M has been turned off.");

        // 2�� ���
        yield return new WaitForSeconds(InvisibleSeconds);

        // Root_M�� Ȱ��ȭ
        _rootM.SetActive(true);

        //���� �������Ƿ� ������Ʈ ����
        Destroy(gameObject);
    }

    public void OnHold()
    {

    }

    public void OnHit()
    {
        
    }
}
