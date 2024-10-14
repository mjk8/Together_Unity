using System;
using System.Collections;
using System.Collections.Generic;
using RainbowArt.CleanFlatUI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class ClientTimer : MonoBehaviour
{
    
    private static float _hardSnapMargin = 0.05f; //HardSanp 기준 시간 / Update 단위
    public float _clientTimerValue = 0; //현재 타이머 값
    TimerCountdownActivator _timerCountdownActivator;
    
    
    ////서버 관련 함수/////
    
    // 라운드 시작 패킷 받을 시, 라운드 시간을 변수로 콜
    public void Init(float newTimeToSet)
    {
        Managers.UI.GetComponentInSceneUI<InGameUI>().SetMaxTimerValue(newTimeToSet);
        _clientTimerValue = newTimeToSet;
        Managers.UI.GetComponentInSceneUI<InGameUI>().ChangeCurrentTimerValue(_clientTimerValue);
        _timerCountdownActivator = Util.GetOrAddComponent<TimerCountdownActivator>(transform.gameObject);
    }

    //서버 시간과 비교. hardsnap 마진보다 차이 날 시 hardsnap
    public void CompareTimerValue(float serverTimerValue)
    {
        if (Math.Abs(serverTimerValue - _clientTimerValue) >= _hardSnapMargin)
        {
            _clientTimerValue = serverTimerValue;
            Managers.UI.GetComponentInSceneUI<InGameUI>().ChangeCurrentTimerValue(_clientTimerValue);
        }
    }
    
    //서버에서 라운드 종료 패킷 받을 시 타이머 처리
    public void EndTimer()
    {
        Destroy(_timerCountdownActivator);
        _clientTimerValue = 0;
    }
}
