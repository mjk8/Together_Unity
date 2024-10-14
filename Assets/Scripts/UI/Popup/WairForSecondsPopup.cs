using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 카운트 다운 팝업 (전체화면) 현재로써는 3초 고정.
/// </summary>
public class WairForSecondsPopup : UI_popup
{
    IEnumerator Start()
    {
        transform.GetComponent<Image>().color= new Color(4F,4F,4F,255);
        for (int i = 3; i > 0; i--)
        {
            transform.Find("Seconds").GetComponent<TMP_Text>().text = i.ToString();
            yield return new WaitForSeconds(1);
        }
    }

    public void GameStart()
    {
        transform.GetComponent<Image>().color= new Color(0,0,0,0);
        transform.Find("Seconds").GetComponent<TMP_Text>().text = "START!";
        
        Managers.Sound.Play("Start!");
        Managers.UI.CloseAllPopup();
    }
}
