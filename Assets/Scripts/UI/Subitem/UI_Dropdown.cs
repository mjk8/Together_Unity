using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Dropdown : MonoBehaviour
{
    TMP_Dropdown dropdown;
    private void Awake()
    {
        dropdown = gameObject.GetComponentInChildren<TMP_Dropdown>();
    }
    
    public void Bind(string stringTable,Action func,List<string> options,int initVal=0)
    {
        if(stringTable != null)
        {
            gameObject.name = stringTable;
            gameObject.GetComponentInChildren<UI_Text>().SetString(stringTable);
        }
        SetOptions(initVal,options);
        SetOnValueChanged(func);
    }
    
    public void SetOptions(int initVal, List<string> options)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = initVal;
    }
    
    public void SetOnValueChanged(Action func)
    {
        dropdown.onValueChanged.AddListener(delegate { func(); });
    }
}
