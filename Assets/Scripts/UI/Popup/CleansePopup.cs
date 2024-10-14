using System;
using System.Collections;
using System.Collections.Generic;
using RainbowArt.CleanFlatUI;
using UnityEngine;

public class CleansePopup : UI_popup
{
    ProgressBar _progressBar;
    Cleanse _currentCleanse;
    void Start()
    {
        _currentCleanse = Managers.Object._cleanseController._myPlayerCurrentCleanse;
        _progressBar = transform.Find("Gauge").GetComponent<ProgressBar>();
        _progressBar.MaxValue = _currentCleanse._cleanseDurationSeconds;
        _progressBar.CurrentValue = 0f;
    }

    private void Update()
    {
        _progressBar.CurrentValue += Time.deltaTime;
        _currentCleanse.CurrentlyCleansing(_progressBar.CurrentValue);
    }
}
