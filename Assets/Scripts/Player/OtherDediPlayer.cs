using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class OtherDediPlayer : MonoBehaviour
{
    //현재 데디서버의 playerId는 독자적인 값(=데디서버의 sessionID)으로 처리하고 있음
    public int PlayerId { get; set; }
    public string Name { get; set; }

    public Quaternion _cameraWorldRotation = Quaternion.identity; //카메라의 월드 회전값

    public bool _isKiller = false; //킬러 여부
    public int _killerType = -1; //어떤 킬러타입인지를 나타내는 ID
    
    public float _gauge = 0; //생명력 게이지
    public float _gaugeDecreasePerSecond = 0; //생명력 게이지 감소량

    private PlayerAnimController _playerAnimController;
    CharacterController _controller;
    public GameObject _ghost;
    public Vector3 _velocity;
    public bool _isRunning = false;
    public bool _isWalking = false;
    public Quaternion _targetRotation; //서버에서 받은 목표 회전값. 이 값으로 update문에서 회전시킴
    
    public int _totalPoint = 0; //상자로 얻은 총 포인트(낮마다 초기화)
    public int _currentItemID = -1; //현재 가지고 있는 아이템 ID

    private bool _isGhostFollowingOn = true; //고스트 따라가기 여부

    public PlayerStatus _playerStatus; //플레이어의 상태

    public void Init(int playerId, string name)
    {
        PlayerId = playerId;
        Name = name;
        _isKiller = false;
        _playerStatus = new PlayerStatus();
    }

    private void Start()
    {
        _playerAnimController = transform.GetChild(0).GetComponent<PlayerAnimController>();
        _controller = GetComponent<CharacterController>();
        _velocity = new Vector3(0f, 0f, 0f);
        _isRunning = false;
    }

    private void Update()
    {
        if (_ghost == null)
        {
            _ghost = GameObject.Find("Ghost_" + PlayerId);
        }
        SyncAnim();
        FollowGhost();
    }

    /// <summary>
    /// 자신의 ghost를 따라서 자연스럽게 움직이는 코드
    /// </summary>
    private void FollowGhost()
    {
        if (_ghost != null)
        {
            //목표 방향으로 회전합니다.
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, Time.deltaTime * 30f);
            
            // 목표 방향을 계산합니다. _ghost.transform.position과 transform.position의 높이차는 고려하지 않고 x,z만 고려
            Vector3 directionToGhost = _ghost.transform.position - transform.position;
            directionToGhost.y = 0; // Y축을 고려하지 않음

            //목표 위치까지 거리가 beta보다 작으면 도착한것으로 간주하고 멈춤
            float beta = 0.05f;
            if (directionToGhost.magnitude < beta)
            {
                _velocity = Vector3.zero;
                _controller.Move(_velocity);
                return;
            }

            // 목표 방향으로 이동합니다.
            _velocity = directionToGhost.normalized;
            if (_isRunning)
            {
                _velocity *= Managers.Player._syncMoveController._runSpeed;
            }
            else
            {
                _velocity *= Managers.Player._syncMoveController._walkSpeed;
            }
            
            _velocity.y = -10f; //중력 같은 효과
            
            _controller.Move(_velocity * Time.deltaTime);
        }
    }

    public void SyncAnim()
    {
        _playerAnimController.isRunning = _isRunning;
        _playerAnimController.isWalking = _isWalking;
        _playerAnimController.PlayAnim();
    }

    /// <summary>
    /// 고스트 따라가기를 토글함.
    /// </summary>
    /// <param name="isFollow">따라가게 할거면 true, 안따라가게 할거면 false</param>
    public void ToggleFollowGhost(bool isFollow)
    {
        if (_ghost != null)
        {
            _isGhostFollowingOn = isFollow;
        }
    }
}
