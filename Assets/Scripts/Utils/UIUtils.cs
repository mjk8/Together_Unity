using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class UIUtils
{
    public static void BindClassToUISlider<T>(T classToBind, Action<GameObject> OnSliderValueChanged,
        Transform transform)
    {
        foreach (var current in classToBind.GetType().GetFields().ToList())
        {
            GameObject go = Managers.Resource.Instantiate("UI/Subitem/UI_Slider", transform);
            go.name = current.Name;
            go.transform.GetChild(0).GetComponent<LocalizeStringEvent>().StringReference
                .SetReference("StringTable", current.Name);
            string val = current.GetValue(classToBind).ToString();
            go.transform.GetChild(1).GetComponent<Slider>().value = float.Parse(val);
            go.transform.GetChild(1).GetComponent<Slider>().onValueChanged.AddListener(delegate
            {
                OnSliderValueChanged(go);
            });
            go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = val;
        }
    }

    public static void BindFieldToUISlider<T>(T classToBind, string fieldName, Action<GameObject> OnSliderValueChanged,
        Transform transform)
    {
        GameObject go = Managers.Resource.Instantiate("UI/Subitem/UI_Slider", transform);
        go.name = fieldName;
        go.transform.GetChild(0).GetComponent<LocalizeStringEvent>().StringReference
            .SetReference("StringTable", fieldName);
        string val = Util.GetValueClassField(classToBind, fieldName).ToString();
        go.transform.GetChild(1).GetComponent<Slider>().value = float.Parse(val);
        go.transform.GetChild(1).GetComponent<Slider>().onValueChanged.AddListener(delegate
        {
            OnSliderValueChanged(go);
        });
        go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = val;
    }

    /*
    public static void BindFieldToUIToggle<T>(T classToBind, string fieldName, Action<GameObject> OnToggleValueChanged,
        Transform transform)
    {
        GameObject go = Managers.Resource.Instantiate("UI/Subitem/UI_Toggle", transform);
        go.name = fieldName;
        go.transform.GetChild(0).GetComponent<LocalizeStringEvent>().StringReference
            .SetReference("StringTable", fieldName);
        go.transform.GetChild(1).GetComponent<Toggle>().isOn =
            bool.Parse(Util.GetValueClassField(classToBind, fieldName).ToString());
        go.transform.GetChild(1).GetComponent<Toggle>().onValueChanged.AddListener(delegate
        {
            OnToggleValueChanged(go);
        });
    }*/
    
    public static void BindFieldToUIDropdown<T>(T classToBind, string fieldName, Action<TMP_Dropdown> OnToggleValueChanged,
        Transform transform, List<string> dropdownMenus)
    {
        GameObject go = Managers.Resource.Instantiate("UI/Subitem/UI_DropDown", transform);
        go.name = fieldName;
        TMP_Dropdown dropdown = go.transform.GetChild(1).GetComponent<TMP_Dropdown>();
        go.transform.GetChild(0).GetComponent<LocalizeStringEvent>().StringReference
            .SetReference("StringTable", fieldName);
        dropdown.AddOptions(dropdownMenus);
        if (classToBind is string)
        {
            Debug.Log("string lol");
            dropdown.value = dropdownMenus.IndexOf(classToBind.ToString());
        }
        else
        {
            Debug.Log("not string lol");
            dropdown.value = dropdownMenus.IndexOf(Util.GetValueClassField(classToBind, fieldName).ToString());
        }
        go.transform.GetChild(1).GetComponent<TMP_Dropdown>().onValueChanged.AddListener(delegate
        {
            OnToggleValueChanged(dropdown);
        });
    }

    public static GameObject BindUIButtonWithText(string buttonName, Action OnButtonPress, Transform transform)
    {
        GameObject go = Managers.Resource.Instantiate("UI/Subitem/UI_Button", transform);
        go.name = buttonName;
        go.transform.GetChild(0).GetComponent<LocalizeStringEvent>().StringReference
            .SetReference("StringTable", buttonName);
        go.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnButtonPress();
        });
        return go;
    }
    
    public static void BindUIButtonNoText(string path, Action<GameObject> OnButtonPress, Transform transform)
    {
        GameObject go = Managers.Resource.Instantiate($"UI/{path}", transform);
        go.GetComponent<Button>().onClick.AddListener(delegate
        {
            OnButtonPress(go);
        });
    }
}