using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class WinnerUI : UI_scene
{
    private string _winPlayerPrefabPath = "WinnerPlayerGO";
    private TMP_Text _winnerPlayerNameText;
    private TMP_Text _anyKeyText;
    private GameObject _currentGO;
    Camera _camera;
    bool _canMoveOn = false;

    void Awake()
    {
        _winnerPlayerNameText = transform.Find("WinnerPlayerName").GetComponent<TMP_Text>();
        _anyKeyText = transform.Find("AnyKeyText").GetComponent<TMP_Text>();
        _currentGO = GameObject.Find(string.Concat(_winPlayerPrefabPath, "/PlayerPrefab"));
        _camera = GameObject.Find(string.Concat(_winPlayerPrefabPath,"/RenderCamera")).transform.GetComponent<Camera>();
        _camera.enabled = true;
        _anyKeyText.enabled = false;
        
        // Set this camera to be the main camera
        Camera.main.gameObject.SetActive(false); // Disable the current main camera
        _camera.tag = "MainCamera"; // Set this camera's tag to "MainCamera"
        _camera.AddComponent<AudioListener>(); // Add an AudioListener to this camera
        Managers.Sound.Play("Win");
        StartCoroutine(WaitForTwoSeconds());
    }
    
    public void SetWinner(int playerId, string playerName)
    {
        Managers.Player._syncMoveController.ToggleHardSnap(playerId,false);
        GameObject newGO;
        if(playerId == Managers.Player._myDediPlayerId)
        {
            newGO = Managers.Resource.Instantiate("Player/OtherPlayer(Model)", _currentGO.transform);
            Managers.Player._myDediPlayer.SetActive(false);
        }
        else
        {
            newGO = Managers.Player._otherDediPlayers[playerId];
            newGO.GetComponentInChildren<OtherDediPlayer>().enabled = false;
        }
        newGO.transform.SetParent(_currentGO.transform);
        newGO.transform.localPosition = Vector3.zero;
        newGO.transform.localRotation = Quaternion.identity;
        newGO.GetComponentInChildren<PlayerAnimController>().SetTriggerByString("Victory");
        _winnerPlayerNameText.text = playerName;
    }

    private void Update()
    {
        if (_canMoveOn)
        {
            if (Input.anyKeyDown)
            {
                Managers.Scene.EndGameAndReturnToLobby();
            }
        }
    }
    
    IEnumerator WaitForTwoSeconds()
    {
        yield return new WaitForSeconds(2f);
        _canMoveOn = true;
        _anyKeyText.enabled = true;
    }
}
