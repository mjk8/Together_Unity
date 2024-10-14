using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PlayerLoop;

public class BaseResponseDTO
{
    public int code; //1: 성공 , 그 외: 실패
    public string message; //실패시 실패 이유
}
public abstract class BaseWebProtocol : MonoBehaviour
{
    protected string url;
    protected Define.HttpMethod httpMethod { get; set; }

    /// <summary>
    /// url과 httpMethod를 반드시 설정
    /// </summary>
    public abstract void SetUrlHttpMethod();

    /// <summary>
    /// http의 바디에 들어갈 내용을 json으로 만들어서 반환
    /// </summary>
    /// <returns>json의 string형식(없으면 null 반환하기)</returns>
    public abstract string MakeRequestBody();
    
    public void SendRequest(Action<UnityWebRequest> responseCallback)
    {
        SetUrlHttpMethod();
        StartCoroutine(CoSendWebRequest(responseCallback));
    }
    
    /// <summary>
    /// 웹서버에 요청을 보내고 응답을 받는 코루틴
    /// </summary>
    /// <param name="responseCallback">받은 응답을 처리하는 콜백함수</param>
    /// <param name="httpBody">http의 바디에 들어갈 내용</param>
    /// <returns></returns>
    IEnumerator CoSendWebRequest(Action<UnityWebRequest> responseCallback)
    {
        string sendUrl = $"{WebManager.Instance._baseUrl}{url}";
        string httpBodyJson= MakeRequestBody();

        byte[] jsonBytes = null;
        if (httpBodyJson != null)
        {
            jsonBytes = Encoding.UTF8.GetBytes(httpBodyJson); // body에 넣기위해서 json을 byte로 변환
        }
        
        var uwr=new UnityWebRequest(sendUrl, httpMethod.ToString());
        uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        Debug.Log($"현재 send한 url은 {sendUrl}이고, httpMethod는 {httpMethod.ToString()}이며, httpBody는 {httpBodyJson}");
        yield return uwr.SendWebRequest();
        

        if(uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log($"{uwr.error}");
        }
        else
        {
            Debug.Log($"Recv {uwr.downloadHandler.text}");
            responseCallback.Invoke(uwr);
        }
        
        //다 끝났기 때문에 destroy
        Destroy(this);
    }
    
}
