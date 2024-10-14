using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public int _chestId = 0; // 상자의 고유 ID (0부터 시작)
    public int _chestLevel; //상자의 레벨(1,2,3중에 하나)
    public int _point = 0; // 상자가 가지고 있는 포인트 (1렙:꽝or1, 2렙:꽝or2, 3렙:3)
    public bool _isOpened = false; // 상자가 열렸는지 여부

    private Animator _anim; //상자 애니메이터
    private CapsuleCollider _trigger; //상자 트리거

    private void Start()
    {
        _anim = transform.GetChild(0).GetComponent<Animator>();
        _trigger = transform.Find("TriggerCapsule").GetComponent<CapsuleCollider>();
    }

    public void InitChest(int chestId, int chestLevel, int point)
    {
        _chestId = chestId;
        _chestLevel = chestLevel;
        _point = point;
        _isOpened = false;
        
        if(_anim == null)
            _anim = transform.GetChild(0).GetComponent<Animator>();
        if(_trigger == null)
            _trigger = transform.Find("TriggerCapsule").GetComponent<CapsuleCollider>();
        
        //상자 닫힌 상태 애니메이션 실행하기
        _anim.SetBool("ChestOpened",false);
        
        //트리거(캡슐콜라이더) 키기
        _trigger.enabled = true;
    }

    public void HighlightChest()
    {
        _anim.speed = 0.3f;
    }
    
    public void UnHighlightChest()
    {
        _anim.speed = 1;
    }

    /// <summary>
    /// 서버가 상자가 열렸다고 패킷을 보내면 이 함수를 호출해서 상자를 열어줌(상자 효과처리도 여기서)
    /// </summary>
    public void OpenChest()
    {
        _isOpened = true;
        _anim.SetBool("ChestOpened",true); //상자 여는 애니매이션 작동
        Managers.Sound.Play("ChestOpened",Define.Sound.Effects,transform.GetComponent<AudioSource>()); //상자 여는 효과음
        _trigger.enabled = false; //트리거 비활성화
        //(꽝 상자일 경우 고려. point가 0인 경우가 꽝 상자임)
    }
}