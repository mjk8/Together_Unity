using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MyKillerSkill : MonoBehaviour
{
    InGameUI _inGameUI;
    float _currentCoolTime;
    public void Init()
    {
        _inGameUI = Managers.UI.GetComponentInSceneUI<InGameUI>();
        //random init value so that the skill is ready to use
        _inGameUI.SetSkillMaxValue(10f);
        _inGameUI.SetSkillCurrentValue(10f);
    }
    
    public void UsedSkill(float skillCoolTimeSeconds, float skillUseTime = 0f)
    {
        if (_inGameUI == null)
        {
            return;
        }
        if (skillUseTime > 0)
        {
            _currentCoolTime = skillUseTime;
            _inGameUI.SetSkillMaxValue(skillUseTime);
            _inGameUI.SetSkillCurrentValue(skillUseTime);
            StartCoroutine(DecreaseUseSkillTime(skillUseTime,skillCoolTimeSeconds));
        }
        else
        {
            StartCoroutine(IncreaseCoolTime(skillCoolTimeSeconds));
        }
    }

    IEnumerator DecreaseUseSkillTime(float skillUseTime,float skillCoolTimeSeconds)
    {
        while (_currentCoolTime > 0)
        {
            _currentCoolTime -= Time.deltaTime;
            _inGameUI.SetSkillCurrentValue(_currentCoolTime);
            yield return null;
        }
        StartCoroutine(IncreaseCoolTime(skillCoolTimeSeconds));
    }

    IEnumerator IncreaseCoolTime(float skillCoolTimeSeconds)
    {
        _inGameUI.SetSkillCurrentColor(false);
        _currentCoolTime = 0;
        _inGameUI.SetSkillMaxValue(skillCoolTimeSeconds);
        _inGameUI.SetSkillCurrentValue(0);
        while (_currentCoolTime < skillCoolTimeSeconds)
        {
            _currentCoolTime += Time.deltaTime;
            _inGameUI.SetSkillCurrentValue(_currentCoolTime);
            yield return null;
        }
        _inGameUI.SetSkillCurrentColor(true);
    }
}
