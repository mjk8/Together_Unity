using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InGameDialogue : UI_popup
{ 
    protected void Init<T>() where T: InGameDialogue
    {
        transform.Find("InGameText").GetComponent<UI_Text>().SetString(typeof(T).Name);
    }
}
