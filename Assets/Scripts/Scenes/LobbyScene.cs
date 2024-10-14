using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        Managers.UI.LoadScenePanel(Define.SceneUIType.MainMenuUI);
        
        Managers.Sound.Play("MainMenuMusic",Define.Sound.Bgm);
    }
    
    

    public override void Clear()
    {
        
    }
}