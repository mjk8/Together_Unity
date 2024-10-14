using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NightIsOverPopup : UI_popup
{
    private string _deadPlayerPrefabPath = "DeadPlayerGO";
    Animator _backgroundAnim;
    private Camera _camera;
    private TMP_Text _mySacrificeText;
    private TMP_Text _otherSacrificeText;
    private TMP_Text _playerName;

    private string _myText;
    private string _otherText;
    private int _playerID;
    void Start()
    {
        _playerID = Managers.Player._processDeadPlayerID;
        _mySacrificeText = transform.Find("MySacrificeText").GetComponent<TMP_Text>();
        _otherSacrificeText = transform.Find("OtherSacrificeText").GetComponent<TMP_Text>();
        _playerName = transform.Find("PlayerName").GetComponent<TMP_Text>();
        
        _myText = _mySacrificeText.text;
        _otherText = _otherSacrificeText.text;
        
        _playerName.text = "";
        _mySacrificeText.text = "";
        _otherSacrificeText.text = "";

        _backgroundAnim = transform.GetComponent<Animator>();
        _camera = GameObject.Find(String.Concat(_deadPlayerPrefabPath,"/RenderCamera")).transform.GetComponent<Camera>();
        _camera.enabled = true;
        Managers.UI.ChangeCanvasRenderMode(RenderMode.ScreenSpaceCamera); //캔버스 렌더모드 변경
        //죽을 플레이어를 찾아서 이동시키기
        GameObject currentGO = GameObject.Find(String.Concat(_deadPlayerPrefabPath, "/PlayerPrefab"));
        Util.DestroyAllChildren(currentGO.transform);
        GameObject newGO;
        if (_playerID == Managers.Player._myDediPlayerId)
        {
            if (_playerID != Managers.Player._processKillerPlayerID)
            {
                newGO = Managers.Resource.Instantiate("Player/OtherPlayer(Model)", currentGO.transform);
            }
            else
            {
                newGO = Managers.Resource.Instantiate(string.Concat("Player/OtherPlayerKiller/",Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._killerEngName),currentGO.transform);
            }
        }
        else
        {
            newGO = Managers.Player._otherDediPlayers[_playerID];
            newGO.transform.GetComponent<CharacterController>().enabled = false;
        }
        newGO.transform.SetParent(currentGO.transform);
        newGO.transform.localPosition = Vector3.zero;
        newGO.transform.localRotation = Quaternion.identity;
        
        if (_playerID == Managers.Player._myDediPlayerId)
        {
            newGO.GetComponentInChildren<PlayerAnimController>().SetTriggerByString("Die");
            _mySacrificeText.text = _myText;
        }
        else
        {
            Managers.Player.GetAnimator(_playerID).SetTriggerByString("Die");
            _otherSacrificeText.text = _otherText;
            _playerName.text = Managers.Player._otherDediPlayers[_playerID].GetComponent<OtherDediPlayer>().Name;
        }
        Managers.Sound.Play("SurvivorBoom");
    }
    
    public void StartDay()
    {
        _camera.enabled = false;
        if (Managers.UI.SceneUI.name == Define.SceneUIType.ObserveUI.ToString())
        {
            Managers.UI.ChangeCanvasRenderMode(RenderMode.ScreenSpaceOverlay);
            Managers.UI.GetComponentInSceneUI<ObserveUI>().ChangeIfObservingThisPlayer(_playerID);
            Managers.Player.DeletePlayerObject(_playerID);
            Managers.UI.CloseAllPopup();
        }
        else if (_playerID == Managers.Player._myDediPlayerId)
        {
            Managers.Player.DeletePlayerObject(_playerID);
            Managers.UI.ChangeCanvasRenderMode(RenderMode.ScreenSpaceOverlay);
            Managers.UI.LoadScenePanel(Define.SceneUIType.PlayerDeadUI);
            Managers.UI.CloseAllPopup();
        }
        else
        {
            Managers.Player.DeletePlayerObject(_playerID);
            StartCoroutine(OpenEyes());
        }
    }
    
    private IEnumerator OpenEyes()
    {
        _backgroundAnim.SetTrigger("OpenEyes");
        Managers.Game._playKillerSound._checkForSound = false;
        yield return new WaitForSeconds(0.4f);
        Managers.UI.ChangeCanvasRenderMode(RenderMode.ScreenSpaceOverlay);
        Managers.Player.ActivateInput();
        Managers.UI.CloseAllPopup();
    }
}