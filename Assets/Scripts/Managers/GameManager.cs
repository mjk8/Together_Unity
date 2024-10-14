using Google.Protobuf.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager
{
    public static GameObject root;
    public bool _isDay;
    
    public ClientTimer _clientTimer;
    public ClientGauge _clientGauge;
    public MyKillerSkill _myKillerSkill;

    private float _dokidokiStart = 20;
    private float _dokidokiClose = 15;
    private float _dokidokiExtreme = 10;

    //Managers Init과 함께 불리는 Init
    public void Init()
    {
        root = GameObject.Find("@Game");
        if (root == null)
        {
            root = new GameObject { name = "@Game" };
            Object.DontDestroyOnLoad(root);
        }
    }

    //게임 씬으로 넘어갈 시에 호출되는 '사실상 init'인 함수
    public void GameScene()
    {
        _clientTimer = Util.GetOrAddComponent<ClientTimer>(root);
        _clientGauge = Util.GetOrAddComponent<ClientGauge>(root);
        _playKillerSound = Util.GetOrAddComponent<PlayKillerSound>(root);
        _myKillerSkill = Util.GetOrAddComponent<MyKillerSkill>(root);
        _myKillerSkill.enabled = false;
    }

    public void Clear()
    {
    }

    private void WhenChangeDayNight(float timeToSet)
    {
        if (Managers.UI.GetComponentInSceneUI<InGameUI>() != null)
        {
            _clientTimer.Init(timeToSet);
            Managers.UI.GetComponentInSceneUI<InGameUI>().ChangeDayNightUI();
        }
        else if (Managers.UI.GetComponentInSceneUI<ObserveUI>() != null)
        {
            Managers.UI.GetComponentInSceneUI<ObserveUI>().InitObserveTimer(timeToSet);
        }
    }

    public void ChangeToDay(float timeToSet)
    {
        _isDay = true;
        WhenChangeDayNight(timeToSet);
    }
    
    public void ChangeToNight(float timeToSet)
    {
        _isDay = false;
        WhenChangeDayNight(timeToSet);
    }

    public void ChangeToKiller()
    {
        _myKillerSkill.enabled = true;
        _myKillerSkill.Init();
    }

    public void IsNotKiller()
    {
        _myKillerSkill.enabled = false;
        Managers.UI.GetComponentInSceneUI<InGameUI>().IsNotKiller();
    }

    #region 근처 킬러 소리 처리
    public PlayKillerSound _playKillerSound;

    public void SetUpKillerSound()
    {
        if (!Managers.Player.IsMyDediPlayerKiller())
        {
            Managers.Sound.SetupKillerAudioSource();
            _playKillerSound.Init(_dokidokiStart, _dokidokiClose, _dokidokiExtreme);
        }
    }
    public void PlayChaseSound()
    {if(_playKillerSound!=null)
        _playKillerSound.CheckPlayChaseSound();
    }

    #endregion

}