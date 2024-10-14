using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ClientGauge : MonoBehaviour
{

    public float _gaugeMax = 0; //게이지 최대값
    public float _hardSnapMargin = 0.05f; //HardSanp 기준 시간 / Update 단위
    public static GaugeActivator _gaugeActivator;

    // 밤 시작 패킷 받을 시, 게이지 Max와 현재 게이지 채우기
    public void Init()
    {
        _gaugeActivator = transform.AddComponent<GaugeActivator>();
        Managers.UI.GetComponentInSceneUI<InGameUI>().SetMaxGauge(GetMyGauge());
    }
    
    
    //서버에서 밤 종료 패킷 받을 시 게이지 처리
    public void EndGauge()
    {
        Destroy(_gaugeActivator);
    }
    
    //서버 시간과 비교. hardsnap 마진보다 차이 날 시 hardsnap
    public void CheckHardSnap(int dediPlayerId, float serverGaugeValue)
    {
        //죽은 플레이어는 게이지처리 필요없음
        if (Managers.Player.IsPlayerDead(dediPlayerId))
        {
            return;
        }

        if (Math.Abs(serverGaugeValue - GetGauge(dediPlayerId)) >= _hardSnapMargin)
        {
            SetGauge(dediPlayerId, serverGaugeValue);
            if (Managers.Player._myDediPlayerId == dediPlayerId)
            {
                Managers.UI.GetComponentInSceneUI<InGameUI>().SetCurrentGauge();
            }
        }
    }
    

    /// <summary>
    /// <para>모든 플레이어의 gauge를 본인의 _gaugeDecreasePerSecond만큼 감소시킴.</para>
    /// <para>만약 감소시킨 결과가 0보다 작다면 0으로 설정.</para>
    /// <para>time.deltatime적용된 상태</para>
    /// </summary>
    public void DecreaseAllGaugeAuto()
    {
        DecreaseGauge(Managers.Player._myDediPlayerId, GetGaugeDecreasePerSecond(Managers.Player._myDediPlayerId) * Time.deltaTime);
        
        foreach (int dediPlayerId in Managers.Player._otherDediPlayers.Keys)
        {
            DecreaseGauge(dediPlayerId, GetGaugeDecreasePerSecond(dediPlayerId) * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// 특정 플레이어의 gauge를 감소시킴. 만약 감소시킨 결과가 0보다 작다면 0으로 설정
    /// </summary>
    /// <param name="dediPlayerId">데디 플레이어id</param>
    /// <param name="amount">얼만큼 감소시킬 것인가</param>
    /// <returns>감소된 게이지 결과. 존재하지 않는 플레이어라면 -1 반환</returns>
    public float DecreaseGauge(int dediPlayerId, float amount)
    {
        float gauge = GetGauge(dediPlayerId);
        if (gauge == -1)
            return -1;
        
        gauge -= amount;
        SetGauge(dediPlayerId, gauge);
        return GetGauge(dediPlayerId);
    }
    
    
    /// <summary>
    /// 특정 플레이어의 gauge를 증가시킴. 만약 증가시킨 결과가 _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="dediPlayerId">데디 플레이어id</param>
    /// <param name="amount">얼만큼 증가시킬 것인가</param>
    /// <returns>증가된 게이지 결과. 존재하지 않는 플레이어라면 -1 반환</returns>
    public float IncreaseGauge(int dediPlayerId, float amount)
    {
        float gauge = GetGauge(dediPlayerId);
        if (gauge == -1)
            return -1;
        
        gauge += amount;
        SetGauge(dediPlayerId, gauge);
        return GetGauge(dediPlayerId);
    }
    
    /// <summary>
    /// 내플레이어의 gauge를 증가시킴. 만약 증가시킨 결과가 _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="amount">얼만큼 증가시킬 것인가</param>
    /// <returns>증가된 게이지 결과. 문제 있으면 -1반환</returns>
    public float IncreaseMyGauge(float amount)
    {
        return IncreaseGauge(Managers.Player._myDediPlayerId, amount);
    }
    
    
    /// <summary>
    /// 특정 플레이어의 _gaugeDecreasePerSecond를 반환함
    /// </summary>
    /// <param name="dediPlayerId">데디플레이어id</param>
    /// <returns>존재하지 않는 플레이어면 -1 반환</returns>
    public float GetGaugeDecreasePerSecond(int dediPlayerId)
    {
        if (Managers.Player._myDediPlayerId == dediPlayerId)
        {
            return Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._gaugeDecreasePerSecond;
        }
        else
        {
            if (Managers.Player._otherDediPlayers.ContainsKey(dediPlayerId))
            {
                return Managers.Player._otherDediPlayers[dediPlayerId].GetComponent<OtherDediPlayer>()._gaugeDecreasePerSecond;
            }
        }

        return -1;
    }

    /// <summary>
    /// 모든 플레이어의 초당 게이지 감소량을 설정
    /// </summary>
    /// <param name="playerGaugeDecreasePerSecond">서버에서 온 map형식의 플레이어별 게이지 감소량 정보</param>
    public void SetAllGaugeDecreasePerSecond(MapField<int,float> playerGaugeDecreasePerSecond)
    {
        foreach (int playerId in playerGaugeDecreasePerSecond.Keys)
        {
            SetGaugeDecreasePerSecond(playerId, playerGaugeDecreasePerSecond[playerId]);
        }
    }

    /// <summary>
    /// 특정 플레이어의 초당 게이지 감소량을 설정
    /// </summary>
    /// <param name="dediPlayerId">플레이어id</param>
    /// <param name="gaugeDecreasePerSecond">설정할 게이지 감소량</param>
    private void SetGaugeDecreasePerSecond(int dediPlayerId, float gaugeDecreasePerSecond)
    {
        if (Managers.Player._myDediPlayerId == dediPlayerId)
        {
            Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._gaugeDecreasePerSecond = gaugeDecreasePerSecond;
        }
        else
        {
            if (Managers.Player._otherDediPlayers.ContainsKey(dediPlayerId))
                Managers.Player._otherDediPlayers[dediPlayerId].GetComponent<OtherDediPlayer>()._gaugeDecreasePerSecond = gaugeDecreasePerSecond;
        }
    }

    
    /// <summary>
    /// 특정 플레이어의 게이지를 반환함. 존재하지 않는 플레이어라면 -1 반환
    /// </summary>
    /// <param name="dediPlayerId">데디 플레이어id</param>
    /// <returns></returns>
    public float GetGauge(int dediPlayerId)
    {
        if (Managers.Player._myDediPlayerId == dediPlayerId)
        {
            return Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._gauge;
        }
        else
        {
            if (Managers.Player._otherDediPlayers.ContainsKey(dediPlayerId))
            {
                return Managers.Player._otherDediPlayers[dediPlayerId].GetComponent<OtherDediPlayer>()._gauge;
            }
        }

        return -1;
    }
    
    /// <summary>
    /// 내 데디플레이어의 게이지를 반환함. 문제 있을시 -1 반환
    /// </summary>
    /// <returns></returns>
    public float GetMyGauge()
    {
        return GetGauge(Managers.Player._myDediPlayerId);
    }
    
    /// <summary>
    /// 모든 플레이어의 게이지를 amount로 설정함. 만약 amount가 0보다 작다면 0으로 설정, _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="amount">설정할 게이지 값</param>
    public void SetAllGauge(float amount)
    {
        SetGauge(Managers.Player._myDediPlayerId, amount);
        foreach (int dediPlayerId in Managers.Player._otherDediPlayers.Keys)
        {
            SetGauge(dediPlayerId, amount);
        }
    }
    
    /// <summary>
    /// 특정 플레이어의 gauge를 amount로 설정함. 만약 amount가 0보다 작다면 0으로 설정, _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="dediPlayerId">데디플레이어id</param>
    /// <param name="amount">설정할 게이지 값</param>
    public void SetGauge(int dediPlayerId, float amount)
    {
        if (Managers.Player._myDediPlayerId == dediPlayerId)
        {
            Managers.Player._myDediPlayer.GetComponent<MyDediPlayer>()._gauge = Mathf.Clamp(amount, 0, _gaugeMax);
        }
        else
        {
            Managers.Player._otherDediPlayers[dediPlayerId].GetComponent<OtherDediPlayer>()._gauge = Mathf.Clamp(amount, 0, _gaugeMax);
        }
    }
    
    /// <summary>
    /// 내 데디플레이어의 게이지를 amount로 설정함. 만약 amount가 0보다 작다면 0으로 설정, _gaugeMax보다 크다면 _gaugeMax로 설정
    /// </summary>
    /// <param name="amount">설정할 게이지 값</param>
    public void SetMyGauge(float amount)
    {
        SetGauge(Managers.Player._myDediPlayerId, amount);
        Managers.UI.GetComponentInSceneUI<InGameUI>().SetCurrentGauge();
    }
}
