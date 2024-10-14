using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using WebServer;

public class GetSoundSettingCallBack : MonoBehaviour
{
    //유니티의 OnClick 이벤트에 연결하기 위한 함수예시를 알려줘
    public void OnClickGetButton()
    {
        WebManager.Instance.GetSoundSetting("1", ProceeResponse);
    }

    private void ProceeResponse(UnityWebRequest unityWebRequest)
    {
        //서버로부터 받은 json을 SetSoundSettingResponseDTO로 변환
        GetSoundSettingResponseDTO responseDto = LitJson.JsonMapper.ToObject<GetSoundSettingResponseDTO>(unityWebRequest.downloadHandler.text);

        //TODO : 서버로부터 받은 데이터를 처리
        //예시
        if (responseDto.code == 1) //서버에서의 처리가 성공일때 code=1을 담아서 서버에서 보내기로 약속함
        {
            Debug.Log($"서버에서 온 code: {responseDto.code}, 서버에서 온 message: {responseDto.message}");
            Debug.Log($"해당 유저의 마스터 볼륨: {responseDto.soundSetting.Master}, 해당 유저의 배경음 볼륨: {responseDto.soundSetting.Bgm}, 해당 유저의 효과음 볼륨: {responseDto.soundSetting.Effects}");
        }
        else
        {
            Debug.Log($"Server Error : {responseDto.message}");
        }
    }
}