using System.Collections;
using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;

public class Cleanse : MonoBehaviour
{
    //INIT INFOS
    public int _cleanseId = 0; // 클린즈의 고유 ID (0부터 시작)
    public TransformInfo _transformInfo = new TransformInfo(); // 클린즈의 위치 정보
    public float _cleansePoint = 0; //클린즈로 올라갈 게이지 정도
    public float _cleanseDurationSeconds = 0; //정화하는데 걸리는 시간
    public float _cleanseCoolTimeSeconds = 0; //클린즈를 사용한 후 쿨타임
    public bool _isAvailable = true; // 클린즈 사용 가능 여부
    
    //RUNTIME INFOS
    private CapsuleCollider _trigger; 
    private float _currentCleanseSeconds = 0f; //현재 정화를 몇초동안 했는지
    public float _currentCooltimeSeconds = 0; //현재 클린즈 쿨타임이 몇초 남았는지
    

    /// <summary>
    /// 클린즈 정보 초기화
    /// </summary>
    /// <param name="cleanseId">클린즈id</param>
    /// <param name="transformInfo">위치,회전 정보</param>
    /// <param name="point">클린즈로 올라갈 게이지 정도</param>
    /// <param name="durationSeconds">정화하는데 걸리는 시간</param>
    /// <param name="coolTimeSeconds">클린즈를 사용한 후 쿨타임</param>
    public void InitCleanse(int cleanseId, TransformInfo transformInfo, float point, float durationSeconds, float coolTimeSeconds)
    {
        _cleanseId = cleanseId;
        _transformInfo = transformInfo;
        _cleansePoint = point;
        _cleanseDurationSeconds = durationSeconds;
        _cleanseCoolTimeSeconds = coolTimeSeconds;
        _isAvailable = true;
        
        //_transformInfo를 이용해서 위치, 회전 정보 설정
        transform.position = new Vector3(_transformInfo.Position.PosX, _transformInfo.Position.PosY, _transformInfo.Position.PosZ);
        transform.rotation = new Quaternion(_transformInfo.Rotation.RotX, _transformInfo.Rotation.RotY, _transformInfo.Rotation.RotZ, _transformInfo.Rotation.RotW);
        
        if(_trigger == null)
            _trigger = transform.Find("TriggerCapsule").GetComponent<CapsuleCollider>();
        _trigger.enabled = true;
    }
    
    /// <summary>
    /// 클린징중일때, 클린징이 완료되었다면 처리하는것 까지 담당
    /// </summary>
    /// <param name="cleanseTime">현재 클린징을 몇초동안 했는지</param>
    public void CurrentlyCleansing(float cleanseTime)
    {
        _currentCleanseSeconds = cleanseTime;
        if(_currentCleanseSeconds>=_cleanseDurationSeconds)
        {
            Managers.UI.ClosePopup();
            Managers.Object._cleanseController.CleanseSuccess(_cleanseId);
        }
    }
    
    /// <summary>
    /// 내 플레이어가 클린징 권한 얻었을때 처리
    /// </summary>
    public void OnMyPlayerGetPermission()
    {
        _isAvailable = true;
        _isAvailable = true;
    }
    
    /// <summary>
    /// 다른 플레이어가 클린징 권한 얻었을때 처리
    /// </summary>
    public void OnOtherPlayerGetPermission()
    {
        _isAvailable = false;
        _trigger.enabled = false;
    }

    /// <summary>
    /// 플레이어가 클린징하던거 취소 했을때 처리
    /// </summary>
    /// <param name="dediPlayerId">데디플레이어id</param>
    public void OnPlayerQuitCleansing()
    {
        Debug.Log("OnPlayerQuitCleansing");
        _isAvailable = true;
        _trigger.enabled = true;
        _currentCleanseSeconds = 0f;
    }
    
    /// <summary>
    /// 누군가가 클린즈 성공했을때 처리(나 포함)
    /// </summary>
    public void OnCleanseSuccess()
    {
        Debug.Log("OnCleanseSuccess");
        _isAvailable = false;
        _trigger.enabled = false;
        _currentCleanseSeconds = 0f;
    }

    /// <summary>
    /// 클린즈 쿨타임이 끝났을때 처리
    /// </summary>
    public void OnCleanseCooltimeFinish()
    {
        Debug.Log("OnCleanseCooltimeFinish");
        _isAvailable = true;
        _trigger.enabled = true;
        _currentCleanseSeconds = 0f;
        _currentCooltimeSeconds = 0f;
    }
}