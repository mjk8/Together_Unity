using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Toggle : MonoBehaviour
{
    private Toggle toggle;
    private void Awake()
    {
        toggle = gameObject.GetComponentInChildren<Toggle>();
    }
    public void Bind(string stringTable,Action onToggleChanged, bool initVal)
    {
        if(stringTable != null)
        {
            gameObject.name = stringTable;
            gameObject.GetComponentInChildren<UI_Text>().SetString(stringTable);
        }
        SetToggleState(initVal);
        SetOnClick(onToggleChanged);
    }
    public void SetToggleState(bool state)
    {
        toggle.isOn = state;
    }

    public bool GetToggleState()
    {
        return toggle.isOn;
    }
    
    public void SetOnClick(Action func)
    {
        toggle.onValueChanged.AddListener(delegate { func(); });
    }
}
