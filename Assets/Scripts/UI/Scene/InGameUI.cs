using System;
using System.Collections;
using System.Collections.Generic;
using RainbowArt.CleanFlatUI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InGameUI : UI_scene
{
    public GameObject _timer;
    public GameObject _gauge;
    public GameObject _coin;
    public GameObject _coinCollect;
    public GameObject _killerSkill;
    public GameObject _inventory;
    public GameObject _shop;
    public GameObject _hotbar;
    
    public bool _isInventoryOpen;
    private void Awake()
    {
        Managers.UI.LoadPopupPanel<WairForSecondsPopup>(true,false); //3초 카운트 다운
        _timer = transform.Find("Timer").gameObject;
        _gauge = transform.Find("Gauge").gameObject;
        _coin = transform.Find("Coin").gameObject;
        _coinCollect = transform.Find("CoinCollect").gameObject;
        _killerSkill = transform.Find("KillerSkill").gameObject;
        _inventory = transform.Find("Inventory").gameObject;
        _shop = transform.Find("Shop").gameObject;
        _hotbar = transform.Find("Hotbar").gameObject;
        _gauge.SetActive(false);
        _coinCollect.SetActive(false);
        _killerSkill.SetActive(false);
        Managers.Inventory.Init();
        CloseInventory();
    }
    
    Color _colorTimerDay = new Color(68f/ 255f,68f/ 255f,68f/ 255f);
    Color _colorTimerNight = new Color(210f/ 255f,4f/ 255f,45f/ 255f);

    public void ChangeDayNightUI()
    {
        bool isDay = Managers.Game._isDay;
        if (Managers.Game._isDay)
        {
            IsNotKiller();
            _timer.GetComponent<ProgressBar>().ChangeForeground(_colorTimerDay);
            if (_isInventoryOpen)
            {
                _shop.SetActive(true);
            }
        }
        else
        {
            _timer.GetComponent<ProgressBar>().ChangeForeground(_colorTimerNight);
            SetCurrentCoin(0);
            if (_isInventoryOpen)
            {
                _shop.SetActive(false);
            }
        }
        _gauge.SetActive(!isDay);
        _coin.SetActive(isDay);
    }
    
    public void OpenInventory()
    {
        _inventory.SetActive(true);
        if (Managers.Game._isDay)
        {
            _shop.SetActive(true);
        }
        _isInventoryOpen = true;
    }

    public void CloseInventory()
    {
        _shop.SetActive(false);
        _inventory.SetActive(false);
        _isInventoryOpen = false;
    }

    public void ChangeCurrentTimerValue(float value)
    {
        _timer.GetComponent<ProgressBar>().CurrentValue = value;
    }
    
    public void SetMaxTimerValue(float value)
    {
        _timer.GetComponent<ProgressBar>().MaxValue = value;
    }
    
    public void SetMaxGauge(float max)
    {
        _gauge.GetComponent<ProgressBar>().MaxValue =max;
        SetCurrentGauge();
    }
    
    public void SetCurrentGauge()
    {
        _gauge.GetComponent<ProgressBar>().CurrentValue = Managers.Game._clientGauge.GetMyGauge();
    }
    
    public void SetCurrentCoin(int coinTotal)
    {
        _coin.transform.Find("CoinText").GetComponent<TMP_Text>().text = coinTotal.ToString();
    }

    public void AddGetCoin(int coinAdded,bool isAddCoin = true)
    {
        _coinCollect.SetActive(true);
        _coinCollect.AddComponent<AddCoinEffect>().Init(coinAdded,isAddCoin);
    }

    public void SetSkillMaxValue(float skillCooltime)
    {
        _killerSkill.SetActive(true);
        _killerSkill.GetComponent<ProgressBar>().MaxValue = skillCooltime;
    }

    public void IsNotKiller()
    {
        _killerSkill.SetActive(false);
    }

    public void SetSkillCurrentValue(float value)
    {
        _killerSkill.GetComponent<ProgressBar>().CurrentValue = value;
    }
    
    public void SetSkillCurrentColor(bool canUseSKill)
    {
        if (canUseSKill)
        {
            _killerSkill.GetComponent<ProgressBar>().ChangeForeground(_colorTimerNight);
        }
        else
        {
            _killerSkill.GetComponent<ProgressBar>().ChangeForeground(_colorTimerDay);
        }
    }
}
