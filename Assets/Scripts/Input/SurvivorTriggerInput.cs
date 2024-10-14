using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivorTriggerInput : MonoBehaviour
{
    private ObjectInput _instance;
    private ObjectInput _objectInput { get { Init(); return _instance; } }

    private int _dediPlayerId;

    private void Init()
    {
        if (_instance == null)
        {
            _instance = transform.GetComponentInParent<ObjectInput>();
            _dediPlayerId = _instance.GetComponent<MyDediPlayer>() != null ? Managers.Player._myDediPlayerId : _instance.gameObject.GetComponent<OtherDediPlayer>().PlayerId;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Managers.Game._isDay) //³·
        {

        }
        else
        {

        }
    }
}
