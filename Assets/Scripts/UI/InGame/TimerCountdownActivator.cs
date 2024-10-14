using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerCountdownActivator : MonoBehaviour
{
    InGameUI _inGameUI;
    private void Start()
    {
        _inGameUI = Managers.UI.GetComponentInSceneUI<InGameUI>();
    }

    //client 자체 카운트 다운
     void Update()
    {
        float old = Managers.Game._clientTimer._clientTimerValue;
        float cur = Mathf.Max(0f, old - Time.deltaTime);
        Managers.Game._clientTimer._clientTimerValue = cur;
        if (_inGameUI != null)
        {
            _inGameUI.ChangeCurrentTimerValue(cur);
        }
    }
}
