namespace WebServer
{
    
    public class GetPlayerSettingResponseDTO : BaseResponseDTO
    {
        public string PlayerSettingString;
    }
    public class GetPlayerSettingP : BaseWebProtocol
    {
        private string _userId;
        public void Init(string userId)
        {
            _userId = $"{userId}";
        }
        public override void SetUrlHttpMethod()
        {
            url = "/playerSetting"+$"/{_userId}";
            httpMethod = Define.HttpMethod.Get;
        }

        public override string MakeRequestBody()
        {
            return null;
        }
    }
}