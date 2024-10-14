using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayKillerSound : MonoBehaviour
{
    private float _dokidokiStart;
    private float _dokidokiClose;
    private float _dokidokiExtreme;
    private bool isDoki;

    private float _lookAwayTime;
    //이 시간보다 이상으로 플레이어가 시야에서 벗어나면 체이스 멈춤
    private float _maxLookAwayTime = 1.0f;
    //체이스 시간 체크를 위해 기록
    private float _timeSinceLastCalled;
    private bool _isChasing;
    private bool _isChaseLookAway;

    public bool _checkForSound;
    public bool _heartlessSkillUsed;

    private void Awake()
    {
        _checkForSound = false;
        _heartlessSkillUsed = false;
    }

    public void Init(float _dokidokiStart, float _dokidokiClose, float _dokidokiExtreme)
    {
        isDoki = false;
        this._dokidokiStart = _dokidokiStart;
        this._dokidokiClose = _dokidokiClose;
        this._dokidokiExtreme = _dokidokiExtreme;
        _lookAwayTime = 0f;
        _timeSinceLastCalled = Time.time;
        _isChasing = false;
        _isChaseLookAway = false;
    }


    public void CheckPlayChaseSound()
    {
        if (_checkForSound)
        {
            //내 플레이어가 킬러일 시
            if (Managers.Player.IsMyDediPlayerKiller())
            {
                CheckKillerSound();
            }
            //내 플레이어가 생존자일 시
            else
            {
                CheckKillerSound();
                if (!_heartlessSkillUsed)
                {
                    CheckSurvivorSound();
                }
                else
                {
                    isDoki = false;
                    PlayBackgroundSound();
                    Managers.Sound.Stop(Define.Sound.Heartbeat);
                }
            }
        }
    }

    private void CheckKillerSound()
    {
        if (!CheckCurrentlyChasing())
        {
            if (_isChasing)
            {
                _isChaseLookAway = true;
                _timeSinceLastCalled = Time.time;
                _lookAwayTime = 0f;
            }
            else if (!_isChasing && _isChaseLookAway)
            {
                _lookAwayTime += Time.time - _timeSinceLastCalled;
                _timeSinceLastCalled = Time.time;
                if(_lookAwayTime>=_maxLookAwayTime)
                {
                    _isChaseLookAway = false;
                    _lookAwayTime = 0f;
                    PlayBackgroundSound();
                }
            }
            _isChasing = false;
        }
        else
        {
            if (!_isChasing)
            {
                PlayChaseSound();
            }
            _isChasing = true;
            _isChaseLookAway = false;
            _lookAwayTime = 0f;
        }
    }
    private void CheckSurvivorSound()
    {
        if (Managers.Player._myDediPlayer == null)
        {
            return;
        }
        Transform myPlayer = Managers.Player._myDediPlayer.transform;
        Vector3 killerPos = Managers.Player.GetKillerGameObject().transform.position;
        Vector3 myPlayerPos = myPlayer.position;
        float currentDistance = Vector3.Distance(myPlayerPos,killerPos);

        if (!_isChasing && !_isChaseLookAway)
        {
            //거리에 따라 두근두근 재생 여부
            if (!isDoki && currentDistance<= _dokidokiStart)
            {
                isDoki = true;
                Managers.Sound.Play("Heartbeat",Define.Sound.Heartbeat);
                Managers.Sound.Stop(Define.Sound.Bgm);
            }
            else if (isDoki && (currentDistance >_dokidokiStart))
            {
                isDoki = false;
                PlayBackgroundSound();
                Managers.Sound.Stop(Define.Sound.Heartbeat);
            }
            
            if (isDoki)
            {
                //거리에 따라 두근두근 재생 pitch (속도) 조절
                if (currentDistance <= _dokidokiExtreme)
                {
                    Managers.Sound.ChangePitch(Define.Sound.Heartbeat, 2.0f);
                }
                else if (currentDistance <= _dokidokiClose)
                {
                    Managers.Sound.ChangePitch(Define.Sound.Heartbeat, 1.5f);
                }
                else if (currentDistance <= _dokidokiStart)
                {
                    Managers.Sound.ChangePitch(Define.Sound.Heartbeat, 1.0f);
                }

                KillerSoundDirection(killerPos, myPlayerPos, myPlayer);
            }
        }
    }

    private static void KillerSoundDirection(Vector3 killerPos, Vector3 myPlayerPos, Transform myPlayer)
    {
        //방향에 따른 소리의 방향 계산
        Vector3 directionToKiller = (killerPos - myPlayerPos).normalized;
        float dotProduct = Vector3.Dot(myPlayer.transform.right, directionToKiller);
        float panStereoVal = Mathf.Sign(dotProduct);
        Managers.Sound.ChangePanStereo(Define.Sound.Heartbeat, panStereoVal);
    }

    private bool CheckCurrentlyChasing()
    {
        float radius = 0.1f;
        RaycastHit hit;
        if (Managers.Player.GetKillerGameObject() == null)
        {
            return false;
        }
        Transform current = Managers.Player.GetKillerGameObject().transform;
        Vector3 position = current.position;
        Vector3 direction = current.TransformDirection(Vector3.forward);
        return Physics.SphereCast(position, radius, direction, out hit, _dokidokiExtreme, LayerMask.GetMask($"Player"),QueryTriggerInteraction.Collide);
        //Physics.Raycast(position,current.TransformDirection(Vector3.forward), out hit,_dokidokiExtreme);
    }

    private void PlayChaseSound()
    {
        Managers.Sound.Play(string.Concat(Managers.Killer.GetKillerEnglishName(),$" Chase"), Define.Sound.Bgm);
    }
    
    private void PlayBackgroundSound()
    {
        Managers.Sound.Play("tense-horror-background",Define.Sound.Bgm);
    }
}
