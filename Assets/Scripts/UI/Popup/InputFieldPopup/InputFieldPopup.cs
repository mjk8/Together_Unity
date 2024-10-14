using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputFieldPopup : UI_popup
{
    protected abstract void Submit();
    protected void Init<T>() where T: InputFieldPopup 
    {
        transform.GetChild(0).GetComponent<UI_Button>().SetOnClick(ClosePopup);
        transform.GetChild(1).GetComponent<UI_Text>().SetString(typeof(T).Name);
        transform.GetChild(3).GetComponent<UI_Button>().SetOnClick(Submit);
        transform.GetChild(3).GetChild(0).GetComponentInChildren<UI_Text>().SetString("Submit");
    }
}
