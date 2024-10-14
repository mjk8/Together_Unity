using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InputField : MonoBehaviour
{
    public void SetPlaceHolder(string reference)
    {
        transform.GetChild(0).GetChild(0).GetComponent<UI_Text>().SetString(reference);
    }

    public string GetInputText()
    {
        return gameObject.GetComponent<TMP_InputField>().text;
    }

    public void SetInteractable(bool interactable)
    {
        gameObject.GetComponent<TMP_InputField>().interactable = interactable;
    }
}
