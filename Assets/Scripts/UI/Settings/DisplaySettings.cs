using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySettings : MonoBehaviour
{
    SettingsPopup _settingsPopup;
    private List<string> resolutions;
    private UI_Toggle _isFullScreenToggle;
    UI_Dropdown _displayQualityDropdown;
    UI_Dropdown _resolutionDropdown;
    
    void Start()
    {
        _settingsPopup = GetComponentInParent<SettingsPopup>();
        GetComponent<VerticalLayoutGroup>().spacing = Screen.height / 150;
        resolutions = new List<string>();
        Screen.resolutions.ToList().ForEach(x=>
        {
            resolutions.Add(ResolutionToString(x));
        });
        _isFullScreenToggle = Managers.Resource.Instantiate("UI/Subitem/UI_Toggle", transform).GetComponent<UI_Toggle>();
        _isFullScreenToggle.Bind("isFullScreen",OnFullScreenChanged,SettingsPopup.currentData.isFullScreen);
        
        _displayQualityDropdown = Managers.Resource.Instantiate("UI/Subitem/UI_Dropdown", transform).GetComponent<UI_Dropdown>();
        _displayQualityDropdown.Bind("DisplayQuality",OnQualityIndexChanged,Util.EnumToStringList<Define.DisplayQuality>(),(int)SettingsPopup.currentData.DisplayQuality);
        
        _resolutionDropdown = Managers.Resource.Instantiate("UI/Subitem/UI_Dropdown", transform).GetComponent<UI_Dropdown>();
        _resolutionDropdown.Bind("MyResolution",OnResolutionChanged,resolutions,resolutions.FindIndex(SettingsPopup.currentData.MyResolution.ToDisplayString().Contains));
    }

    void OnFullScreenChanged()
    {
        bool value = _isFullScreenToggle.transform.GetComponentInChildren<Toggle>().isOn;
        SettingsPopup.currentData.isFullScreen = value;
        SetResolution();
        _settingsPopup.ChangeVal(Define.Settings.Display,"isFullScreen");
    }

    void OnQualityIndexChanged()
    {
        int value = _displayQualityDropdown.transform.GetComponentInChildren<TMP_Dropdown>().value;
        SettingsPopup.currentData.DisplayQuality = Util.GetEnumByIndex<Define.DisplayQuality>(value);
        SetQualityLevel(SettingsPopup.currentData.DisplayQuality);
        _settingsPopup.ChangeVal(Define.Settings.Display,"DisplayQuality");
    }

    void OnResolutionChanged()
    {
        /*dropdown.RefreshShownValue();
        int value = dropdown.value;*/
        Resolution myRes = Screen.resolutions[_resolutionDropdown.transform.GetComponentInChildren<TMP_Dropdown>().value];
        SettingsPopup.currentData.MyResolution.width = myRes.width;
        SettingsPopup.currentData.MyResolution.height = myRes.height;
        SetResolution();
        _settingsPopup.ChangeVal(Define.Settings.Display,"MyResolution");
    }
    
    public static void SetQualityLevel(Define.DisplayQuality value)
    {
        switch (value)
        {
            case Define.DisplayQuality.Low:
                QualitySettings.SetQualityLevel(0, true);
                break;
            case Define.DisplayQuality.Medium:
                QualitySettings.SetQualityLevel(3, true);
                break;
            case Define.DisplayQuality.High:
                QualitySettings.SetQualityLevel(5, true);
                break;
        }
    }


    public static string ResolutionToString(Resolution resolution)
    {
        return resolution.width +"x"+resolution.height;
    }

    private void SetResolution()
    {
        Screen.SetResolution(SettingsPopup.currentData.MyResolution.width,SettingsPopup.currentData.MyResolution.height,SettingsPopup.currentData.isFullScreen?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
    }
}
