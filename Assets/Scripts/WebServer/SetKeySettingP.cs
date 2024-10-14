namespace WebServer
{
    //서버로 보내기 위한 데이터구조(MakeRequestBody에서 json으로 변환됨)
    public class SetKeySettingRequestDTO
    {
        public string UserId;
        public string KeySettingString;
    }

    //서버로부터 받을 데이터구조
    public class SetKeySettingResponseDTO : BaseResponseDTO
    {
    }

    public class SetKeySettingP : BaseWebProtocol
    {
        SetKeySettingRequestDTO _setKeySettingRequestDto = new SetKeySettingRequestDTO();
        
        public void Init(SetKeySettingRequestDTO requestDto)
        {
            _setKeySettingRequestDto = requestDto;
        }
        public override void SetUrlHttpMethod()
        {
            url = "/keySetting";
            httpMethod = Define.HttpMethod.Put;
        }

        public override string MakeRequestBody()
        {
            return LitJson.JsonMapper.ToJson(_setKeySettingRequestDto);
        }
    }
}