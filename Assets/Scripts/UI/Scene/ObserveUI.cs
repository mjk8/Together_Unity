using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ObserveUI : UI_scene
{
    public GameObject _timerText;
    public GameObject _observingPlayerName;
    public GameObject _leftButton;
    public GameObject _rightButton;
    public GameObject _quitButton;

    public float _currentTime;
    private int _currentlyObservingPlayerID;
    
    DeadTimerCountdownActivator _timerCountdownActivator;

    private GameObject _camera;
    
    private void Awake()
    {
        //추후 접근이 필요한 오브젝트들을 찾아서 저장
        _timerText = transform.Find("TimerText").gameObject;
        _observingPlayerName = transform.Find("ObservingPlayerName").gameObject;
        _leftButton = transform.Find("LeftButton").gameObject;
        _rightButton = transform.Find("RightButton").gameObject;
        _quitButton = transform.Find("QuitButton").gameObject;
        _camera = GameObject.Find("Main Camera");
        
        //OtherDediPlayer에 첫 플레이어부터 시작하도록 설정
        _currentlyObservingPlayerID = Managers.Player._otherDediPlayers.Keys.FirstOrDefault();
        ObserveChanged();
        
        //버튼 설정
        _quitButton.GetComponent<UI_Button>().SetOnClick(ReturnToLobby);
        _leftButton.GetComponent<UI_Button>().SetOnClick(LeftButtonClicked);
        _rightButton.GetComponent<UI_Button>().SetOnClick(RightButtonClicked);
    }
    
    

    private void ObserveChanged()
    {
        SetObservingPlayerName();
        GameObject cur = Managers.Player._otherDediPlayers[_currentlyObservingPlayerID];
        _camera.transform.SetParent(cur.transform.Find("CameraPos"));
        _camera.transform.localPosition = Vector3.zero;
        _camera.transform.localRotation = Quaternion.identity;
        Debug.Log("Currently observing: "+ _currentlyObservingPlayerID);
    }
    
    private void ReturnToLobby()
    {
        Managers.Scene.EndGameAndReturnToLobby();
    }
    
    private void SetObservingPlayerName()
    {
        _observingPlayerName.GetComponent<TMP_Text>().text = Managers.Player._otherDediPlayers[_currentlyObservingPlayerID].GetComponent<OtherDediPlayer>().Name;
    }

    private void RightButtonClicked()
    {
        var keys = Managers.Player._otherDediPlayers.Keys.ToList();
        int currentIndex = keys.IndexOf(_currentlyObservingPlayerID);
        int nextIndex = (currentIndex + 1) % keys.Count;
        _currentlyObservingPlayerID = keys[nextIndex];
        ObserveChanged();
    }
    
    private void LeftButtonClicked()
    {
        var keys = Managers.Player._otherDediPlayers.Keys.ToList();
        int currentIndex = keys.IndexOf(_currentlyObservingPlayerID);
        int previousIndex = (currentIndex - 1 + keys.Count) % keys.Count;
        _currentlyObservingPlayerID = keys[previousIndex];
        ObserveChanged();
    }
    
    public void ChangeIfObservingThisPlayer(int playerID)
    {
        if (_currentlyObservingPlayerID == playerID)
        {
            Debug.Log("Change observing player");
            RightButtonClicked();
        }
    }

    public void InitObserveTimer(float time)
    {
        _currentTime = time;
        SetTimerText();
        if (_timerCountdownActivator == null)
        {
            _timerCountdownActivator = Util.GetOrAddComponent<DeadTimerCountdownActivator>(transform.gameObject);
        }
    }

    public void EndTimer()
    {
        Destroy(_timerCountdownActivator);
        _currentTime = 0;
        SetTimerText();
    }
    
    public void SetTimerText()
    {
        Debug.Log(_currentTime);
        _timerText.GetComponent<TMP_Text>().text = Mathf.RoundToInt(_currentTime).ToString();
    }
}
