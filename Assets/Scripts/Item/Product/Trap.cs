using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Google.Protobuf.Protocol;
using UnityEngine;

public class Trap : MonoBehaviour, IItem
{
    //IItem 인터페이스 구현
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }

    public float TrapDuration { get; set; }
    public float TrapRadius { get; set; }
    public float StunDuration { get; set; }


    public string _trapId; //트랩 고유번호(구분용. Trap{_trapId}로 이름 지어줌)

    private GameObject _myDediPlayer;
    private float _forwardRayDistance = 0.8f;
    private float _downRayDistance = 0.2f;
    private bool _onHoldTrigger = false;
    private bool _isMyPlayerTrapAvailable = false; //현재 트랩을 내 캐릭터가 설치할 수 있는지를 확인하는 변수
    private float _setTrapSeconds = 1f; //트랩 설치 시간

    public bool _isAlreadyTrapped = false; //이미 누군가가 트랩에 걸렸는지 여부

    public void Init(int itemId,  int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }

    public void Init(int itemId, int playerId, string englishName, float trapDuration, float trapRadius, float stunDuration)
    {
        Init(itemId,playerId,englishName);
        TrapDuration = trapDuration;
        TrapRadius = trapRadius;
        StunDuration = stunDuration;
    }

    public void LateUpdate()
    {
        if (_onHoldTrigger)
        {
            // _myDediPlayer의 forward 방향으로 ray를 쏴서 아무 것도 없고 && 그곳에서부터 아래로 ray를 쏴서 땅이 닿는다면 _isMyPlayerTrapAvailable를 true로 설정
            RaycastHit hit;
            Vector3 forwardRayStart = _myDediPlayer.transform.position;
            Vector3 forwardRayDirection = _myDediPlayer.transform.forward;
            float forwardRayDistance = _forwardRayDistance;

            if (Physics.Raycast(forwardRayStart, forwardRayDirection, out hit, forwardRayDistance))
            {
                _isMyPlayerTrapAvailable = false;

                // ray가 충돌한 위치에서부터 아래로 ray를 쏘기
                Vector3 downRayStart = hit.point;
                Vector3 downRayDirection = Vector3.down;
                float downRayDistance = _downRayDistance;

                if (Physics.Raycast(downRayStart, downRayDirection, out hit, downRayDistance))
                {
                    // 땅에 설치 불가능하다는 시각적 표시
                    gameObject.GetComponent<MeshRenderer>().enabled = false;
                    gameObject.GetComponent<SphereCollider>().enabled = false;
                    gameObject.transform.Find("TrapGreen").GetComponent<MeshRenderer>().enabled = false;
                    gameObject.transform.Find("TrapRed").GetComponent<MeshRenderer>().enabled = true;

                    gameObject.transform.position = hit.point;
                }
                else
                {
                    // 밑이 허공이니까 딱히 아무것도 표시하지 않아도 됨 (원래 있던걸 다 비활성화)
                    gameObject.GetComponent<MeshRenderer>().enabled = false;
                    gameObject.GetComponent<SphereCollider>().enabled = false;
                    gameObject.transform.Find("TrapGreen").GetComponent<MeshRenderer>().enabled = false;
                    gameObject.transform.Find("TrapRed").GetComponent<MeshRenderer>().enabled = false;
                }
            }
            else
            {
                // 앞 ray가 충돌하지 않았다면 해당 ray의 끝점에서 아래로 ray를 쏴서 땅이 닿는지 확인
                Vector3 downRayStart = forwardRayStart + forwardRayDirection * forwardRayDistance;
                Vector3 downRayDirection = Vector3.down;
                float downRayDistance = _downRayDistance;

                if (Physics.Raycast(downRayStart, downRayDirection, out hit, downRayDistance))
                {
                    _isMyPlayerTrapAvailable = true;

                    // 땅에 설치 가능하다는 시각적 표시
                    gameObject.GetComponent<MeshRenderer>().enabled = false;
                    gameObject.GetComponent<SphereCollider>().enabled = false;
                    gameObject.transform.Find("TrapGreen").GetComponent<MeshRenderer>().enabled = true;
                    gameObject.transform.Find("TrapRed").GetComponent<MeshRenderer>().enabled = false;

                    gameObject.transform.position = hit.point;
                }
                else
                {
                    _isMyPlayerTrapAvailable = false;

                    // 밑이 허공이니까 딱히 아무것도 표시하지 않아도 됨 (원래 있던걸 다 비활성화)
                    gameObject.GetComponent<MeshRenderer>().enabled = false;
                    gameObject.GetComponent<SphereCollider>().enabled = false;
                    gameObject.transform.Find("TrapGreen").GetComponent<MeshRenderer>().enabled = false;
                    gameObject.transform.Find("TrapRed").GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
    }
    
    public bool Use(IMessage recvPacket = null)
    {
        if (PlayerID == Managers.Player._myDediPlayerId && _isMyPlayerTrapAvailable)
        {
            _onHoldTrigger = false;
            _isMyPlayerTrapAvailable = false;

            //내 트랩id 설정 {플레이어id}_{트랩id}
            TrapFactory trapFactory = Managers.Item._itemFactories[4] as TrapFactory;
            _trapId = PlayerID + "_" + trapFactory._trapId;
            gameObject.name = "Trap" + _trapId;
            trapFactory._trapId++;

            //트랩이 설치될 트랜스폼을 패킷에 담아서 서버로 전송
            CDS_UseTrapItem useTrapItemPacket = new CDS_UseTrapItem()
            {
                MyDediplayerId = PlayerID,
                ItemId = ItemID,
                TrapTransform = new TransformInfo()
                {
                    Position = new PositionInfo()
                    {
                        PosX = gameObject.transform.position.x,
                        PosY = gameObject.transform.position.y,
                        PosZ = gameObject.transform.position.z
                    },
                    Rotation = new RotationInfo()
                    {
                        RotX = gameObject.transform.rotation.x,
                        RotY = gameObject.transform.rotation.y,
                        RotZ = gameObject.transform.rotation.z,
                        RotW = gameObject.transform.rotation.w
                    }
                },
                TrapId = _trapId
            };
            Managers.Network._dedicatedServerSession.Send(useTrapItemPacket);

            StartCoroutine(SetTrapDuringSeconds(_setTrapSeconds, null));

            Debug.Log("Item Trap Use");
            return true;
        }
        else if(PlayerID != Managers.Player._myDediPlayerId)
        {
            DSC_UseTrapItem recvPacketTrap = recvPacket as DSC_UseTrapItem;
            _trapId = recvPacketTrap.TrapId;
            gameObject.name = "Trap" + _trapId;

            StartCoroutine(SetTrapDuringSeconds(_setTrapSeconds, recvPacketTrap));

            Debug.Log("Item Trap Use");
            return true;
        }
        
        Object.Destroy(gameObject);
        return false;
    }

    IEnumerator SetTrapDuringSeconds(float animationSeconds, DSC_UseTrapItem recvPacketTrap)
    {
        //설치하는 동안은 키 입력 막음
        if (PlayerID == Managers.Player._myDediPlayerId)
        {
            Managers.Player.DeactivateInput();
        }
        //애니메이션 재생
        Managers.Player.GetAnimator(PlayerID).SetTriggerByString(EnglishName);

        yield return new WaitForSeconds(animationSeconds);

        //아이템 들고있는 상태 업뎃
        Managers.Inventory._hotbar.HoldHotbarItem();

        //트랩 보이게 함
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<SphereCollider>().enabled = true;
        gameObject.transform.Find("TrapGreen").GetComponent<MeshRenderer>().enabled = false;
        gameObject.transform.Find("TrapRed").GetComponent<MeshRenderer>().enabled = false;

        if (PlayerID == Managers.Player._myDediPlayerId)
        {
            //SpereCollider의 반지름을 TrapRadius로 설정
            gameObject.GetComponent<SphereCollider>().radius = TrapRadius;
        }
        else
        {
            //패킷에 담긴 설치 트랜스폼으로 트랩 이동
            gameObject.transform.position = new Vector3(recvPacketTrap.TrapTransform.Position.PosX, recvPacketTrap.TrapTransform.Position.PosY, recvPacketTrap.TrapTransform.Position.PosZ);
            gameObject.transform.rotation = new Quaternion(recvPacketTrap.TrapTransform.Rotation.RotX, recvPacketTrap.TrapTransform.Rotation.RotY, recvPacketTrap.TrapTransform.Rotation.RotZ, recvPacketTrap.TrapTransform.Rotation.RotW);

            //서버 레이턴시 고려해서 덫 설치된 시간을 계산
            float estimatedTrapDuration = TrapDuration - Managers.Time.GetEstimatedLatency();
            TrapDuration = estimatedTrapDuration;
        }

        //설치가 끝나면 키 입력 풀어줌
        if (PlayerID == Managers.Player._myDediPlayerId)
        {
            Managers.Player.ActivateInput();
        }

        //TrapDuration 이후 덫 사라짐
        StartCoroutine(DestroyAfterSeconds(TrapDuration));
    }

    IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }

    public void OnHold()
    {
        if (PlayerID != Managers.Player._myDediPlayerId)
        {
            Destroy(gameObject);
        }

        _myDediPlayer = Managers.Player._myDediPlayer;

        _onHoldTrigger = true;
    }

    public void OnHit()
    {
        StopCoroutine("DestroyAfterSeconds");
        float estimatedStunDuration = StunDuration - Managers.Time.GetEstimatedLatency();
        StartCoroutine(DestroyAfterSeconds(estimatedStunDuration));
    }
}
