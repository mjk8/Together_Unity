using System;
using System.Collections;
using System.Collections.Generic;
using RainbowArt.CleanFlatUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Button : MonoBehaviour
{
    EventTrigger _eventTrigger; //포인터 이벤트를 위한 eventTrigger
    EventTrigger.Entry _pointerEnter; //Hover 시작 이벤트
    EventTrigger.Entry _pointerExit; //Hover 끝 이벤트
    
    /// <summary>
    /// Initial Hover, Click, String Display Init
    /// </summary>
    void Start()
    {
        //EventTrigger Init
        _eventTrigger = GetComponent<EventTrigger>();
        if (_eventTrigger == null)
        {
            _eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        //Hover시 소리 재생
        SetOnHover(PlayButtonHover);
        
        //Click시 소리 재생
        SetOnClick(PlayButtonClick);
        
        //만약 Text가 있다면 Bind
        UI_Text text = GetComponentInChildren<UI_Text>();
        if (text != null)
        {
            text.SetString(gameObject.name);
        }
    }

    /// <summary>
    /// Hover시 재생할 function을 설정합니다.
    /// </summary>
    /// <param name="func"> hover시 실행될 func</param>
    public void SetOnHover(Action func)
    {
        if (_pointerEnter == null)
        {
            _pointerEnter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            }; 
        }
        _pointerEnter.callback.AddListener((data) => { PlayButtonHover(); });
    }
    
    /// <summary>
    /// Exit시 재생할 function을 설정합니다.
    /// </summary>
    /// <param name="func"> exit시 실행될 func</param>
    public void SetOnHoverExit(Action func)
    {
        if (_pointerExit == null)
        {
            _pointerExit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            }; 
        }
        _pointerExit.callback.AddListener((data) => { PlayButtonHover(); });
    }

    public void PlayButtonClick()
    {
        Managers.Sound.Play("Paper");
    }

    public void PlayButtonHover()
    {
        Managers.Sound.Play("ButtonClick");
    }
    
    public void SetOnClick(Action func)
    {
        gameObject.GetComponent<Button>().onClick.AddListener(delegate { func(); });
    }

    public void SetButtonActivation(bool activate)
    {
        gameObject.GetComponent<Button>().interactable = activate;
        ButtonTransition transition = GetComponent<ButtonTransition>();
        if (transition != null)
        {
            if (!activate)
            {
                transition.Unhighlight();
            }
            transition.enabled = activate;
        }
    }
    
    public void RemoveAllOnClick()
    {
        gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
    }
}