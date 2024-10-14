using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingNavigation : MonoBehaviour
{
    [SerializeField] private GameObject DisplaySettingsPanel;
    [SerializeField] private GameObject SoundSettingsPanel;
    [SerializeField] private GameObject ControlSettingsPanel;
    [SerializeField] private GameObject KeyBindingPanel;

    void Start()
    {
        ShowDisplaySetting();
    }

    public void ShowDisplaySetting()
    {
        TurnOffAll();
        DisplaySettingsPanel.SetActive(true);
    }
    
    public void ShowSoundSetting()
    {
        TurnOffAll();
        SoundSettingsPanel.SetActive(true);
    }
    
    public void ShowControlSetting()
    {
        TurnOffAll();
        ControlSettingsPanel.SetActive(true);
    }
    
    public void ShowKeyBindingSetting()
    {
        TurnOffAll();
        KeyBindingPanel.SetActive(true);
    }
    

    void TurnOffAll()
    {
        Managers.Sound.Play("test_effects");
        DisplaySettingsPanel.SetActive(false);
        SoundSettingsPanel.SetActive(false);
        ControlSettingsPanel.SetActive(false);
        KeyBindingPanel.SetActive(false);
    }
}
