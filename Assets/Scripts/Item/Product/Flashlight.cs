using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class Flashlight : MonoBehaviour, IItem
{
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }

    public float BlindDuration {get; set;}
    public float FlashlightDistance {get; set;}
    public float FlashlightAngle {get; set;}
    public float FlashlightAvailableTime {get; set;}
    public float FlashlightTimeRequired {get; set;}


    private bool _isLightOn = false;
    private Light _light;
    private MovementInput _movementInput;
    private Coroutine _currentPlayingCoroutine;
    private GameObject _flashLightSource;
    private MeshRenderer _lightEffectMeshRenderer;

    private OtherDediPlayer _otherDediPlayer;
    public void LateUpdate()
    {
        if (_isLightOn)
        {
            if (PlayerID == Managers.Player._myDediPlayerId)
            {
                //빛과 동일한 길이의 레이 표시
                Debug.DrawRay(_flashLightSource.transform.position,
                    _flashLightSource.transform.forward * FlashlightDistance, Color.red, 0.1f);

                // 현재 회전을 가져옵니다.
                Quaternion currentRotation = _flashLightSource.transform.rotation;

                // 현재 회전의 Euler 각도를 가져옵니다.
                Vector3 eulerAngles = currentRotation.eulerAngles;

                // X축 회전값을 _movementInput._rotationX로 설정합니다.
                float newXRotation = _movementInput._rotationX;

                // 새로운 회전 값을 적용합니다.
                Quaternion newRotation = Quaternion.Euler(newXRotation, eulerAngles.y, eulerAngles.z);
                _flashLightSource.transform.rotation = newRotation;
            }
            else
            {
                //빛과 동일한 길이의 레이 표시
                Debug.DrawRay(_flashLightSource.transform.position,
                    _flashLightSource.transform.forward * FlashlightDistance, Color.red, 0.1f);

                //회전 목표 카메라 위치를 가져옴
                Quaternion targetRotation = _otherDediPlayer._cameraWorldRotation;

                // 현재 회전을 가져옵니다.
                Quaternion currentRotation = _flashLightSource.transform.rotation;

                // 현재 회전의 Euler 각도를 가져옵니다.
                Vector3 eulerAngles = currentRotation.eulerAngles;

                // X축 회전값을 _movementInput._rotationX로 설정합니다.
                float newXRotation = targetRotation.eulerAngles.x;

                // 새로운 회전 부드럽게 값을 적용합니다.
                Quaternion newRotation = Quaternion.Euler(newXRotation, eulerAngles.y, eulerAngles.z);
                _light.transform.rotation = Quaternion.Slerp(currentRotation, newRotation, Time.deltaTime * 40f);

                // Slerp로 이동한 후, 일정한 작은 각도 차이 이하일 경우 최종적으로 newRotation을 확정적으로 설정
                if (Quaternion.Angle(_light.transform.rotation, newRotation) < 0.1f)
                {
                    _light.transform.rotation = newRotation;
                }
                //_flashLightSource.transform.rotation = newRotation;
            }
        }
    }

    public void Init(int itemId,int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }
    
    public void Init(int itemId, int playerId, string englishName, float blindDuration, float flashlightDistance, float flashlightAngle, float flashlightAvailableTime, float flashlightTimeRequired)
    {
        Init(itemId, playerId, englishName);
        BlindDuration = blindDuration;
        FlashlightDistance = flashlightDistance;
        FlashlightAngle = flashlightAngle;
        FlashlightAvailableTime = flashlightAvailableTime;
        FlashlightTimeRequired = flashlightTimeRequired;
    }
    
    public bool Use(IMessage recvPacket = null)
    {

        if (PlayerID == Managers.Player._myDediPlayerId)
        {
            //이미 사용중인데 또 사용하려고 하면, 기존 코루틴 종료하고 코루틴 다시시작
            if (_isLightOn)
            {
                StopCoroutine(_currentPlayingCoroutine);
                _currentPlayingCoroutine = StartCoroutine(LightOffAfterSeconds(FlashlightAvailableTime));
                return true;
            }

            GameObject myDediPlayerGameObject = Managers.Player.GetPlayerObject(PlayerID);

            _flashLightSource = Util.FindChild(myDediPlayerGameObject, "FlashLightSource", true);

            if (_flashLightSource != null)
            {
                GameObject lightGameObject = Util.FindChild(_flashLightSource, "Light", true);
                if (lightGameObject != null)
                {
                    //애니메이션 킴
                    PlayerAnimController anim = Managers.Player.GetAnimator(PlayerID);
                    anim.isFlashlight = true;
                    anim.PlayAnim();

                    //라이트 킴
                    _light = lightGameObject.GetComponent<Light>();
                    _light.enabled = true;
                    _light.range = FlashlightDistance; 
                    _light.spotAngle = FlashlightAngle;

                    //불 킴
                    _movementInput = Managers.Player._myDediPlayer.GetComponent<MovementInput>();
                    _isLightOn = true;

                    //빛 효과 킴
                    _lightEffectMeshRenderer = Util.FindChild(lightGameObject, "LightEffect").gameObject.GetComponent<MeshRenderer>();
                    _lightEffectMeshRenderer.enabled = true;

                    //일정 시간 후 불 끔
                    _currentPlayingCoroutine = StartCoroutine(LightOffAfterSeconds(FlashlightAvailableTime));

                    

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            //이미 사용중인데 또 사용하려고 하면, 기존 코루틴 종료하고 코루틴 다시시작
            if (_isLightOn)
            {
                StopCoroutine(_currentPlayingCoroutine);
                _currentPlayingCoroutine = StartCoroutine(LightOffAfterSeconds(FlashlightAvailableTime));
                return true;
            }

            GameObject otherDediPlayerGameObject = Managers.Player.GetPlayerObject(PlayerID);
            _otherDediPlayer = otherDediPlayerGameObject.GetComponent<OtherDediPlayer>();

            _flashLightSource = Util.FindChild(otherDediPlayerGameObject, "FlashLightSource", true);

            GameObject flashlightGameObject = Util.FindChild(otherDediPlayerGameObject, "3", true);
            if (flashlightGameObject != null)
            {
                GameObject lightGameObject = Util.FindChild(_flashLightSource, "Light", true);
                if (lightGameObject != null)
                {
                    //애니메이션 킴
                    PlayerAnimController anim = Managers.Player.GetAnimator(PlayerID);
                    anim.isFlashlight = true;
                    anim.PlayAnim();

                    //라이트 킴
                    _light = lightGameObject.GetComponent<Light>();
                    _light.enabled = true;
                    _light.range = FlashlightDistance;
                    _light.spotAngle = FlashlightAngle;

                    //불 킴
                    _movementInput = Managers.Player._otherDediPlayers[PlayerID].GetComponent<MovementInput>();
                    _isLightOn = true;

                    //빛 효과 킴
                    _lightEffectMeshRenderer = Util.FindChild(lightGameObject, "LightEffect").gameObject.GetComponent<MeshRenderer>();
                    _lightEffectMeshRenderer.enabled = true;

                    //일정 시간 후 불 끔
                    _currentPlayingCoroutine = StartCoroutine(LightOffAfterSeconds(FlashlightAvailableTime));

                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        Debug.Log("Item Flashlight Use");

        return true;
    }

    IEnumerator LightOffAfterSeconds(float seconds)
    {
        if (PlayerID == Managers.Player._myDediPlayerId)
        {
            //플레이어가 사용한 아이템을 서버로 전송(아이템 사용 패킷 전송)
            CDS_UseFlashlightItem useFlashlightItemPacket = new CDS_UseFlashlightItem()
            {
                MyDediplayerId = PlayerID,
                ItemId = ItemID
            };
            Managers.Network._dedicatedServerSession.Send(useFlashlightItemPacket);
        }

        yield return new WaitForSeconds(seconds);
        
        _isLightOn = false;

        //라이트 끔
        _light.enabled = false;

        //빛 효과 끔
        _lightEffectMeshRenderer.enabled = false;

        //애니메이션 끔
        PlayerAnimController anim = Managers.Player.GetAnimator(PlayerID);
        anim.isFlashlight = false;
        anim.PlayAnim();

        //아이템 들고있는 상태 업뎃
        Managers.Inventory._hotbar.HoldHotbarItem();

        //파괴
        Object.Destroy(gameObject);
    }

    public void OnHold()
    {

    }

    public void OnHit()
    {
        
    }
}
