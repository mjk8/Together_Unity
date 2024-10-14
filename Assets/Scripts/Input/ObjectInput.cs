using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectInput : MonoBehaviour
{
    #region 내 플레이어만 사용하는 것들

    public GameObject _currentChest;
    public GameObject _currentCleanse;

    /// <summary>
    /// 열수있는 상자 표시(애니메이션 느려짐)
    /// </summary>
    /// <param name="newChest">범위안에 감지된 상자</param>
    public void ChangeHighlightChest(GameObject newChest)
    {
        if (_currentChest != null)
        {
            if (
            Vector3.Angle(transform.forward, _currentChest.transform.position - transform.position) <
            Vector3.Angle(transform.forward, newChest.transform.position - transform.position))
            {
                return;
            }
            _currentChest.GetComponent<Chest>().UnHighlightChest();
        }
        _currentChest = newChest;
        _currentChest.GetComponent<Chest>().HighlightChest();
    }

    /// <summary>
    /// 좌클릭: 낮-상자열기 , 밤-생존자면 클렌즈;킬러면 기본공격
    /// </summary>
    /// <param name="value">뉴인풋시스템 사용하느라 필요한거</param>
    void OnInteract(InputValue value)
    {
        Debug.Log("OnInteract");
        if (Managers.Game._isDay) //낮이면
        {
            if (_currentChest != null)
            {
                //상자 열기 시도
                Managers.Object._chestController.TryOpenChest(_currentChest.GetComponent<Chest>()._chestId);
            }
        }
        else //밤이면
        {
            //생존자일때
            if (!Managers.Player.IsMyDediPlayerKiller())
            {
                if (_currentCleanse != null)
                {
                    if ((Managers.Object._cleanseController._myPlayerCurrentCleanse == null) && value.isPressed)
                    {
                        Managers.Object._cleanseController.TryCleanse(
                            _currentCleanse.GetComponent<Cleanse>()._cleanseId);
                    }
                    else if (value.isPressed)
                    {
                        QuitCleansing();
                    }
                }
            }
            //킬러일때
            else
            {
                Managers.Killer.BaseAttack(Managers.Player._myDediPlayerId);
            }
        }
    }

    /// <summary>
    /// 클렌즈 취소
    /// </summary>
    public void QuitCleansing()
    {
        if ((Managers.Object._cleanseController._myPlayerCurrentCleanse != null))
        {
            Managers.Object._cleanseController.QuitCleansing(_currentCleanse.GetComponent<Cleanse>()._cleanseId);
        }
    }

    #endregion

    public void Clear()
    {
        _currentChest = null;
        _currentCleanse = null;
    }

    #region 트랩 관련

    /// <summary>
    /// 서버측 판정으로 트랩에 걸렸을때 처리하는 함수
    /// </summary>
    /// <param name="dediPlayerId"></param>
    public void ProcessTrapped(int dediPlayerId)
    {
        StartCoroutine(Trapped(dediPlayerId));
    }

    /// <summary>
    /// 트랩에 걸렸을때
    /// </summary>
    /// <param name="dediPlayerId">트랩에 걸린 데디플레이어id</param>
    private IEnumerator Trapped(int dediPlayerId)
    {
        float stunDuration = (Managers.Item._itemFactories[4] as TrapFactory).StunDuration;

        if (dediPlayerId == Managers.Player._myDediPlayerId) //내 플레이어일때
        {
            GameObject myDediPlayerGameObject = Managers.Player._myDediPlayer;
            MyDediPlayer myDediPlayer = myDediPlayerGameObject.GetComponent<MyDediPlayer>();
            if (myDediPlayer._playerStatus._isCurrentTrapped) //이미 트랩에 걸렸으면 리턴
            {
                yield break;
            }

            //애니메이션 재생
            PlayerAnimController playerAnimController = myDediPlayerGameObject.GetComponentInChildren<PlayerAnimController>();
            playerAnimController.isTrapped = true;
            playerAnimController.PlayAnim();

            //인풋막음
            Managers.Player.DeactivateInput();

            //현재 트랩에 걸렸다고 표시
            myDediPlayer._playerStatus._isCurrentTrapped = true;

            //스턴시간만큼 기다림
            float estimatedStunDuration = stunDuration - Managers.Time.GetEstimatedLatency();
            yield return new WaitForSeconds(estimatedStunDuration);

            //애니메이션 재생 해제
            playerAnimController.isTrapped = false;
            playerAnimController.PlayAnim();

            //인풋 풀음
            Managers.Player.ActivateInput();

            //트랩에 걸린 상태 해제
            myDediPlayer._playerStatus._isCurrentTrapped = false;
        }
        else //다른 플레이어일때
        {
            GameObject otherDediPlayerGameObject = Managers.Player._otherDediPlayers[dediPlayerId];
            OtherDediPlayer otherDediPlayer = otherDediPlayerGameObject.GetComponent<OtherDediPlayer>();
            if (otherDediPlayer._playerStatus._isCurrentTrapped) //이미 트랩에 걸렸으면 리턴
            {
                yield break;
            }


            //애니메이션 재생
            PlayerAnimController playerAnimController = otherDediPlayerGameObject.GetComponentInChildren<PlayerAnimController>();
            playerAnimController.isTrapped = true;
            playerAnimController.PlayAnim();
            //고스트 따라가기 막음
            otherDediPlayer.ToggleFollowGhost(false);
            //현재 트랩에 걸렸다고 표시
            otherDediPlayer._playerStatus._isCurrentTrapped = true;

            //스턴시간만큼 기다림
            float estimatedStunDuration = stunDuration - Managers.Time.GetEstimatedLatency();
            yield return new WaitForSeconds(estimatedStunDuration);

            //애니메이션 재생 해제
            playerAnimController.isTrapped = false;
            playerAnimController.PlayAnim();
            //고스트 따라가기 재개
            otherDediPlayer.ToggleFollowGhost(true);
            //트랩에 걸린 상태 해제
            otherDediPlayer._playerStatus._isCurrentTrapped = false;
        }
    }


    #endregion

    #region 손전등 관련

    private Coroutine _currentPlayingFlashedCoroutine;

    public void ProcessFlashed(int dediPlayerId)
    {
        if (dediPlayerId == Managers.Player._myDediPlayerId) //내 플레이어일때
        {
            GameObject myDediPlayerGameObject = Managers.Player._myDediPlayer;
            MyDediPlayer myDediPlayer = myDediPlayerGameObject.GetComponent<MyDediPlayer>();

            if (myDediPlayer._playerStatus._isFlashed) //이미 실명에 걸렸으면 기존 코루틴 취소하고 재 실명처리
            {
                StopCoroutine(_currentPlayingFlashedCoroutine);
                _currentPlayingFlashedCoroutine = StartCoroutine(Flashed(dediPlayerId));
                return;
            }
            _currentPlayingFlashedCoroutine = StartCoroutine(Flashed(dediPlayerId));
        }
        else
        {
            GameObject otherDediPlayerGameObject = Managers.Player._otherDediPlayers[dediPlayerId];
            OtherDediPlayer otherDediPlayer = otherDediPlayerGameObject.GetComponent<OtherDediPlayer>();

            if (otherDediPlayer._playerStatus._isFlashed) //이미 실명에 걸렸으면 기존 코루틴 취소하고 재 실명처리
            {
                StopCoroutine(_currentPlayingFlashedCoroutine);
                _currentPlayingFlashedCoroutine = StartCoroutine(Flashed(dediPlayerId));
                return;
            }

            _currentPlayingFlashedCoroutine = StartCoroutine(Flashed(dediPlayerId));
        }

    }

    private IEnumerator Flashed(int dediPlayerId)
    {
        float blindDuration = (Managers.Item._itemFactories[3] as FlashlightFactory).BlindDuration;

        if (dediPlayerId == Managers.Player._myDediPlayerId) //내 플레이어일때
        {
            GameObject myDediPlayerGameObject = Managers.Player._myDediPlayer;
            MyDediPlayer myDediPlayer = myDediPlayerGameObject.GetComponent<MyDediPlayer>();

            //현재 플래시에 맞았다고 표시
            Debug.Log("내 시야 막힘 플래시맞아서!!!!!!!!!!!!!!!");
            myDediPlayer._playerStatus._isFlashed = true;
            Managers.Effects.FlashlightPPPlay();

            //실명시간만큼 기다림
            yield return new WaitForSeconds(blindDuration);

            //실명에 걸린 상태 해제
            myDediPlayer._playerStatus._isFlashed = false;
            Debug.Log("플래시 이제 풀림!!");
            Managers.Effects.FlashlightPPStop();

        }
        else
        {
            GameObject otherDediPlayerGameObject = Managers.Player._otherDediPlayers[dediPlayerId];
            OtherDediPlayer otherDediPlayer = otherDediPlayerGameObject.GetComponent<OtherDediPlayer>();

            //현재 플래시에 맞았다고 표시
            otherDediPlayer._playerStatus._isFlashed = true;
            //TODO: 다른 플레이어 시야 가려졌다는 이펙트 표시

            //실명시간만큼 기다림
            yield return new WaitForSeconds(blindDuration);

            //실명에 걸린 상태 해제
            otherDediPlayer._playerStatus._isFlashed = false;
            //TODO: 다른 플레이어 시야 가려졌다는 이펙트 지우기

        }
    }

    #endregion




}
