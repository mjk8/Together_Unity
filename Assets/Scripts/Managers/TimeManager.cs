using System;
using Google.Protobuf.Protocol;

public class TimeManager
{
    private TimeSpan _dediserverClientTimeDelta; //데디서버와 클라이언트의 시간차이. 클라 타임스탬프에 이값을 더하면 대충 데디서버 시간이라고 추론 가능
    
    
    private DateTime _rttStartTime; //RTT 측정 시작 시간
    private DateTime _rttEndTime; //RTT 측정 종료 시간
    
    private TimeSpan _estimatedRTT; //추정 RTT
    private TimeSpan _sampleRTT; //방금 측정된 RTT
    private float _alpha; //EWMA의 alpha값

    //이건 데디서버에 연결되고 나서 실행되야 하므로, DediServer OnConnected에서 호출해야함
    public void Init()
    {
        _dediserverClientTimeDelta = TimeSpan.Zero;
        _rttStartTime = DateTime.MinValue;
        _rttEndTime = DateTime.MinValue;
        _estimatedRTT = TimeSpan.Zero;
        _sampleRTT = TimeSpan.Zero;
        _alpha = 0.125f;
        
        //0.5초마다 데디서버의 타임스탬프를 요청
        Managers.Job.Push(RequestDediTimeStamp);
    }
    
    /// <summary>
    /// 데디서버의 타임스탬프를 요청함(데디 서버의 시간 추론을 위한 정보 획득을 위하여)
    /// </summary>
    public void RequestDediTimeStamp()
    {
        //RTT 측정 시작
        _rttStartTime = DateTime.UtcNow;
        
        //데디서버에게 타임스탬프 요청 패킷을 보냄
        CDS_RequestTimestamp requestTimestampPacket = new CDS_RequestTimestamp();
        Managers.Network._dedicatedServerSession.Send(requestTimestampPacket);
        
        //0.5초마다 데디서버의 타임스탬프를 요청
        Managers.Job.Push(RequestDediTimeStamp,500);
    }

    //데디서버의 타임스탬프를 받으면 rtt/2를 RequestDediTimeStamp()호출시의 타임스탬프에 더한 값 Tc를 구하고,
    //데디서버가 응답한 타임스탬픅 Ts일때, Ts-Tc = d를 구한다.
    //이 d값을 이용하여 데디서버의 시간을 추론할 수 있다.(클라 타임스탬프에 d 더하면 대충 데디서버 시간값이라고 추론 가능)
    //이때 rtt를 측정하는 방식은 Exponential Weighted Moving Average (EWMA)를 사용한다. (넷응설 챕3 70p참고)
    //이러면 과거 rtt들과 현rtt가 적절히 섞이면서 d값이 계속 적절히 조정됨.
    public void OnRecvDediServerTimeStamp(DSC_ResponseTimestamp packet)
    {
        //RTT 측정 종료
        _rttEndTime = DateTime.UtcNow;
        _sampleRTT = _rttEndTime - _rttStartTime;
        
        //EWMA를 이용하여 추정 RTT를 구함
        if (_estimatedRTT == TimeSpan.Zero)
        {
            //첫 측정이면 _estimatedRTT = _sampleRTT로 초기화
            _estimatedRTT = _sampleRTT;
        }
        else
        {
            _estimatedRTT = (1 - _alpha) * _estimatedRTT + _alpha * _sampleRTT;
        }
        
        //_estimatedRTT/2를 RequestDediTimeStamp()호출시의 타임스탬프에 더한 값 Tc를 구함
        DateTime Tc = _rttStartTime + _estimatedRTT/2;
        
        //데디서버가 응답한 타임스탬픅 Ts일때, Ts-Tc = d를 구함
        TimeSpan d = packet.Timestamp.ToDateTime() - Tc;
        
        //이 d값을 이용하여 데디서버의 시간을 추론할 수 있다.(클라 타임스탬프에 d 더하면 대충 데디서버 시간값이라고 추론 가능)
        _dediserverClientTimeDelta = d;
    }
    
    /// <summary>
    /// 추론된 현재 데디서버 시간을 반환
    /// </summary>
    /// <returns></returns>
    public DateTime GetDediServerTime()
    {
        return DateTime.UtcNow + _dediserverClientTimeDelta;
    }
    
    /// <summary>
    /// estimatedRTT의 반을 초단위로 바꿔서 리턴 (소수점도 포함)
    /// </summary>
    /// <returns>데디서버까지의 예상 레이턴시(초 단위)</returns>
    public float GetEstimatedLatency()
    {
        return (float)_estimatedRTT.TotalSeconds / 2;
    }
}