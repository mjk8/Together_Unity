using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaugeActivator : MonoBehaviour
{
    InGameUI _inGameUI;
    void Start()
    {
        _inGameUI = Managers.UI.GetComponentInSceneUI<InGameUI>();
    }
    void Update()
    {
        if (_inGameUI != null)
        {
            Managers.Game._clientGauge.DecreaseAllGaugeAuto();
            _inGameUI.SetCurrentGauge();
        }
    }
}
