using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitGameYesNoPopup : YesNoPopup
{
    private void Start()
    {
        Init<QuitGameYesNoPopup>();
    }

    protected override void YesFunc()
    {
        Application.Quit();
    }
    
    protected override void NoFunc()
    {
        ClosePopup();
    }
}
