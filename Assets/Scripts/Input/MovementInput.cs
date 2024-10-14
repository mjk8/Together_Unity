using System;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementInput : MonoBehaviour
{
    int _runBit = (1 << 4);
    int _upBit = (1 << 3);
    int _leftBit = (1 << 2);
    int _downBit = (1 << 1);
    int _rightBit = 1;
    
    
    public static Vector2 _moveInput;
    static int sensitivityAdjuster = 3;
    static float _walkSpeed = 2f;
    static float _runSpeed = 3f;
    public static float _minViewDistance = 50f;
    static float _mouseSensitivity;
    public float _rotationX = 0f;
    public Vector3 _velocity;

    CharacterController _controller;
    private Transform _camera;
    private Transform _player;
    private Transform _prefab; //얘가 프리팹 본체
    PlayerAnimController _playerAnimController;

    public static bool _isRunning = false;
    private void ChangeAnim()
    {
        _playerAnimController.PlayAnim();
    }
    
    void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
        _playerAnimController.isWalking = _moveInput.magnitude > 0;
        ChangeAnim();
    }

    void OnRun(InputValue value)
    {
        _isRunning = value.isPressed;
        _playerAnimController.isWalking = _isRunning;
        ChangeAnim();
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _playerAnimController = transform.GetComponentInChildren<PlayerAnimController>();
        _mouseSensitivity = Managers.Data.Player.MouseSensitivity *sensitivityAdjuster;
        _prefab = gameObject.transform;
        _camera = _prefab.transform.GetChild(0);
        _player = _prefab.transform.GetChild(1);
        _velocity = new Vector3(0f,0f,0f);
        Managers.Job.Push(SendMove); //50ms 마다 보냄 (초당 20번)
    }
    
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        _rotationX -= mouseY;
        _rotationX = Mathf.Clamp(_rotationX, -70f, _minViewDistance);
        
        _camera.localRotation = Quaternion.Euler(_rotationX,0f,0f);
        _prefab.Rotate(3f * mouseX * Vector3.up);
        /*if (_moveInput.magnitude<=0)
        {
            _player.transform.Rotate(3f * -mouseX * Vector3.up);
        }
        if (_moveInput.magnitude > 0)
        {
            _player.transform.localRotation =
                Quaternion.AngleAxis(Mathf.Atan2(_moveInput.x, _moveInput.y) * Mathf.Rad2Deg, Vector3.up);
        }*/
        //_player.transform.localRotation =Quaternion.AngleAxis(Mathf.Atan2(_moveInput.x, _moveInput.y) * Mathf.Rad2Deg, Vector3.up);
        
        _velocity= CalculateVelocity(_moveInput, _prefab.localRotation);
        _controller.Move(_velocity * Time.deltaTime);
    }


    void SendMove()
    {
        //만약 내가 죽었다면 보내지 않고 그냥 무시
        if (Managers.Player.IsMyPlayerDead())
        {
            return;
        }
        
        
        CDS_Move packet = new CDS_Move();
        
        packet.DediplayerId = Managers.Player._myDediPlayerId;
        
        TransformInfo ghostTransformInfo = new TransformInfo();
        PositionInfo ghostPositionInfo = new PositionInfo();
        RotationInfo ghostRotationInfo = new RotationInfo();
        
        Vector3 position = _prefab.transform.position;
        Quaternion rotation = _prefab.transform.rotation;
        ghostPositionInfo.PosX = position.x;
        ghostPositionInfo.PosY = position.y;
        ghostPositionInfo.PosZ = position.z;
        ghostRotationInfo.RotX = rotation.x;
        ghostRotationInfo.RotY = rotation.y;
        ghostRotationInfo.RotZ = rotation.z;
        ghostRotationInfo.RotW = rotation.w;
        
        ghostTransformInfo.Position = ghostPositionInfo;
        ghostTransformInfo.Rotation = ghostRotationInfo;
        
        packet.TransformInfo = ghostTransformInfo;
        //Debug.Log($"보내는 패킷의 posY값 : {packet.TransformInfo.Position.PosY}");
        
        int moveBit = 0;
        if (_isRunning)
        {
            //Debug.Log("달리기 키 눌림");
            moveBit |= _runBit;
        }
        if (_moveInput.y > 0.5f) //윗키눌림
        {
            moveBit |= _upBit;
        }
        if(_moveInput.y < -0.5f) //아래키눌림
        {
            moveBit |= _downBit;
        }
        if(_moveInput.x < -0.5f) //왼쪽키눌림
        {
            moveBit |= _leftBit;
        }
        if(_moveInput.x > 0.5f) //오른쪽키눌림
        {
            moveBit |= _rightBit;
        }
        packet.KeyboardInput = moveBit;

        //캐릭터컨트롤러를 사용해서 현재 velocity를 구함
        //packet.Velocity = new Vector3(_velocity.x, _velocity.y, _velocity.z);
        Vector3 velocityTemp = CalculateVelocity(_moveInput, _prefab.localRotation);
        Velocity velocity = new Velocity();
        velocity.X = velocityTemp.x;
        velocity.Y = velocityTemp.y;
        velocity.Z = velocityTemp.z;
        packet.Velocity = velocity;

        packet.Timestamp = Managers.Time.GetDediServerTime().ToTimestamp();

        packet.CameraWorldRotation = new RotationInfo()
        {
            RotX = _camera.rotation.x,
            RotY = _camera.rotation.y,
            RotZ = _camera.rotation.z,
            RotW = _camera.rotation.w
        };
        
        Managers.Network._dedicatedServerSession.Send(packet);
        
        //초당 20번 보냄
        Managers.Job.Push(SendMove, 50);
    }

    private Vector3 CalculateVelocity(Vector2 moveInputVector, Quaternion prefabRotation)
    {
        Vector3 velocity;
        if (_isRunning)
        {
            velocity = prefabRotation.normalized * new Vector3( moveInputVector.x, 0,  moveInputVector.y) * _runSpeed;
        }
        else
        {   
            velocity = prefabRotation.normalized * new Vector3( moveInputVector.x, 0,  moveInputVector.y) * _walkSpeed;
        }

        velocity.y = -10f;

        return velocity;
    }

}
