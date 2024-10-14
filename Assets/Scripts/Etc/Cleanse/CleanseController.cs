using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class CleanseController : MonoBehaviour
{
    public string _cleansePrefabPath = "Cleanse/Cleanse"; //클린즈 프리팹 경로
    public string _cleanseParentPath = "Map/Cleanses"; //클린즈들이 실제로 생성될 부모 오브젝트
    public GameObject _cleanseParent; //클린즈들이 실제로 생성될 부모 오브젝트
    public List<GameObject> _cleansetList = new List<GameObject>(); //클린즈 리스트(인덱스는 클린즈 고유 ID)
    public float _cleansePoint = 0; //클린즈로 올라갈 게이지 정도
    public float _cleanseDurationSeconds = 0; //정화하는데 걸리는 시간(초 단위)
    public float _cleanseCoolTimeSeconds = 0; //클린즈를 사용한 후 쿨타임(초 단위)
    public Cleanse _myPlayerCurrentCleanse; //내 플레이어가 현재 클린징중인 클린즈

    public void Init()
    {
        //TODO: 이닛 함수 구현
        _myPlayerCurrentCleanse = null;
    }

    /// <summary>
    /// 패킷 정보를 바탕으로 클린즈 생성 및 정보 입력
    /// </summary>
    /// <param name="newCleansesInfo">서버로부터 받은 클린즈들 정보</param>
    public void SpawnAllCleanse(DSC_NewCleansesInfo newCleansesInfo)
    {
        if (_cleanseParent == null)
        {
            _cleanseParent = GameObject.Find(_cleanseParentPath);
        }
        _cleanseParent.SetActive(true);
        
        //기존 클린즈들이 있다면 삭제
        foreach (GameObject cleanse in _cleansetList)
        {
            Managers.Resource.Destroy(cleanse);
        }
        _cleansetList.Clear();
        
        //newClenasesInfo에 있는 클린즈 정보를 이용해서 클린즈 생성
        foreach (CleanseInfo cleanseInfo in newCleansesInfo.Cleanses)
        {
            GameObject cleanse = Managers.Resource.Instantiate(_cleansePrefabPath, _cleanseParent.transform);
            Cleanse cleanseComponent = Util.GetOrAddComponent<Cleanse>(cleanse);
            cleanseComponent.InitCleanse(cleanseInfo.CleanseId, cleanseInfo.CleanseTransform,
                cleanseInfo.CleansePoint, cleanseInfo.CleanseDurationSeconds, cleanseInfo.CleanseCoolTimeSeconds);
            _cleansetList.Add(cleanse);
        }
    }

    /// <summary>
    /// 내 플레이어가 클린즈 사용 시도(서버로부터 사용 가능여부 확인)
    /// </summary>
    /// <param name="cleanseId">사용시도하는 클린즈id</param>
    public void TryCleanse(int cleanseId)
    {
        //킬러라면 불가능처리
        if (Managers.Player.IsMyDediPlayerKiller())
            return;

        // 내 플레이어가 죽었으면 불가능
        if (Managers.Player.IsPlayerDead(Managers.Player._myDediPlayerId))
        {
            return;
        }


        //서버에게 cleanseId를 보내서 사용 가능여부 확인
        CDS_RequestCleansePermission requestCleansePermission = new CDS_RequestCleansePermission();
        requestCleansePermission.MyDediplayerId = Managers.Player._myDediPlayerId;
        requestCleansePermission.CleanseId = cleanseId;
        Managers.Network._dedicatedServerSession.Send(requestCleansePermission);
    }

    /// <summary>
    /// 내 플레이어가 클린즈하던거 취소
    /// </summary>
    public void QuitCleansing(int cleanseId)
    {
        Managers.UI.ClosePopup();
        _myPlayerCurrentCleanse = null;

        //내 플레이어가 죽었으면 불가능
        if (Managers.Player.IsPlayerDead(Managers.Player._myDediPlayerId))
        {
            return;
        }

        //서버에게 클린즈 중단을 알림
        CDS_CleanseQuit cleanseQuit = new CDS_CleanseQuit();
        cleanseQuit.MyDediplayerId = Managers.Player._myDediPlayerId;
        cleanseQuit.CleanseId = cleanseId;
        Managers.Network._dedicatedServerSession.Send(cleanseQuit);

        _cleansetList[cleanseId].GetComponent<Cleanse>().OnPlayerQuitCleansing();

        Debug.Log("Cleanse Quit");
    }

    /// <summary>
    /// 클린즈 성공시 서버에게 알림
    /// </summary>
    /// <param name="cleanseId"></param>
    public void CleanseSuccess(int cleanseId)
    {
        Debug.Log("Cleanse Success");
        //서버에게 클린즈 성공을 알림
        CDS_CleanseSuccess cleanseSuccess = new CDS_CleanseSuccess();
        cleanseSuccess.MyDediplayerId = Managers.Player._myDediPlayerId;
        cleanseSuccess.CleanseId = cleanseId;
        Managers.Network._dedicatedServerSession.Send(cleanseSuccess);
    }

    /// <summary>
    /// 클린즈 사용 허가를 받았을때 처리
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="cleanseId">클린즈id</param>
    public void OnGetPermission(int playerId, int cleanseId)
    {
        if (playerId == Managers.Player._myDediPlayerId) //내 플레이어 일때
        {
            //TODO: 내 플레이어가 클린징 시작했을때의 필요한 처리 + 내 플레이어 클렌징 모션 실행시키기
            _myPlayerCurrentCleanse = _cleansetList[cleanseId].GetComponent<Cleanse>();
            _myPlayerCurrentCleanse.OnMyPlayerGetPermission(); //딱히 이때는 처리할거 없는듯?

            //클린징  시작
            Managers.UI.LoadPopupPanel<CleansePopup>(true, false);
        }
        else //다른 플레이어 일때
        {
            //TODO: 다른 플레이어가 클린징 시작했을때의 필요한 처리
            _cleansetList[cleanseId].GetComponent<Cleanse>().OnOtherPlayerGetPermission();

            //TODO: 해당 플레이어 클렌징 모션 실행시키기
        }
    }

    /// <summary>
    /// 다른 플레이어가 클린징 취소했을때 처리
    /// </summary>
    /// <param name="playerId">플레이어id</param>
    /// <param name="cleanseId">클린즈id</param>
    public void OnOtherClientQuitCleanse(int playerId, int cleanseId)
    {
        //내 플레이어가 클린징 취소한거는 이미 처리했으니까 처리할 필요 없음
        if (playerId == Managers.Player._myDediPlayerId)
            return;

        Cleanse cleanse = _cleansetList[cleanseId].GetComponent<Cleanse>();
        cleanse.OnPlayerQuitCleansing();

        //TODO: 해당 플레이어 클렌징 취소 모션 실행시키기
    }

    /// <summary>
    /// 나 또는 다른플레이어가 클린즈 성공했을때 호출됨
    /// </summary>
    /// <param name="playerId">클린즈 성공한 플레이어id</param>
    /// <param name="cleanseId">정화 성공한 클린즈id</param>
    public void OnClientCleanseSuccess(int playerId, int cleanseId)
    {
        if (playerId == Managers.Player._myDediPlayerId) //내 플레이어 일때
        {
            _myPlayerCurrentCleanse.OnCleanseSuccess();
            _myPlayerCurrentCleanse = null;
        }
        else //다른 플레이어 일때
        {
            Cleanse cleanse = _cleansetList[cleanseId].GetComponent<Cleanse>();
            cleanse.OnCleanseSuccess();
        }

        Managers.Game._clientGauge.IncreaseGauge(playerId, _cleansePoint);
    }

    /// <summary>
    /// 클린즈 쿨타임이 끝났을때 호출
    /// </summary>
    /// <param name="cleanseId"></param>
    public void OnCleanseCooltimeFinish(int cleanseId)
    {
        Cleanse cleanse = _cleansetList[cleanseId].GetComponent<Cleanse>();
        cleanse.OnCleanseCooltimeFinish();
    }

    public void NightIsOver()
    {
        if (_myPlayerCurrentCleanse != null)
        {
            QuitCleansing(_myPlayerCurrentCleanse._cleanseId);
        }

        _myPlayerCurrentCleanse = null;
        if (_cleanseParent != null)
        {
            _cleanseParent.SetActive(false);
        }
    }
}