using UnityEngine;

namespace WebServer
{
    #region 서버로부터 받을 데이터구조
    public class GetSoundSettingResponseDTO : BaseResponseDTO
    {
        public SoundSetting soundSetting;
    }
    
    public class SoundSetting
    {
        public float Master;
        public float Bgm;
        public float Effects;
    }
    # endregion
    
    public class GetSoundSettingP : BaseWebProtocol
    {
        private string _userId;
        public void Init(string userId)
        {
            _userId = $"{userId}";
        }
        
        public override void SetUrlHttpMethod()
        {
            url = "/soundsetting"+$"/{_userId}";
            httpMethod = Define.HttpMethod.Get;
        }

        public override string MakeRequestBody()
        {
            return null;
        }
    }
}