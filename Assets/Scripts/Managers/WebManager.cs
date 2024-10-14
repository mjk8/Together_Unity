using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using WebServer;


public class WebManager : MonoBehaviour
{
    private static WebManager _instance; 
    public static WebManager Instance {get { Init(); return _instance; } } 
    public static void Init()
    {
        if (_instance == null)
        {
            GameObject go = GameObject.Find("@WebManager");
            if (go == null)
            {
                go = new GameObject { name = "@WebManager" };
                go.AddComponent<WebManager>();
            }
            DontDestroyOnLoad(go);
            _instance = go.GetComponent<WebManager>();
        }
        
        UnityWebRequest.ClearCookieCache();
    }
    
    //웹서버 주소
    public readonly string _baseUrl = "http://igh01ip.iptime.org:5265";

    #region 클라에서는 아래 함수를 사용하면 됨 (responseCallback은 서버에서 받은 응답을 처리하는 콜백함수)

    //사운드 설정을 서버에게 받아옴
    public void GetSoundSetting(string userId,Action<UnityWebRequest> responseCallback)
    {
        GetSoundSettingP getSoundSettingP = gameObject.AddComponent<GetSoundSettingP>();
        getSoundSettingP.Init(userId);
        getSoundSettingP.SendRequest(responseCallback);
    }
    
    //사운드 설정을 서버로 보냄
    public void SetSoundSetting(SetSoundSettingRequestDTO requestDto, Action<UnityWebRequest> responseCallback)
    {
        SetSoundSettingP setSoundSettingP = gameObject.AddComponent<SetSoundSettingP>();
        setSoundSettingP.Init(requestDto);
        setSoundSettingP.SendRequest(responseCallback);
    }
    
    //키 설정을 서버에게 받아옴
    public void GetKeySetting(string userId,Action<UnityWebRequest> responseCallback)
    {
        GetKeySettingP getKeySettingP = gameObject.AddComponent<GetKeySettingP>();
        getKeySettingP.Init(userId);
        getKeySettingP.SendRequest(responseCallback);
    }
    
    //키 설정을 서버로 보냄
    public void SetKeySetting(SetKeySettingRequestDTO requestDto, Action<UnityWebRequest> responseCallback)
    {
        SetKeySettingP setKeySettingP = gameObject.AddComponent<SetKeySettingP>();
        setKeySettingP.Init(requestDto);
        setKeySettingP.SendRequest(responseCallback);
    }
    
    //플레이어 설정 정보를 서버에게 받아옴
    public void GetPlayerSetting(string userId,Action<UnityWebRequest> responseCallback)
    {
        GetPlayerSettingP getPlayerSettingP = gameObject.AddComponent<GetPlayerSettingP>();
        getPlayerSettingP.Init(userId);
        getPlayerSettingP.SendRequest(responseCallback);
    }
    
    //플레이어 설정 정보를 서버로 보냄
    public void SetPlayerSetting(SetPlayerSettingRequestDTO requestDto, Action<UnityWebRequest> responseCallback)
    {
        SetPlayerSettingP setPlayerSettingP = gameObject.AddComponent<SetPlayerSettingP>();
        setPlayerSettingP.Init(requestDto);
        setPlayerSettingP.SendRequest(responseCallback);
    }
    
    #endregion

    
    
    
}
