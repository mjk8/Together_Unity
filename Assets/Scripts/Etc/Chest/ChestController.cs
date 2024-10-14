using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public List<GameObject> _chestList = new List<GameObject>(); //상자 리스트(인덱스는 chestId)
    public string _level1ChestPath = "Chest/Chest Standard";
    public string _level2ChestPath = "Chest/Chest Royal";
    public string _level3ChestPath = "Chest/Chest Mythical";
    GameObject _level1Chest; //레벨1 상자 프리팹
    GameObject _level2Chest; //레벨2 상자 프리팹
    GameObject _level3Chest; //레벨3 상자 프리팹
    Transform _chestsParent; //상자들이 실제로 생성될 부모 오브젝트

    float _noPointProbability = 0.15f; //꽝 상자 확률(1,2레벨 상자만 꽝이 있음)
    
    int _level1Point = 1; //레벨1 상자 포인트
    int _level2Point = 2; //레벨2 상자 포인트
    int _level3Point = 3; //레벨3 상자 포인트
    
    int _level1Count = 0; //레벨1 상자 개수
    int _level2Count = 0; //레벨2 상자 개수
    int _level3Count = 0; //레벨3 상자 개수
    

    public void Init()
    {
        //상자들이 실제로 생성될 부모 오브젝트 초기화
        //Map/Chests가 존재하는지 검사
        if(GameObject.Find("Map/Chests") != null)
        {
            _chestsParent =  GameObject.Find("Map/Chests").transform;
        }
       
        
        //상자 포인트 초기화
        _level1Point = 1;
        _level2Point = 2;
        _level3Point = 3;

        //상자 개수 카운트 초기화
        _level1Count = 0;
        _level2Count = 0;
        _level3Count = 0;
    }

    /// <summary>
    /// 낮 시작되어서 서버로부터 상자 패킷을 받았을때, 이거 하나만 호출하면 상자 초기화 및 생성까지 다 처리됨
    /// </summary>
    /// <param name="dscNewChestsInfo"></param>
    public void ChestSetAllInOne(DSC_NewChestsInfo dscNewChestsInfo)
    {
        ClearAllChest();
        SpawnAllChest(dscNewChestsInfo);
    }
    
    /// <summary>
    /// 매번 낮이 되면 초기화를 위해 호출되는 함수
    /// </summary>
    public void ClearAllChest()
    {
        foreach (var chest in _chestList)
        {
            Managers.Resource.Destroy(chest);
        }
        _chestList.Clear();
        
        _level1Count = 0;
        _level2Count = 0;
        _level3Count = 0;
    }
    
    /// <summary>
    /// 매번 낮이 되면 상자를 생성하기 위해 호출되는 함수(데디서버 정보에 따라서)
    /// </summary>
    public void SpawnAllChest(DSC_NewChestsInfo dscNewChestsInfo)
    {
        if(_chestsParent == null)
        {
            _chestsParent =  GameObject.Find("Map/Chests").transform;
        }
        
        //서버에서 알려준 정보로 상자 생성
        int chestsCount = _chestsParent.childCount; //_chestsParent 자식 개수
        for (int i = 0; i < chestsCount; i++)
        {
            //실제로 생성할 상자 오브젝트, 그 부모 오브젝트
            GameObject chest = null;
            Transform parent = _chestsParent.GetChild(i);
            
            //i번째 상자 정보 꺼내옴
            ChestInfo chestInfo = dscNewChestsInfo.ChestsInfo[i];
            
            //상자 레벨 처리
            int chestLevel = chestInfo.ChestLevel;
            if(chestLevel == 1)
            {
                _level1Count++;
                chest = Managers.Resource.Instantiate(_level1ChestPath, parent);
                parent.name = $"Lv1Chest_{i}";
            }
            else if(chestLevel == 2)
            {
                _level2Count++;
                chest = Managers.Resource.Instantiate(_level2ChestPath, parent);
                parent.name = $"Lv2Chest_{i}";
            }
            else if(chestLevel == 3)
            {
                _level3Count++;
                chest = Managers.Resource.Instantiate(_level3ChestPath, parent);
                parent.name = $"Lv3Chest_{i}";
            }
            
            //생성한 상자 부모 설정
            chest.transform.SetParent(parent, false);
            chest.transform.localPosition = Vector3.zero;
            chest.transform.localRotation = Quaternion.identity;
            chest.transform.localScale = Vector3.one;
            
            //상자에 전용 스크립트 부착 및 정보 입력
            Chest chestScript = Util.GetOrAddComponent<Chest>(chest);
            chestScript.InitChest(chestInfo.ChestId, chestLevel, chestInfo.ChestPoint);
            
            //상자 리스트에 추가(인덱스는 상자의 고유 ID)
            _chestList.Add(chest);
        }
        
    }
    
    /// <summary>
    /// <para>상자 열기를 시도할때 호출(실제 가능 여부는 서버에서 판단)</para>
    /// 상자 열기 시도 패킷을 보냄
    /// </summary>
    /// <param name="chestId">상자id</param>
    public void TryOpenChest(int chestId)
    {
        //내 플레이어가 죽은 상태면 무시
        if(Managers.Player.IsPlayerDead(Managers.Player._myDediPlayerId))
            return;

        //상자 열었을때 처리
        //1. 특정 상자 열고싶다는 패킷을 데디서버에게 보냄
        //2. (동시성 문제)데디서버에서 판단 후, 상자 open처리 가능하면 => (먼저 열은)클라가 상자 열었다는 패킷을 모든 클라이언트에게 보냄
        //3. 상자 열었을때의 이펙트,사운드 처리
        
        //이 함수에서는 1번만 처리
        CDS_TryChestOpen tryChestOpenPacket = new CDS_TryChestOpen()
        {
            MyDediplayerId = Managers.Player._myDediPlayerId,
            ChestId = chestId
        };
        Managers.Network._dedicatedServerSession.Send(tryChestOpenPacket);
    }
        
    /// <summary>
    /// 다른 플레이어가 상자를 열었을때 처리
    /// </summary>
    /// <param name="chestId">상자id</param>
    /// <param name="otherDediPlayerId">열은 데디플레이어id</param>
    public void OnOtherPlayerOpenChestSuccess(DSC_ChestOpenSuccess chestOpenSuccessPacket)
    {
        int chestId = chestOpenSuccessPacket.ChestId;
        int dediPlayerId = chestOpenSuccessPacket.PlayerId;
        int getPoint = chestOpenSuccessPacket.GetPoint; //얻은 포인트
        int totalPoint = chestOpenSuccessPacket.TotalPoint; //총 포인트
        
        //상자 열었다는 정보기록,이펙트,사운드 처리
        Chest chest = _chestList[chestId].GetComponent<Chest>();
        chest.OpenChest();
        
        Managers.Player._otherDediPlayers[dediPlayerId].GetComponent<OtherDediPlayer>()._totalPoint = totalPoint;

        if (chest._point > 0) //꽝 상자 아님
        {
            //플레이어 효과처리
            //Managers.Sound.Play("WarningNotification",Define.Sound.Effects,chest.transform.GetComponent<AudioSource>());
        }
        else //꽝 상자임
        {
            //플레이어 효과처리
            //Managers.Sound.Play("WarningNotification",Define.Sound.Effects,chest.transform.GetComponent<AudioSource>());
        }
    }
    
    /// <summary>
    /// 내 플레이어가 상자를 열었을때 처리
    /// </summary>
    /// <param name="chestId">상자id</param>
    public void OnMyPlayerOpenChestSuccess(DSC_ChestOpenSuccess chestOpenSuccessPacket)
    {
        int chestId = chestOpenSuccessPacket.ChestId;
        int getPoint = chestOpenSuccessPacket.GetPoint; //얻은 포인트
        int totalPoint = chestOpenSuccessPacket.TotalPoint; //총 포인트
        
        
        //상자 열었다는 정보기록,이펙트,사운드 처리
        Chest chest = _chestList[chestId].GetComponent<Chest>();
        chest.OpenChest();
        
        //내 플레이어 포인트 증가처리 및 효과처리
        Managers.Inventory.SetTotalPoint(totalPoint);//포인트 증가 처리
        
        if (chest._point > 0) //꽝 상자 아님
        {
            Managers.Sound.Play("Success",Define.Sound.Effects,chest.transform.GetComponent<AudioSource>());
            Managers.Sound.Play("CoinSound",Define.Sound.Effects,chest.transform.GetComponent<AudioSource>());
            Managers.UI.GetComponentInSceneUI<InGameUI>().SetCurrentCoin(totalPoint);
            Managers.UI.GetComponentInSceneUI<InGameUI>().AddGetCoin(getPoint);
        }
        else //꽝 상자임
        {
            Managers.Sound.Play("Fail",Define.Sound.Effects,chest.transform.GetComponent<AudioSource>());
        }
    }
}