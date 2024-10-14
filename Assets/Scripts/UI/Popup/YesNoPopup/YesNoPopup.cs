using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class YesNoPopup : UI_popup
{
    protected abstract void YesFunc();
    protected abstract void NoFunc();

    protected GameObject popup;
    
    protected  void Init<T>() where T: YesNoPopup
    {
        transform.Find("View/DescriptionText").GetComponent<UI_Text>().SetString(typeof(T).Name);
    }
}
