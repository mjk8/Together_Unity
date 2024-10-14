using System;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using UnityEngine;

public class Dash : MonoBehaviour, IItem
{
    //IItem 인터페이스 구현
    public int ItemID { get; set; }
    public int PlayerID { get; set; }
    public string EnglishName { get; set; }


    //이 아이템만의 속성
    public float DashDistance { get; set; }


    private GameObject _player;
    private CharacterController _characterController;
    private GameObject _survivorTrigger;
    private float _dashTime = 0.35f; //대시 시간(애니메이션 재생 시간) (무적시간이기도 함)
    private float _dashSpeed; //대시 속도
    private bool _isDashing = false; //대시 중인지 여부

    void Update()
    {
        if (_isDashing && !Managers.Player.IsPlayerDead(PlayerID))
        {
            //대시 속도만큼 이동
            _characterController.Move(_player.transform.forward * _dashSpeed * Time.deltaTime);
            _dashTime -= Time.deltaTime;
            if (_dashTime <= 0)
            {
                _isDashing = false;

                //무적 풀기(KillerTrigger의 캡슐콜라이더를 킴)
                _survivorTrigger.GetComponent<CapsuleCollider>().enabled = true;

                //내 플레이어라면 인풋 다시 받게 하는 코드 추가
                if (PlayerID == Managers.Player._myDediPlayerId)
                {
                    Managers.Player.ActivateInput();
                }

                //다른 플레이어라면 고스트 따라가기 재개 코드 추가
                else if(Managers.Player._otherDediPlayers.ContainsKey(PlayerID))
                {
                    Managers.Player._otherDediPlayers[PlayerID].GetComponent<OtherDediPlayer>().ToggleFollowGhost(true);
                }

                //아이템 들고있는 상태 업뎃
                Managers.Inventory._hotbar.HoldHotbarItem();

                //대시가 끝났으므로 대시오브젝트 삭제
                Destroy(gameObject);
            }
        }
    }
    
    public void Init(int itemId, int playerId, string englishName)
    {
        this.ItemID = itemId;
        this.PlayerID = playerId;
        this.EnglishName = englishName;
    }

    public void Init(int itemId, int playerId, string englishName, float dashDistance)
    {
        Init(itemId,playerId, englishName);
        DashDistance = dashDistance;
    }

    public bool Use(IMessage recvPacket = null)
    {
        Managers.Player.GetAnimator(PlayerID).SetTriggerByString(EnglishName);
        Debug.Log("Item Dash Use");

        
        if (PlayerID == Managers.Player._myDediPlayerId)
        {
            _player = Managers.Player._myDediPlayer;
            //대시 패킷 서버로 보내기
            CDS_UseDashItem useDashItemPacket = new CDS_UseDashItem()
            {
                MyDediplayerId = PlayerID,
                ItemId = ItemID,
                DashStartingTransform = new TransformInfo()
                {
                    Position = new PositionInfo()
                    {
                        PosX = _player.transform.position.x,
                        PosY = _player.transform.position.y,
                        PosZ = _player.transform.position.z
                    },
                    Rotation = new RotationInfo()
                    {
                        RotX = _player.transform.rotation.x,
                        RotY = _player.transform.rotation.y,
                        RotZ = _player.transform.rotation.z,
                        RotW = _player.transform.rotation.w
                    }
                }
            };
            Managers.Network._dedicatedServerSession.Send(useDashItemPacket);

            //내 플레이어라면 인풋 막는 코드 추가
            Managers.Player.DeactivateInput();
        }
        else
        {
            _player = Managers.Player._otherDediPlayers[PlayerID];

            //다른 플레이어라면 고스트 따라가기 기능 멈추는 코드 추가
            Managers.Player._otherDediPlayers[PlayerID].GetComponent<OtherDediPlayer>().ToggleFollowGhost(false);
        }
        _characterController = _player.GetComponent<CharacterController>();

        //무적 처리(KillerTrigger의 캡슐콜라이더를 끔)
        _survivorTrigger = Util.FindChild(_player, "SurvivorTrigger", true);
        _survivorTrigger.GetComponent<CapsuleCollider>().enabled = false;

        //하드스냅 정지 코드 추가 (하드스냅 재개는 서버로부터 대시완료 패킷 받은 후 풀어야만 함)
        Managers.Player._syncMoveController.ToggleHardSnap(PlayerID,false);

        //DashDistance만큼의 거리를 dashTime동안 이동하려면 속도가 몇이어야 하는지
        _dashSpeed = DashDistance / _dashTime;

        //대시 시작(update문에서 대시 수행)
        _isDashing = true;

        return true;
    }

    public void OnHold()
    {
    }
    
    public void OnHit()
    {
    }
}