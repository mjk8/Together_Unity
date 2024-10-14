using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LobbyInput : MonoBehaviour
{
    void OnCancel()
    {
        if (Managers.UI.PopupActive())
        {
            Managers.UI.ClosePopup();
        }
    }
}