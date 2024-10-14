using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadUI : UI_scene
{
    public GameObject _lobbyButton;
    public GameObject _observeButton;
    
    private enum Buttons
    {
        LobbyButton,
        ObserveButton
    }
    void Awake()
    {
        InitButtons<Buttons>(gameObject);
    }
    
    private void LobbyButton()
    {
        Managers.Scene.EndGameAndReturnToLobby();
    }
    
    private void ObserveButton()
    {
        float currentTime = Managers.Game._clientTimer._clientTimerValue;
        Managers.UI.LoadScenePanel(Define.SceneUIType.ObserveUI);
        Managers.UI.GetComponentInSceneUI<ObserveUI>().InitObserveTimer(currentTime);
    }
}
