using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 모든 씬들의 상위 클래스. 씬들이 공통적으로 불러야하는 Init과 필요한 함수들을 가지고 있다.
/// </summary>
public abstract class BaseScene : MonoBehaviour
{
    void Awake()
	{
		Init();
	}

	protected virtual void Init()
    {
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
            Managers.Resource.Instantiate("UI/EventSystem").name = "@EventSystem";
    }

    public abstract void Clear();
}
