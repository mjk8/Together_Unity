using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ControlSetting : MonoBehaviour
{
    void Start()
    {
        UIUtils.BindFieldToUISlider(Managers.Data.Player,"MouseSensitivity",OnSliderValueChanged,transform);
    }

    void OnSliderValueChanged(GameObject go)
    {
        float currentValue = go.transform.GetChild(1).GetComponent<Slider>().value;
        Managers.Data.Player.MouseSensitivity = currentValue;
        //Managers.Data.Player.GetType().GetField(go.name).SetValue(Managers.Data.Player,currentValue);
        go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = currentValue.ToString();
        CameraMovement.MouseSensitivityChanged(Managers.Data.Player.MouseSensitivity);
    }
    
    public void SaveChanges()
    {
        Managers.Data.SaveToJson<PlayerData>(Define.SaveFiles.Player,Managers.Data.Player);
    }
}
