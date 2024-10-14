using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;

public class SyncMoveController
{
    //키보드 인풋 판별용 비트
    int _runBit = (1 << 4);
    int _upBit = (1 << 3);
    int _leftBit = (1 << 2);
    int _downBit = (1 << 1);
    int _rightBit = 1;
    
    //플레이어 이동속도
    public float _walkSpeed = 2f;
    public float _runSpeed = 3f;
    
    private float _hardSnapDistance = 2f; //하드스냅 거리(이 이상 서버와 거리 차이가 나면 하드스냅)

    private Dictionary<int, bool> _hardSnapOn = new Dictionary<int, bool>(); //하드스냅 기능을 켤지 말지 (대시 등 아이템 사용때문에 추가된 기능)

    public void Clear()
    {
        _hardSnapOn.Clear();
    }

    /// <summary>
    /// 다른 플레이어의 움직임을 동기화 (정확히는 고스트를 데디서버와 동기화시킴)
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="transformInfo"></param>
    /// <param name="keyboardInput"></param>
    public void SyncOtherPlayerMove(DSC_Move packet)
    {
        //패킷정보를 꺼내옴
        int playerId = packet.DediplayerId;

        float posX = packet.TransformInfo.Position.PosX;
        float posY = packet.TransformInfo.Position.PosY;
        float posZ = packet.TransformInfo.Position.PosZ;
        Vector3 pastPosition = new Vector3(posX, posY, posZ);
        float rotX = packet.TransformInfo.Rotation.RotX;
        float rotY = packet.TransformInfo.Rotation.RotY;
        float rotZ = packet.TransformInfo.Rotation.RotZ;
        float rotW = packet.TransformInfo.Rotation.RotW;
        Quaternion pastLocalRotation = new Quaternion(rotX, rotY, rotZ, rotW);

        int keyboardInput = packet.KeyboardInput;

        Vector3 velocity = new Vector3();
        velocity.x = packet.Velocity.X;
        velocity.y = packet.Velocity.Y;
        velocity.z = packet.Velocity.Z;

        DateTime pastDateTime = packet.Timestamp.ToDateTime();

        //카메라 회전값(해당 플레이어의 시야 위아래 표현을 위해서..)
        Quaternion cameraWorldRotation = new Quaternion(packet.CameraWorldRotation.RotX,
            packet.CameraWorldRotation.RotY, packet.CameraWorldRotation.RotZ, packet.CameraWorldRotation.RotW);

        //이미 죽은 플레이어라면 무시
        if (Managers.Player.IsPlayerDead(playerId))
        {
            return;
        }

        //추측항법을 이용해서 위치 예측
        TransformInfo predictedTransformInfo = DeadReckoning(pastDateTime, packet.TransformInfo, velocity);
        Vector3 predictedPosition = new Vector3(predictedTransformInfo.Position.PosX,
            predictedTransformInfo.Position.PosY, predictedTransformInfo.Position.PosZ);


        if (playerId == Managers.Player._myDediPlayerId) //내 플레이어 정보일 경우
        {
            //서버와 차이가 많이나면 서버 정보로 hardsnap
            HardSnap(packet.TransformInfo, playerId);
            return;
        }

        if (Managers.Player._ghosts.TryGetValue(playerId, out GameObject ghostObj)) //다른 플레이어 정보일 경우
        {
            if (HardSnap(packet.TransformInfo, playerId)) //서버와 차이가 많이나면 서버 정보로 hardsnap
            {
                return;
            }
            
            ghostObj.transform.position = predictedPosition; //고스트 위치 갱신

            if (Managers.Player._otherDediPlayers.TryGetValue(playerId, out GameObject playerObj))
            {
                SetPlayerAnimationState(playerId, keyboardInput); //플레이어 뛰는 상태 동기화
                
                //회전해야하는 값 세팅해주기
                playerObj.GetComponent<OtherDediPlayer>()._targetRotation = pastLocalRotation;

                //카메라 회전값(시야 회전값) 저장
                playerObj.GetComponent<OtherDediPlayer>()._cameraWorldRotation = cameraWorldRotation;
            }
        }
    }

    /// <summary>
    /// playerId에 해당하는 오브젝트가 서버위치정보와 차이가 많이 나면 서버위치로 플레이어,고스트 하드스냅
    /// </summary>
    /// <param name="transformInfo">서버가 보내준 위치 정보</param>
    /// <param name="dediPlayerId">검사하려는 데디플레이어id</param>
    /// <returns>하드스냅을 했으면 true, 아니면 false</returns>
    private bool HardSnap(TransformInfo transformInfo, int dediPlayerId)
    {
        if (_hardSnapOn.ContainsKey(dediPlayerId) && _hardSnapOn[dediPlayerId]==false) //하드스냅 기능이 꺼져있으면 무시
        {
            return false;
        }

        //dediPlayerId에 해당하는 오브젝트가 있는지 검사(내 플레이어 id포함)
        if (Managers.Player._myDediPlayerId == dediPlayerId)
        {
            GameObject myDediPlayer = Managers.Player._myDediPlayer;
            //플레이어 오브젝트와 transformInfo의 position정보와의 거리 차이를 구함
            float distance = Vector3.Distance(myDediPlayer.transform.position,
                new Vector3(transformInfo.Position.PosX, transformInfo.Position.PosY, transformInfo.Position.PosZ));

            //거리 차이가 하드스냅 거리보다 크다면 서버 위치,회전으로 내 플레이어 하드스냅
            if (distance > _hardSnapDistance)
            {
                myDediPlayer.transform.position = new Vector3(transformInfo.Position.PosX, transformInfo.Position.PosY,
                    transformInfo.Position.PosZ);
                
                myDediPlayer.transform.rotation = new Quaternion(transformInfo.Rotation.RotX, transformInfo.Rotation.RotY,
                    transformInfo.Rotation.RotZ, transformInfo.Rotation.RotW);
                return true;
            }

            return false;
        }
        else if (Managers.Player._otherDediPlayers.TryGetValue(dediPlayerId, out GameObject dediPlayerObj))
        {
            //플레이어 오브젝트와 transformInfo의 position정보와의 거리 차이를 구함
            float distance = Vector3.Distance(dediPlayerObj.transform.position,
                new Vector3(transformInfo.Position.PosX, transformInfo.Position.PosY, transformInfo.Position.PosZ));

            //거리 차이가 하드스냅 거리보다 크다면 서버 위치,회전으로 플레이어,고스트 하드스냅
            if (distance > _hardSnapDistance)
            {
                if (Managers.Player._ghosts.TryGetValue(dediPlayerId, out GameObject ghostObj)) //고스트 하드스냅
                {
                    CharacterController ghostController = ghostObj.GetComponent<CharacterController>();
                    ghostController.transform.position = new Vector3(transformInfo.Position.PosX, transformInfo.Position.PosY,
                        transformInfo.Position.PosZ);
                    ghostObj.transform.rotation = new Quaternion(transformInfo.Rotation.RotX, transformInfo.Rotation.RotY,
                        transformInfo.Rotation.RotZ, transformInfo.Rotation.RotW);
                }
                
                //플레이어 하드스냅
                dediPlayerObj.transform.position = new Vector3(transformInfo.Position.PosX, transformInfo.Position.PosY,
                    transformInfo.Position.PosZ);
                dediPlayerObj.transform.rotation = new Quaternion(transformInfo.Rotation.RotX, transformInfo.Rotation.RotY,
                    transformInfo.Rotation.RotZ, transformInfo.Rotation.RotW);
                
                return true;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// 과거의 정보를 갖고 데드레커닝을 통해 현재 위치를 예측하는 함수
    /// </summary>
    /// <param name="pastDateTime">과거 시간</param>
    /// <param name="pastTransform">과거 트랜스폼</param>
    /// <param name="pastVelocity">과거 속도</param>
    /// <returns>예측된 위치(회전은 고려 x)</returns>
    public TransformInfo DeadReckoning(DateTime pastDateTime, TransformInfo pastTransform, Vector3 pastVelocity)
    {
        //현재 DateTime과 과거 DateTime의 차이를 구함
        TimeSpan timeSpan = DateTime.UtcNow - pastDateTime;
        
        //단순 시간계산만으로 위치를 예측하면 끊기듯이 이동하기 때문에 보정을 이용해 더 이동해줘야 함
        float alpha = 1.3f;

        //과거 위치를 기준으로 과거 속도를 이용해 예측 위치를 구함
        float posX = pastTransform.Position.PosX + pastVelocity.x * (float)timeSpan.TotalSeconds * alpha;
        float posZ = pastTransform.Position.PosZ + pastVelocity.z * (float)timeSpan.TotalSeconds * alpha;

        //그 결과를 리턴
        TransformInfo transformInfo = new TransformInfo();
        PositionInfo positionInfo = new PositionInfo();
        positionInfo.PosX = posX;
        positionInfo.PosY = pastTransform.Position.PosY;
        positionInfo.PosZ = posZ;
        transformInfo.Position = positionInfo;
        transformInfo.Rotation = pastTransform.Rotation;

        return transformInfo;
    }
    
    /// <summary>
    /// 다른 플레이어 애니메이션에 필요한 변수 세팅
    /// </summary>
    /// <param name="dediPlayerId">데디플레이어id</param>
    /// <param name="keyboardInput">키보드인풋비트</param>
    public void SetPlayerAnimationState(int dediPlayerId,int keyboardInput)
    {
        if (Managers.Player._otherDediPlayers.TryGetValue(dediPlayerId, out GameObject playerObj))
        {
            playerObj.GetComponent<OtherDediPlayer>()._isRunning = (keyboardInput & _runBit) != 0;
            playerObj.GetComponent<OtherDediPlayer>()._isWalking =
                (keyboardInput & (_upBit | _leftBit | _downBit | _rightBit)) != 0;
        }
    }

    /// <summary>
    /// 하드스냅 기능을 토글하는 함수
    /// </summary>
    /// <param name="isOn">하드스냅을 킬거면 true, 끌거면 false</param>
    public void ToggleHardSnap(int dediPlayerId , bool isOn)
    {
        if (_hardSnapOn == null)
        {
            _hardSnapOn = new Dictionary<int, bool>();
        }

        if (_hardSnapOn.ContainsKey(dediPlayerId))
        {
            _hardSnapOn[dediPlayerId] = isOn;
        }
        else
        {
            _hardSnapOn.Add(dediPlayerId, isOn);
        }
    }

    //이거 전에 고스트에서 계속 이동할 속도 계산할때 썻던 것
    public void CalculateVelocity(int keyboardInput, Quaternion localRotation)
    {
        /*Vector3 velocity;
        bool isRunning = false;
        Vector2 moveInputVector = new Vector2();
        moveInputVector.x =
            (keyboardInput & (_leftBit | _rightBit)) == 0 ? 0 : (keyboardInput & _leftBit) == 0 ? 1 : -1;
        moveInputVector.y = (keyboardInput & (_downBit | _upBit)) == 0 ? 0 : (keyboardInput & _downBit) == 0 ? 1 : -1;

        //방향키가 아무것도 안눌렀다면
        if ((keyboardInput & (_upBit | _downBit | _leftBit | _rightBit)) == 0)
        {
            velocity = Vector3.zero;
        }
        else
        {
            if ((keyboardInput & _runBit) != 0)
            {
                isRunning = true;
            }

            if (isRunning)
            {
                velocity = localRotation.normalized * new Vector3( moveInputVector.x, 0,  moveInputVector.y).normalized * _runSpeed;
            }
            else
            {
                velocity = localRotation.normalized * new Vector3( moveInputVector.x, 0,  moveInputVector.y).normalized * _walkSpeed;
            }
        }
        _velocity = velocity;*/
    }
}