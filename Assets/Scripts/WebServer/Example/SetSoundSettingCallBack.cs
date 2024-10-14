using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using WebServer;

public class SetSoundSettingCallBack : MonoBehaviour
{
    public void OnClickSetButton()
    {
        SetSoundSettingRequestDTO requestDto = new SetSoundSettingRequestDTO();
        requestDto.UserId = "testUserId0";
        requestDto.Master = 45.0f;
        requestDto.Bgm = 33.5f;
        requestDto.Effects = 22.2f;
        
        WebManager.Instance.SetSoundSetting(requestDto,ProceeResponse);
    }
    
    private void ProceeResponse(UnityWebRequest unityWebRequest)
    {
        //서버로부터 받은 json을 SetSoundSettingResponseDTO로 변환
        SetSoundSettingResponseDTO responseDto = LitJson.JsonMapper.ToObject<SetSoundSettingResponseDTO>(unityWebRequest.downloadHandler.text);
        
        //TODO : 서버로부터 받은 데이터를 처리
        //예시
        if (responseDto.code == 1) //서버에서의 처리가 성공일때 code=1을 담아서 서버에서 보내기로 약속함
        {
            Debug.Log($"서버에서 온 code: {responseDto.code}, 서버에서 온 message: {responseDto.message}");
        }
        else
        {
            Debug.Log($"Server Error : {responseDto.message}");
        }
    }
}
