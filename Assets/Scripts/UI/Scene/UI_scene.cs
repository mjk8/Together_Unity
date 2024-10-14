using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 모든 UI_scene의 상위 클래스.
/// </summary>
public abstract class UI_scene : UI_base
{
    

    public static void InstantiateSceneUI(Define.SceneUIType sceneUIType)
    {
        Managers.UI.LoadScenePanel(sceneUIType);
    }

    /// <summary>
    /// 씬 안에 모든 버튼들을 해당 버튼의 함수와 매칭하는 함수. PrefabName = ButtonName = 함수명
    /// </summary>
    protected void InitButtons<T>(GameObject go,bool initiate = false) where T : Enum
    {
        foreach (string buttonName in Enum.GetNames(typeof(T)))
        {
            GameObject buttonGO;
            if (initiate)
            {
                buttonGO = Managers.Resource.Instantiate("UI/Subitem/UI_Button", go.transform);
                buttonGO.name = buttonName;
            }
            else
            {
                buttonGO = go.transform.Find(buttonName).gameObject;
            }

            GameObject localText = Util.FindChild(buttonGO, "LocalizationText", true);

            if (localText != null)
            {
                localText.GetComponent<UI_Text>().SetString(buttonName);
            }

            buttonGO.GetComponent<UI_Button>().SetOnClick(funcToRun(buttonName));
        }
    }

    protected Action funcToRun(string func)
    {
        return delegate { Invoke(func, 0f); };
    }
}