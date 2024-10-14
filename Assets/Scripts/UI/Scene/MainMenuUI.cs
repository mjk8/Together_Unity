using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : UI_scene
{
    private enum Buttons
    {
        StartGame,
        Shop,
        Settings,
        EndGame
    }
    private void Start()
    {
        InitButtons<Buttons>(transform.GetChild(0).gameObject,true);
    }

    private void StartGame()
    {
       Managers.UI.LoadScenePanel(Define.SceneUIType.LobbyUI);
    }
    
    private void Shop()
    {
        //implement shop
    }
    
    private void Settings()
    {
        Managers.UI.LoadPopupPanel<SettingsPopup>(true);
    }
    
    private void EndGame()
    {
        Managers.UI.LoadPopupPanel<QuitGameYesNoPopup>();
    }
}
