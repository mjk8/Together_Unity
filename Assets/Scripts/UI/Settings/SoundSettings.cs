using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

[System.Serializable]
public class SoundSettingComponent
{
    //haven't found a way to link with Define.Sound... 
    //Make sure field names are the same as those defined in Define.Sound
    public float Master;
    public float Bgm;
    public float Effects;

    public SoundSettingComponent()
    {
        Master = 100;
        Bgm = 100;
        Effects = 100;
    }
}

public class SoundSettings : MonoBehaviour
{
    public static SoundSettingComponent _soundSetting;
    void Start()
    {
        _soundSetting = new SoundSettingComponent();
        _soundSetting = Managers.Data.LoadFromJson<SoundSettingComponent>(Define.SaveFiles.Sound, _soundSetting);
        UpdateVolume("Master");
        UIUtils.BindClassToUISlider(_soundSetting,OnSliderValueChanged,transform);
    }

    void OnSliderValueChanged(GameObject go)
    {
        float currentValue = go.transform.GetChild(1).GetComponent<Slider>().value;
        _soundSetting.GetType().GetField(go.name).SetValue(_soundSetting,currentValue);
        go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = currentValue.ToString();
        UpdateVolume(_soundSetting.GetType().GetField(go.name).Name);
    }

    private void UpdateVolume(string name)
    {
        if (name == "Master")
        {
            Managers.Sound.ChangeAudioVolume(Define.Sound.Bgm, _soundSetting.Master * _soundSetting.Bgm / 10000);
            Managers.Sound.ChangeAudioVolume(Define.Sound.Effects, _soundSetting.Master * _soundSetting.Effects / 10000);
        }
        else
        {
            Define.Sound type = (Define.Sound)Enum.Parse(typeof(Define.Sound), name);
            float volume = float.Parse(_soundSetting.GetType().GetField(name).GetValue(_soundSetting).ToString()) /
                           100;
            volume *= _soundSetting.Master / 100;
            Managers.Sound.ChangeAudioVolume(type, volume);
        }
    }

    public void SaveChanges()
    {
        Managers.Data.SaveToJson<SoundSettingComponent>(Define.SaveFiles.Sound,_soundSetting);
    }
}
