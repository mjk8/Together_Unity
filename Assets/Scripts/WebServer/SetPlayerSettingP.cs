namespace WebServer
{
    //서버로 보내기 위한 데이터구조(MakeRequestBody에서 json으로 변환됨)
    public class SetPlayerSettingRequestDTO
    {
        public string UserId;
        public string PlayerSettingString;
    }
    
    //서버로부터 받을 데이터구조
    public class SetPlayerSettingResponseDTO : BaseResponseDTO
    {
    }


    public class SetPlayerSettingP : BaseWebProtocol
    {
        SetPlayerSettingRequestDTO _setPlayerSettingRequestDto = new SetPlayerSettingRequestDTO();
        
        public void Init(SetPlayerSettingRequestDTO requestDto)
        {
            _setPlayerSettingRequestDto = requestDto;
        }
        
        public override void SetUrlHttpMethod()
        {
            url = "/playerSetting";
            httpMethod = Define.HttpMethod.Put;
        }

        public override string MakeRequestBody()
        {
            return LitJson.JsonMapper.ToJson(_setPlayerSettingRequestDto);
        }
    }
}