using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// 인게임의 모든 UI를 관리하는 매니져
/// </summary>
public class UIManager
{
    public static int order = 0;
    LinkedList<GameObject> _popupLinkedList = new LinkedList<GameObject>();
    Stack<GameObject> _firstSelected = new Stack<GameObject>();

    public static GameObject root;

    public GameObject Root { get { return root; } }
    
    public static GameObject sceneUI;
    
    public GameObject SceneUI { get { return sceneUI; } }
    
    /// <summary>
    /// UI들의 Parent가 될 Root가 없으면 생성.
    /// </summary>
    public void Init()
    {
        root = GameObject.Find("@UI_Root");
        if (root == null)
        {
            root = new GameObject { name = "@UI_Root" };
            Object.DontDestroyOnLoad(root);
            Canvas canvas = Util.GetOrAddComponent<Canvas>(root);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler canvasScaler = Util.GetOrAddComponent<CanvasScaler>(root);
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            root.AddComponent<GraphicRaycaster>();
        }
    }
    
    public void ChangeCanvasRenderMode(RenderMode renderMode)
    {
        root.GetComponent<Canvas>().renderMode = renderMode;
        if (renderMode == RenderMode.ScreenSpaceCamera)
        {
            if (Managers.Game._isDay)
            {
                root.GetComponent<Canvas>().worldCamera =
                    GameObject.Find("KillerChangeGO/RenderCamera").GetComponent<Camera>();
            }
            else
            {
                root.GetComponent<Canvas>().worldCamera =
                    GameObject.Find("DeadPlayerGO/RenderCamera").GetComponent<Camera>();
            }
        }
    }
    
    /// <summary>
    /// 씬패널 로드 함수 (씬!=Unity Scene. 같은 로비 씬 내에서도 로비, 룸 등 여러 씬패널이 있다)
    /// *** ScenePanel Prefab명과 동일한 SceneUI 클래스가 존재해야한다. 그 클래스가 씬패널의 기능을 담당한다.***
    /// 씬 패널은 하나만 존재한다. 다른 씬패널이 불려오면 기존 씬패널은 Destroy 된다.
    /// </summary>
    public void LoadScenePanel(Define.SceneUIType sceneUIType)
    {
        if (sceneUI != null)
        {
            Managers.Resource.Destroy(sceneUI);
        }

        sceneUI = Managers.Resource.Instantiate($"UI/Scene/{sceneUIType.ToString()}", root.transform);
        sceneUI.AddComponent(Type.GetType(sceneUIType.ToString())); //동일한 이름의 클래스 부착
    }
    
    /// <summary>
    /// 팝업을 생성한다.
    /// *** 생성하는 Popup Prefab명과 동일한 popup 클래스가 존재해야한다. 그 클래스가 해당 팝업의 기능을 담당한다.***
    /// </summary>
    /// <param name="isBase">단독 popup이면 true, 상위 팝업 틀이 존재하면 false</param>
    /// <param name="popupInteractableOnly">해당 팝업만 interactable하면 true, 다른 팝업 밑 ui 패널들도 interactable하면 false</param>

    public T LoadPopupPanel<T>(bool isBase = false,bool popupInteractableOnly = true) where T: UI_popup
    {
        GameObject popup;
        
        //만약 해당 popup만 interactable하면 다른 팝업과의 interaction을 막기 위해 뒤에 패널 하나를 생성
        if (popupInteractableOnly)
        {
            _popupLinkedList.AddLast(Managers.Resource.Instantiate($"UI/Subitem/Panel", root.transform));
        }
        
        if (isBase)
        {
            popup =  Managers.Resource.Instantiate($"UI/Popup/{typeof(T)}",root.transform);
        }
        else
        {
            popup = Managers.Resource.Instantiate($"UI/Popup/{typeof(T).BaseType}",root.transform);
        }
        
        _popupLinkedList.AddLast(popup);
            
        popup.AddComponent(typeof(T)); //동일한 이름의 클래스 부착
        return popup.GetComponent<T>();
    }

    /// <summary>
    /// 제일 상위 팝업을 닫는다.
    /// *** 만약 popupinteractableonly 팝업으로 뒤에 패널이 같이 생성됐다면 그 패널도 destroy 한다.***
    /// </summary>
    public void ClosePopup(GameObject gameObject = null)
    {
        if (!PopupActive())
            return;
        if (gameObject == null)
        {
            Managers.Resource.Destroy(_popupLinkedList.Last.Value);
            _popupLinkedList.RemoveLast();
            if (_popupLinkedList.Count>0 && _popupLinkedList.Last.Value.name == "Panel")
            {
                Managers.Resource.Destroy(_popupLinkedList.Last.Value);
                _popupLinkedList.RemoveLast();
            }
        }
        else
        {
            var cur = _popupLinkedList.Find(gameObject);

            if (cur == null)
            {
                return;
            }
            if (_popupLinkedList.Count>1 && cur.Previous.Value.name == "Panel")
            {
                Managers.Resource.Destroy(cur.Previous.Value);
                _popupLinkedList.Remove(cur.Previous);
            }

            Managers.Resource.Destroy(_popupLinkedList.Find(gameObject).Value);
            _popupLinkedList.Remove(cur);
        }
    }

    public bool PopupActive()
    {
        return (_popupLinkedList.Count > 0);
    }
    
    /// <summary>
    /// 팝업이 활성화되어있는지 확인하고 리턴한다.
    /// </summary>
    /// <param name="popupName"></param>
    /// <returns></returns>
    public GameObject CheckPopupActive(string popupName)
    {
        foreach(var cur in _popupLinkedList)
        {
            if (cur.name == popupName)
                return cur;
        }
        return null;
    }

    public void SetEventSystemNavigation(GameObject go)
    {
        EventSystem.current.firstSelectedGameObject = go;
    }

    /// <summary>
    /// 모든 UI를 삭제한다.
    /// </summary>
    public void Clear()
    {
        if (sceneUI != null)
        {
            Managers.Resource.Destroy(sceneUI);
        }
        CloseAllPopup();
    }
    
    /// <summary>
    /// 모든 Popup을 삭제한다.
    /// </summary>
    public void CloseAllPopup()
    {
        foreach (Transform child in root.transform)
        {
            if (child.gameObject != sceneUI)
            {
                Managers.Resource.Destroy(child.gameObject);
            }
        }
        _popupLinkedList.Clear();
    }
    
    public T GetComponentInSceneUI<T>(string childName = null) where T : MonoBehaviour
    {
        if(childName == null)
            return sceneUI.GetComponent<T>();
        return sceneUI.transform.Find(childName).GetComponent<T>();
    }
    
    public T GetComponentInPopup<T>(string popupName = null) where T : MonoBehaviour
    {
        Debug.Log( _popupLinkedList.Last.Value.name);
        return _popupLinkedList.Last.Value.GetComponent<T>();
    }
    
    
    /*
    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : MonoBehaviour
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        GameObject go = Managers.Resource.Instantiate($"UI/subitem/{name}");
        if (parent != null)
        {
            go.transform.SetParent(parent);
        }

        return Util.GetOrAddComponent<T>(go);
    }
    
	public T ShowSceneUI<T>(string name = null) where T : UI_Scene
	{
		if (string.IsNullOrEmpty(name))
			name = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
		T sceneUI = Util.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;

		go.transform.SetParent(Root.transform);

		return sceneUI;
	}

	public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        T popup = Util.GetOrAddComponent<T>(go);
        _popupStack.Push(popup);

        go.transform.SetParent(Root.transform);

		return popup;
    }

    public void ClosePopupUI(UI_Popup popup)
    {
		if (_popupStack.Count == 0)
			return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!");
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
        _order--;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public void Clear()
    {
        CloseAllPopupUI();
        _sceneUI = null;
    }
    */
}