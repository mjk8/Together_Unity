using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadTimerCountdownActivator : MonoBehaviour
{
    ObserveUI _observeUI;
    void Start()
    {
        _observeUI = Managers.UI.GetComponentInSceneUI<ObserveUI>();
    }
    void Update()
    {
        float old = _observeUI._currentTime;
        float cur = Mathf.Max(0f, old - Time.deltaTime);
        _observeUI._currentTime = cur;
       _observeUI.SetTimerText();
    }
}
