using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTriggerInput : MonoBehaviour
{
    private ObjectInput _instance;
    private ObjectInput _objectInput { get { Init(); return _instance; } }

    private void Init()
    {
        if (_instance == null)
        {
            _instance = Managers.Player._myDediPlayer.GetComponent<ObjectInput>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
        if (Managers.Game._isDay)//낮
        {
            //상자 트리거 처리
            if (other.CompareTag("Chest") && !other.transform.parent.GetComponent<Chest>()._isOpened)
            {
                _objectInput.ChangeHighlightChest(other.transform.parent.gameObject);
            }
        }
        else//밤
        {
            //생존자일때
            if (!Managers.Player.IsMyDediPlayerKiller())
            {
                //클렌즈 처리
                if (other.CompareTag("Cleanse") && other.transform.parent.GetComponent<Cleanse>()._isAvailable)
                {
                    _objectInput._currentCleanse = other.transform.parent.gameObject;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Chest" && _objectInput._currentChest.Equals(other.transform.parent.gameObject))
        {
            _objectInput._currentChest.GetComponent<Chest>().UnHighlightChest();
            _objectInput._currentChest = null;
            Managers.UI.ClosePopup();
        }
        else if (other.tag == "Cleanse")
        {
            _objectInput.QuitCleansing();
            _objectInput._currentCleanse = null;
        }
    }
}
