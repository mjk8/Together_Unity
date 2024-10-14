using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class SettingsPopup : UI_popup
{
    Define.Settings currentTab = Define.Settings.Display;
    private Transform resetText;
    public bool isHoveringReset = false;
    public static PlayerData currentData;
    private UI_Button resetButton;
    private UI_Button saveButton;
    private static bool[] _changeInSetting = { false, false, false, false };

    void Awake()
    {
        //현재의 플레이어 데이터를 불러오기.
        currentData = new PlayerData(Managers.Data.Player);
        
        //resetText Binding
        resetText = transform.Find("ResetText");
        
        //Close Icon Button Binding
        transform.Find("Operations/CloseIcon").GetComponent<UI_Button>().SetOnClick(ClosePopup);

        //Reset button
        resetButton = transform.Find("Operations/Reset").GetComponent<UI_Button>();
        resetButton.SetOnClick(InitResetButton);
        resetButton.SetOnHover(OnHoverEnter);
        resetButton.SetOnHoverExit(OnHoverExit);
        OnHoverExit();
        
        //Save button
        saveButton = transform.Find("Operations/Save").GetComponent<UI_Button>();
        saveButton.SetOnClick(SaveCurrentData);
        CheckActivate();
        
        //Navigation Buttons
        foreach (Define.Settings i in System.Enum.GetValues(typeof(Define.Settings)))
        {
            transform.Find("Navigation").GetChild((int)i).GetComponent<UI_Button>().SetOnClick(()=>NavButton(i));
        }
        NavButton(currentTab);
    }
    
    void Update()
    {
        if (isHoveringReset)
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform, 
                Input.mousePosition, 
                null, 
                out mousePos
            );
            resetText.position = mousePos;
        }
    }

    #region SettingButtons
    public void InitResetButton()
    {
        currentData = new PlayerData(Managers.Data.Player);
    }

    public void OnHoverEnter()
    {
        isHoveringReset = true;
        resetText.gameObject.SetActive(true);
    }
    
    public void OnHoverExit()
    {
        isHoveringReset = false;
        resetText.gameObject.SetActive(false);
    }

    public void CheckActivate()
    {
        saveButton.SetButtonActivation(_changeInSetting.Any(x => x));
        resetButton.SetButtonActivation(_changeInSetting.Any(x => x));
    }

    public void NavButton(Define.Settings current)
    {
        for (int i =0;i<transform.Find("Navigation").childCount;i++)
        {
            transform.Find("Navigation").GetChild(i).GetComponent<UI_Button>().SetButtonActivation(i != (int)current);
            transform.Find("Panels").GetChild(i).gameObject.SetActive(i == (int)current);
        }
    }
    #endregion

    public void SaveCurrentData()
    {
        Managers.Data.SaveToJson<PlayerData>(Define.SaveFiles.Player,currentData);
        Managers.Data.Player = currentData;
    }

    public void ChangeVal(Define.Settings settingType, string fieldName)
    {
        bool changed = currentData.GetType().GetField(fieldName)
            .Equals(Managers.Data.Player.GetType().GetField(fieldName));
        _changeInSetting[(int)settingType] = changed;
        CheckActivate();
    }
}
