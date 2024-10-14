using UnityEngine;

namespace WebServer
{
    //서버로 보내기 위한 데이터구조(MakeRequestBody에서 json으로 변환됨)
    public class SetSoundSettingRequestDTO
    {
        public string UserId;
        public float Master;
        public float Bgm;
        public float Effects;
    }
    
    //서버로부터 받을 데이터구조
    public class SetSoundSettingResponseDTO : BaseResponseDTO
    {
    }
    
    public class SetSoundSettingP: BaseWebProtocol
    {
        SetSoundSettingRequestDTO _setSoundSettingRequestDto = new SetSoundSettingRequestDTO();

        public void Init(SetSoundSettingRequestDTO requestDto)
        {
            _setSoundSettingRequestDto = requestDto;
        }

        public override void SetUrlHttpMethod()
        {
            url = "/soundSetting";
            httpMethod = Define.HttpMethod.Put;
        }

        public override string MakeRequestBody()
        {
            return LitJson.JsonMapper.ToJson(_setSoundSettingRequestDto);
        }
    }
}