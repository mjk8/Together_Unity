using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 팝업들의 상위 클래스.
/// </summary>
public class UI_popup : UI_base
{
    //모든 popup type을 들고 있는 enum. 새로운걸 만들 때 여기에 업데이트.
    public enum PopupType
    {
        YesNoPopup,
        Settings,
        CreateRoom,
        Alert,
        ProgressPopup,
        SettingsPopup,
        WairForSecondsPopup,
        InGameDialogue,
        CleansePopup,
        DayToNightPopup,
        NightIsOverPopup,
        BlurryBackgroundPopup
    }
    
    protected void ClosePopup()
    {
        Managers.UI.ClosePopup(gameObject);
    }
}
