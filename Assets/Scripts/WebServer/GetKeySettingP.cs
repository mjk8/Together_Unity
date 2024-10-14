namespace WebServer
{
    public class GetKeySettingResponseDTO : BaseResponseDTO
    {
        public string KeySettingString;
    }
    public class GetKeySettingP : BaseWebProtocol
    {
        private string _userId;
        public void Init(string userId)
        {
            _userId = $"{userId}";
        }
        public override void SetUrlHttpMethod()
        {
            url = "/keysetting"+$"/{_userId}";
            httpMethod = Define.HttpMethod.Get;
        }

        public override string MakeRequestBody()
        {
            return null;
        }
    }
}