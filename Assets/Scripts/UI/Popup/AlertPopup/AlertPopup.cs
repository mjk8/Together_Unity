using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertPopup : UI_popup
{
    protected void Init<T>() where T: AlertPopup
    {
        transform.GetChild(0).GetComponent<UI_Button>().SetOnClick(ClosePopup);
        transform.GetChild(1).GetComponent<UI_Text>().SetString(typeof(T).Name);
    }
}
