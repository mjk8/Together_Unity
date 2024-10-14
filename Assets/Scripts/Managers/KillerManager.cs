using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class KillerManager
{
    private string _jsonPath;
    private Dictionary<int, KillerFactory> _killerFactories; //key: 킬러Id, value: 킬러 팩토리 객체
    public Dictionary<int, IKiller> _killers; //key: 킬러Id, value: 킬러 객체(킬러별 데이터 저장용. 전시품이라고 생각)
    public Dictionary<int,GameObject> _myKillerPrefabs; //key: 킬러Id, value: 내 킬러 프리팹
    public Dictionary<int,GameObject> _otherPlayerKillerPrefabs; //key: 킬러Id, value: 다른 플레이어 킬러 프리팹
    private static string _killersDataJson; //json이 들어 있게 됨(파싱 해야 함)
    
    public string _myPlayerKillerPrefabPath = "Prefabs/Player/MyPlayerKiller/"; //내 킬러 프리팹 경로
    public string _otherPlayerKillerPrefabPath = "Prefabs/Player/OtherPlayerKiller/"; //다른 플레이어 킬러 프리팹 경로
    
    public void Init()
    {
        _jsonPath = Application.persistentDataPath + "/Data/Killer/Killers.json";
        if (!Directory.Exists(Path.GetDirectoryName(_jsonPath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_jsonPath));
        }
        if (!File.Exists(_jsonPath))
        {
            File.WriteAllText(_jsonPath, "{}"); // Create an empty JSON file
        }
        InitKillerFactories();
        LoadKillerPrefabs();
    }
    
    /// <summary>
    /// 내 플레이어, 다른플레이어에 따라서 킬러 내부 정보 세팅 및 프리팹 생성
    /// </summary>
    /// <param name="dediPlayerId"></param>
    /// <param name="killerType"></param>
    public GameObject CreateKiller(int dediPlayerId, int killerType)
    {
        if (Managers.Player._myDediPlayerId == dediPlayerId)
        {
            if(_killerFactories.ContainsKey(killerType))
            {
                GameObject killerObj = _killerFactories[killerType].CreateKiller(isMyPlayer:true);
                killerObj.transform.SetParent(Managers.Player._myDediPlayer.transform,false);

                return killerObj;
            }
        }
        else
        {
            if(_killerFactories.ContainsKey(killerType))
            {
                GameObject killerObj = _killerFactories[killerType].CreateKiller(isMyPlayer:false);
                killerObj.transform.SetParent(Managers.Player._otherDediPlayers[dediPlayerId].transform,false);

                return killerObj;
            }
        }
        return null;
    }

    /// <summary>
    /// 킬러 기본 공격
    /// </summary>
    public void BaseAttack(int dediPlayerId)
    {
        GameObject killerPlayer =  dediPlayerId == Managers.Player._myDediPlayerId ? Managers.Player._myDediPlayer : Managers.Player._otherDediPlayers[dediPlayerId];
        IKiller killer = killerPlayer.GetComponentInChildren<IKiller>();
        //만약 공격중이라면 공격 못하게 막아두기
        if (!killerPlayer.transform.GetComponentInChildren<PlayerAnimController>().IsAttacking())
        {
            killer.BaseAttack();
            //TODO:: 공격 중일시 속도를 반으로 줄이기
            //TODO:: 공격 패킷 보내기
        }
        
    }
    
    /// <summary>
    /// 킬러 스킬 사용 처리
    /// </summary>
    /// <param name="dediPlayerId">스킬을 사용한 현재 킬러의 데디플레이어id</param>
    public void UseSkill(int killerPlayerId, int skillId)
    {
        IKiller killer = Managers.Player.GetKillerGameObject().GetComponentInChildren<IKiller>();
        //killerID가 같아야지만 스킬 사용
        if (killer.Id == skillId)
        {
            killer.Use(killerPlayerId);
        }
    }
    
    /// <summary>
    /// 킬러 영어 이름 리턴
    /// </summary>
    public string GetKillerEnglishName()
    {
        if (Managers.Player.IsMyDediPlayerKiller())
        {
            return Managers.Player._myDediPlayer.GetComponentInChildren<MyDediPlayer>()
                ._killerEngName;
        }
        else
        {
            return _killers[Managers.Player.GetKillerGameObject().GetComponentInChildren<OtherDediPlayer>()
                ._killerType].EnglishName;
        }
    }

    public IKiller GetMyKillerInfo()
    {
        return _killers[Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._killerType];
    }
    
    /// <summary>
    /// 킬러 팩토리 초기화
    /// </summary>
    public void InitKillerFactories()
    {
        _killerFactories = new Dictionary<int, KillerFactory>();
        _killers = new Dictionary<int, IKiller>();
        
        //킬러 팩토리 생성
        _killerFactories.Add(0, new TheHeartlessFactory());
        _killerFactories.Add(1, new TheDetectorFactory());
    }
    
    /// <summary>
    /// 킬러 프리팹 로드(반드시 아이템 팩토리 초기화 이후에 호출해야 함)
    /// </summary>
    public void LoadKillerPrefabs()
    {
        _myKillerPrefabs = new Dictionary<int, GameObject>();
        _otherPlayerKillerPrefabs = new Dictionary<int, GameObject>();
        
        _myKillerPrefabs.Add(0, Managers.Resource.Load<GameObject>(_myPlayerKillerPrefabPath + "The Heartless"));
        _otherPlayerKillerPrefabs.Add(0, Managers.Resource.Load<GameObject>(_otherPlayerKillerPrefabPath + "The Heartless"));
        
        _myKillerPrefabs.Add(1, Managers.Resource.Load<GameObject>(_myPlayerKillerPrefabPath + "The Detector"));
        _otherPlayerKillerPrefabs.Add(1, Managers.Resource.Load<GameObject>(_otherPlayerKillerPrefabPath + "The Detector"));
    }
    
    /// <summary>
    /// 킬러 데이터를 로드후 파싱
    /// </summary>
    public void LoadKillerData()
    {
        if (File.Exists(_jsonPath))
        {
            string dataAsJson = File.ReadAllText(_jsonPath);
            _killersDataJson = dataAsJson;
        }
        else
        {
            Debug.LogError("Cannot find file at " + _jsonPath);
            return;
        }
        
        //파싱
        ParseKillerData();
    }
    
    /// <summary>
    /// json파일을 이미 받은 상태에서 킬러 데이터를 파싱
    /// </summary>
    public void ParseKillerData()
    {
        var killersData = JObject.Parse(_killersDataJson)["Killers"];
        _killers = new Dictionary<int, IKiller>();
        
        foreach (var killerData in killersData)
        {
            IKiller killer = null;
            string className = killerData["EnglishName"].ToString();
            //className의 띄어쓰기 제거
            className = className.Replace(" ", "");

            Type type = Type.GetType(className);
            if (type != null)
            {
                killer = (IKiller)killerData.ToObject(type);
            }
            
            if (killer != null)
            {
                _killers.Add(killer.Id, killer);
            }
        }
    }

    /// <summary>
    /// 서버로부터 받은 json데이터를 저장함
    /// </summary>
    /// <param name="jsonData">json 문자열</param>
    public void SaveJsonData(string jsonData)
    {
        //_jsonPath에다가 jsonData를 저장. 이미 존재한다면 지우고 덮어쓰기
        File.WriteAllText(_jsonPath, jsonData);
        
        //TODO: 패킷핸들러에서 동시성이슈때문에 StartGameHandler에서 주석처리해놓은거 출시할때 풀기
    }
}