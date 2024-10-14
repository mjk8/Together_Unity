using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameScene : BaseScene
{
    
    protected override void Init()
    {
        base.Init();
        Managers.Game.GameScene();
        Managers.Sound.Play("tense-horror-background",Define.Sound.Bgm);
        Managers.UI.LoadPopupPanel<ProgressPopup>(true,false);
    }


    public override void Clear()
    {
        
    }
}
