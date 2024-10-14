using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

/// <summary>
/// 룸서버와 데디서버의 플레이어 정보를 들고있는 매니저
/// </summary>
public class PlayerManager
{
    public SyncMoveController _syncMoveController = new SyncMoveController();
    
    public MyRoomPlayer _myRoomPlayer; //내 룸서버 플레이어
    public Dictionary<int, RoomPlayer> _otherRoomPlayers = new Dictionary<int, RoomPlayer>(); //다른 룸서버 플레이어들 (key: playerId, value: 플레이어 정보)
    
    
    public GameObject _myDediPlayer; //내 데디서버 플레이어
    public int _myDediPlayerId = -1; //내 데디서버 플레이어의 playerId (초기값 -1)
    public Dictionary<int,GameObject> _otherDediPlayers = new Dictionary<int, GameObject>(); //다른 데디서버 플레이어들 (key: 데디playerId, value: 플레이어 오브젝트)
    public Dictionary<int,GameObject> _ghosts = new Dictionary<int, GameObject>()   ; //key: 데디playerId, value: 고스트 오브젝트
    public string _tempMyPlayerPrefabPath = "Player/MyPlayer";
    public string _tempOtherPlayerPrefabPath = "Player/OtherPlayer";
    public string _tempTargetGhost = "Player/TargetGhost";
    
    public int _processDeadPlayerID = -1; //NightIsOverPopup에서 죽은 플레이어의 id
    public int _processKillerPlayerID = -1; //NightIsOverPopup에서 킬러로 지정된 플레이어의 id
    public void Init()
    {
        _myRoomPlayer = new MyRoomPlayer();
        if(_syncMoveController == null)
            _syncMoveController = new SyncMoveController();

        _myRoomPlayer.Name = Managers.Steam._steamName;
    }
    
    public void Clear()
    {
        _myRoomPlayer.Clear();
        _otherRoomPlayers.Clear();
        
        ClearDedi();
        _syncMoveController.Clear();
    }

    public void ClearDedi()
    {
        _myDediPlayerId = -1;
        if (_myDediPlayer != null)
        {
            GameObject.Destroy(_myDediPlayer);
            _myDediPlayer = null;
        }
        
        foreach (GameObject dediPlayer in _otherDediPlayers.Values)
        {
            if (dediPlayer != null)
            {
                GameObject.Destroy(dediPlayer);
            }
        }
        _otherDediPlayers.Clear();
        
        foreach (GameObject ghost in _ghosts.Values)
        {
            if (ghost != null)
            {
                GameObject.Destroy(ghost);
            }
        }
        _ghosts.Clear();
    }
    
    
    /// <summary>
    /// 데디서버 플레이어+고스트(다른 플레이어일 경우에만)를 실제로 생성하는 함수
    /// </summary>
    /// <param name="playerInfo"></param>
    /// <param name="transformInfo"></param>
    /// <param name="isMyPlayer"></param>
    /// <returns></returns>
    public GameObject SpawnPlayer(PlayerInfo playerInfo, TransformInfo transformInfo, bool isMyPlayer)
    {
        GameObject obj = null;
        if(isMyPlayer) //본인 플레이어용 프리팹 이용
        {
            obj = Managers.Resource.Instantiate(_tempMyPlayerPrefabPath);
            obj.name = $"MyPlayer_{playerInfo.PlayerId}";
            _myDediPlayer = obj;
            _myDediPlayerId = playerInfo.PlayerId;
            obj.transform.position = new Vector3(transformInfo.Position.PosX, transformInfo.Position.PosY, transformInfo.Position.PosZ);
            obj.transform.rotation = new Quaternion(transformInfo.Rotation.RotX, transformInfo.Rotation.RotY, transformInfo.Rotation.RotZ, transformInfo.Rotation.RotW);
            
            MyDediPlayer myDediPlayerComponent = obj.AddComponent<MyDediPlayer>();
            myDediPlayerComponent.Init(playerInfo.PlayerId, playerInfo.Name);
            
            //플레이어 아이템 숨기기
            Transform t = Util.FindChild(obj, "WeaponR_Parent", true).transform;
            foreach (Transform child in t)
            {
                child.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else //다른 플레이어용 프리팹 이용
        {
            obj = Managers.Resource.Instantiate(_tempOtherPlayerPrefabPath);
            obj.name = $"OtherPlayer_{playerInfo.PlayerId}";
            Managers.Player._otherDediPlayers.Add(playerInfo.PlayerId, obj);
            obj.transform.position = new Vector3(transformInfo.Position.PosX, transformInfo.Position.PosY, transformInfo.Position.PosZ);
            obj.transform.rotation = new Quaternion(transformInfo.Rotation.RotX, transformInfo.Rotation.RotY, transformInfo.Rotation.RotZ, transformInfo.Rotation.RotW);
            
            OtherDediPlayer otherDediPlayerComponent = obj.AddComponent<OtherDediPlayer>();
            otherDediPlayerComponent.Init(playerInfo.PlayerId, playerInfo.Name);
            
            //플레이어 아이템 숨기기
            Transform t = Util.FindChild(obj, "WeaponR_Parent", true).transform;
            foreach (Transform child in t)
            {
                child.GetComponent<MeshRenderer>().enabled = false;
            }
            t = Util.FindChild(obj, "WeaponL_Parent", true).transform;
            foreach (Transform child in t)
            {
                child.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        
        
        //다른 플레이어라면 고스트 생성 및 등록
        if (!isMyPlayer)
        {
            GameObject newGhost = Managers.Resource.Instantiate(_tempTargetGhost);
            newGhost.transform.position = new Vector3(transformInfo.Position.PosX, transformInfo.Position.PosY, transformInfo.Position.PosZ);
            newGhost.transform.rotation = new Quaternion(transformInfo.Rotation.RotX, transformInfo.Rotation.RotY, transformInfo.Rotation.RotZ, transformInfo.Rotation.RotW);
            _ghosts.Add(playerInfo.PlayerId,newGhost);
            newGhost.name = $"Ghost_{playerInfo.PlayerId}"; //고스트 오브젝트 이름을 "Ghost_플레이어id"로 설정
            
            //만약 newGhost가 Ghost.cs컴포넌트 가지고 있지 않다면 추가
            if (newGhost.GetComponent<Ghost>() == null)
            {
                newGhost.AddComponent<Ghost>();
            }
        }

        return obj;
    }
    

    /// <summary>
    /// 플레이어를 게임상에서 제거하는 함수 (Destroy처리)
    /// </summary>
    /// <param name="dediPlayerObj">Destroy할 플레이어 오브젝트</param>
    public void DespawnPlayer(GameObject dediPlayerObj)
    {
        Managers.Resource.Destroy(dediPlayerObj);
    } 
    public void DespawnGhost(GameObject ghostObj)
    {
        Managers.Resource.Destroy(ghostObj);
    }

    /// <summary>
    /// 킬러를 지정함
    /// </summary>
    /// <param name="dediPlayerId">킬러로 지정할 데디플레이어id</param>
    /// <param name="killerType">킬러의 종류</param>
    /// <param name="callback">킬러 설정된 후 불릴 함수</param>
    public void OnKillerAssigned(int dediPlayerId, int killerType, Action callback = null)
    {
        ClearKiller();
        if (Managers.Player._myDediPlayerId == dediPlayerId)
        {
            Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._isKiller = true;
            Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._killerType = killerType;
            Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._killerEngName = Managers.Killer._killers[killerType].EnglishName;
            
            _myDediPlayer.transform.Find("PlayerPrefab").gameObject.SetActive(false);
            Managers.Killer.CreateKiller(dediPlayerId, killerType);

            //킬러 콜라이더만 쓰도록 생존자용 콜라이더 끄기
            Managers.Player._myDediPlayer.transform.Find("PlayerTrigger").GetComponent<CapsuleCollider>().enabled = false;
            Managers.Player._myDediPlayer.transform.Find("SurvivorTrigger").GetComponent<CapsuleCollider>().enabled = false;
            Managers.Game.ChangeToKiller(); //킬러일시 처리 (스킬 UI 등)
        }

        else
        {
            Managers.Player._otherDediPlayers[dediPlayerId].GetComponent<OtherDediPlayer>()._isKiller = true;
            Managers.Player._otherDediPlayers[dediPlayerId].GetComponent<OtherDediPlayer>()._killerType = killerType;
            
            _otherDediPlayers[dediPlayerId].transform.Find("PlayerPrefab").gameObject.SetActive(false);
            Managers.Killer.CreateKiller(dediPlayerId, killerType);
            if (!IsMyPlayerDead())
            {
                Managers.Player._myDediPlayer.transform.Find("PlayerTrigger").GetComponent<CapsuleCollider>().enabled =
                    true;
                Managers.Game.IsNotKiller(); //킬러가 아닐 시 처리
            }

            Managers.Player._myDediPlayer.transform.Find("PlayerTrigger").GetComponent<CapsuleCollider>().enabled = true;
            Managers.Player._myDediPlayer.transform.Find("SurvivorTrigger").GetComponent<CapsuleCollider>().enabled = true;
        }

        callback?.Invoke();
    }

    /// <summary>
    /// 내 데디플레이어를 포함한 모든 데디플레이어의 isKiller를 false로 설정 + 킬러타입도 -1로 초기화
    /// </summary>
    public void ClearKiller()
    {
        if (IsMyDediPlayerKiller())
        {
            int killerType = Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._killerType;
            _myDediPlayer.transform.Find("PlayerPrefab").gameObject.SetActive(true);
            Managers.Resource.Destroy(_myDediPlayer.transform.Find(Managers.Killer._killers[killerType].EnglishName).gameObject);
            _myDediPlayer.GetComponent<MyDediPlayer>()._isKiller = false;
            _myDediPlayer.GetComponent<MyDediPlayer>()._killerType = -1;

            //생존자용 콜라이더 키기
            Managers.Player._myDediPlayer.transform.Find("PlayerTrigger").GetComponent<CapsuleCollider>().enabled = true;
            Managers.Player._myDediPlayer.transform.Find("SurvivorTrigger").GetComponent<CapsuleCollider>().enabled = true;
        }
        
        foreach (GameObject dediPlayer in Managers.Player._otherDediPlayers.Values)
        {
            if (dediPlayer.GetComponent<OtherDediPlayer>()._isKiller)
            {
                int killerType = dediPlayer.GetComponent<OtherDediPlayer>()._killerType;
                dediPlayer.transform.Find("PlayerPrefab").gameObject.SetActive(true);
                Managers.Resource.Destroy(dediPlayer.transform.Find(Managers.Killer._killers[killerType].EnglishName).gameObject);
                dediPlayer.GetComponent<OtherDediPlayer>()._isKiller = false;
                dediPlayer.GetComponent<OtherDediPlayer>()._killerType = -1;

                //생존자용 콜라이더 키기
                Managers.Player._myDediPlayer.transform.Find("PlayerTrigger").GetComponent<CapsuleCollider>().enabled = true;
                Managers.Player._myDediPlayer.transform.Find("SurvivorTrigger").GetComponent<CapsuleCollider>().enabled = true;
            }
        }
    }
    
    /// <summary>
    /// killer의 id를 반환함
    /// </summary>
    /// <returns>만약 killer가 없을시 -1을 반환</returns>
    public int GetKillerId()
    {
        foreach (GameObject dediPlayer in Managers.Player._otherDediPlayers.Values)
        {
            if (dediPlayer.GetComponent<OtherDediPlayer>()._isKiller)
            {
                return dediPlayer.GetComponent<OtherDediPlayer>().PlayerId;
            }
        }
        
        if (!IsMyPlayerDead() && Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._isKiller)
        {
            return Managers.Player._myDediPlayerId;
        }

        return -1;
    }
    
    public void ChangeHoldingItem(int itemId,int playerId)
    {
        if (Managers.Player.GetKillerId() == playerId)
        {
            return;
        }

        if (playerId == Managers.Player._myDediPlayerId)
        {
            Transform t = Util.FindChild(_myDediPlayer, "WeaponR_Parent", true).transform;
            if (t == null)
            {
                return;
            }

            _myDediPlayer.GetComponent<MyDediPlayer>()._currentItemID = itemId;
            if (playerId != GetKillerId())
            {
                foreach (Transform child in t)
                {
                    child.GetComponent<MeshRenderer>().enabled = false;

                    //Managers.Item._root.transform.Find("@OnHoldItem")의 자식들을 모두 삭제
                    foreach (Transform onHoldItem in Managers.Item._root.transform.Find("@OnHoldItem"))
                    {
                        Object.Destroy(onHoldItem.gameObject);
                    }
                }

                if (t.Find(itemId.ToString()) != null)
                {
                    t.Find(itemId.ToString()).GetComponent<MeshRenderer>().enabled = true;

                    //OnHold오브젝트 생성하고 실행
                    GameObject onHoldGameObject= Managers.Item._itemFactories[itemId].CreateOnHoldItem(playerId);
                    if (onHoldGameObject != null) //onhold용 게임오브젝트가 있는 경우에만 실행
                    {
                        //생성한 아이템을 @Item/@OnHoldItem 밑에 넣음
                        onHoldGameObject.transform.SetParent(Managers.Item._root.transform.Find("@OnHoldItem"));

                        onHoldGameObject.GetComponent<IItem>().OnHold();
                    }
                }
            }
        }
        else
        {
            Transform right = Util.FindChild(_otherDediPlayers[playerId], "WeaponR_Parent", true).transform;
            Transform left = Util.FindChild(_otherDediPlayers[playerId], "WeaponL_Parent", true).transform;
            _otherDediPlayers[playerId].GetComponent<OtherDediPlayer>()._currentItemID = itemId;
            if (playerId != GetKillerId())
            {
                foreach (Transform child in right)
                { 
                    child.GetComponent<MeshRenderer>().enabled = false;
                }
                foreach (Transform child in left)
                {
                    child.GetComponent<MeshRenderer>().enabled = false;
                }
                if (left.Find(itemId.ToString())!=null)
                {
                    left.Find(itemId.ToString()).GetComponent<MeshRenderer>().enabled = true;
                }
                else if(right.Find(itemId.ToString()) != null)
                {
                    right.Find(itemId.ToString()).GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }
    }
    
    /// <summary>
    /// killer의 GameObject를 반환함
    /// </summary>
    /// <returns>만약 killer가 없을시 null을 반환</returns>
    public GameObject GetKillerGameObject()
    {
        if (GetKillerId() == -1)
        {
            return null;
        }
        
        else if (IsMyDediPlayerKiller())
        {
            return _myDediPlayer;
        }
        return _otherDediPlayers[GetKillerId()];
    }
    
    /// <summary>
    /// 내 데디플레이어가 killer인지 확인
    /// </summary>
    /// <returns>'내'가 killer이면 true, 아니면 false</returns>
    public bool IsMyDediPlayerKiller()
    {
        if (IsMyPlayerDead())
        {
            return false;
        }
        return Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._isKiller;
    }
    
    /// <summary>
    /// 낮 시작할때 모든 플레이어를 초기화
    /// </summary>
    public void ResetPlayerOnDayStart()
    {
        //내 플레이어는 살아있을때만 초기화
        if (!IsPlayerDead(_myDediPlayerId))
        {
            Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._isKiller = false;
            Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._gauge = 0;
            Managers.Inventory.SetTotalPoint(0);
        }
        
        foreach (KeyValuePair<int, GameObject> a in _otherDediPlayers)
        {
            //다른 플레이어는 살아있을때만 초기화
            if (IsPlayerDead(a.Key))
            {
                continue;
            }
            a.Value.GetComponent<OtherDediPlayer>()._isKiller = false;
            a.Value.GetComponent<OtherDediPlayer>()._gauge = 0;
            a.Value.GetComponent<OtherDediPlayer>()._totalPoint = 0;
        }
    }

    public PlayerAnimController GetAnimator(int playerId)
    {
        if (playerId == Managers.Player._myDediPlayerId)
        {
            return _myDediPlayer.GetComponentInChildren<PlayerAnimController>();
        }
        else
        {
            return _otherDediPlayers[playerId].GetComponentInChildren<PlayerAnimController>();
        }
    }
    
    public void ActivateInput()
    {
        //내 죽었다면 활성화 할필요 없음
        if (IsPlayerDead(_myDediPlayerId))
        {
            return;
        }

        _myDediPlayer.GetComponent<PlayerInput>().ActivateInput();
    }
    
    public void DeactivateInput()
    {
        //내 죽었다면 비활성화 할필요 없음
        if (IsPlayerDead(_myDediPlayerId))
        {
            return;
        }

        _myDediPlayer.GetComponent<PlayerInput>().DeactivateInput();
    }

    /// <summary>
    /// 플레이어가 죽었을때의 처리
    /// </summary>
    /// <param name="dediPlayerId">죽은 플레이어 id</param>
    public void ProcessPlayerDeath(int dediPlayerId, int killerPlayerId)
    {
        _processDeadPlayerID = dediPlayerId;
        _processKillerPlayerID = killerPlayerId;
        Managers.UI.LoadPopupPanel<NightIsOverPopup>(true,false);
    }

    /// <summary>
    /// Die animation 끝에 Animation event로 실행되는 함수.
    /// PlayerDeadUI로 sceneUI 변경 후 플레이어 프리팹 삭제
    /// </summary>
    public void DeletePlayerObject(int dediPlayerId)
    {
        if (dediPlayerId==_myDediPlayerId) //죽은게 '나'일때
        {
            Managers.UI.LoadScenePanel(Define.SceneUIType.PlayerDeadUI);
            //내 플레이어 오브젝트 삭제
            if (_myDediPlayer != null)
            {
                DespawnPlayer(_myDediPlayer);
                _myDediPlayer = null;
            }
            
        }
        else if (_otherDediPlayers.ContainsKey(dediPlayerId)) //다른 플레이어가 죽었을때
        {
            //다른 플레이어 오브젝트 삭제
            if (_otherDediPlayers.ContainsKey(dediPlayerId))
            {
                DespawnPlayer(_otherDediPlayers[dediPlayerId]);
                _otherDediPlayers.Remove(dediPlayerId);
            }

            //해당 플레이어 고스트 삭제
            if (_ghosts.ContainsKey(dediPlayerId))
            {
                DespawnGhost(_ghosts[dediPlayerId]);
                _ghosts.Remove(dediPlayerId);
            }
        }
    }

    /// <summary>
    /// 죽은 플레이어인지 확인하는 함수
    /// </summary>
    /// <param name="dediPlayerId">확인할 플레이어의 id</param>
    /// <returns>죽었으면 true, 살아있으면 false</returns>
    public bool IsPlayerDead(int dediPlayerId)
    {
        if (dediPlayerId == _myDediPlayerId)
        {
            return _myDediPlayer == null;
        }
        else
        {
            if (_otherDediPlayers.ContainsKey(dediPlayerId) && _ghosts.ContainsKey(dediPlayerId))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    
    /// <summary>
    /// 내 플레이어가 죽었는지 확인하는 함수
    /// </summary>
    /// <returns>죽었으면 true, 살아있으면 false</returns>
    public bool IsMyPlayerDead()
    {
        return IsPlayerDead(_myDediPlayerId);
    }

    /// <summary>
    /// 데디플레이어 오브젝트를 반환하는 함수
    /// </summary>
    /// <param name="dediPlayerId">구하려는 데디플레이어 오브젝트의 id</param>
    /// <returns>존재하지 않는다면 null</returns>
    public GameObject GetPlayerObject(int dediPlayerId)
    {
        if (dediPlayerId == _myDediPlayerId)
        {
            return _myDediPlayer;
        }
        else if (_otherDediPlayers.ContainsKey(dediPlayerId))
        {
            return _otherDediPlayers[dediPlayerId];
        }
        else
        {
            return null;
        }
    }
}
