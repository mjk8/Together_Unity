using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AddCoinEffect : MonoBehaviour
{
    private static float _duration = 1.68f;
    
    private float _time;
    private TMP_Text _tmpText;
    private Vector3 _originalPos;
    private float _finalUpLen;

    public void Init(int coinAdded,bool isAddCoin = true)
    {
        _originalPos = transform.position;
        _tmpText = GetComponent<TMP_Text>();
        _tmpText.alpha = 0;
        _time = 0;
        _finalUpLen = Screen.height * 0.03f;
        if(isAddCoin)
        {
            _tmpText.text = String.Concat("+", coinAdded.ToString());
        }
        else
        {
            _tmpText.text = String.Concat("-", coinAdded.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        _tmpText.alpha = Mathf.Sin(_time/_duration*Mathf.PI);
        transform.position = _originalPos + ( _time/_duration * _finalUpLen * Vector3.up);
        if (_time >= _duration)
        {
            _tmpText.transform.position = _originalPos;
            gameObject.SetActive(false);
            Destroy(this);
        }
    }
}
