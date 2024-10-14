using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using File = System.IO.File;

[System.Serializable]
public struct ResolutionStruct
{
    public int width;
    public int height;

    public string ToDisplayString()
    {
        return this.width +"x"+this.height;
    }
}

[System.Serializable]
public class PlayerData
{
    public Locale currentLocale;
    public float MouseSensitivity;
    public bool isFullScreen;
    public Define.DisplayQuality DisplayQuality;
    public ResolutionStruct MyResolution;

    public PlayerData()
    {
        MyResolution.width = Screen.currentResolution.width;
        MyResolution.height = Screen.currentResolution.height;
        currentLocale = LocalizationSettings.AvailableLocales.Locales[0];
        MouseSensitivity = 100f;
        isFullScreen = true;
        DisplayQuality = Define.DisplayQuality.High;
    }
    
    public PlayerData(PlayerData copy)
    {
        this.currentLocale = copy.currentLocale;
        this.MouseSensitivity = copy.MouseSensitivity;
        this.isFullScreen = copy.isFullScreen;
        this.DisplayQuality = copy.DisplayQuality;
        this.MyResolution = new ResolutionStruct { width = copy.MyResolution.width, height = copy.MyResolution.height };
    }
}

public class DataManager
{
    Dictionary<Define.SaveFiles, string> fileNames;
    public static PlayerData _playerData;

    public PlayerData Player
    {
        get { return _playerData; }
        set { _playerData = value; }
    }

    public void Init()
    {
        fileNames = new Dictionary<Define.SaveFiles, string>();

        //Define file names
        fileNames[Define.SaveFiles.Player] = "PlayerData.json";
        fileNames[Define.SaveFiles.Display] = "DisplaySettings.json";
        fileNames[Define.SaveFiles.Sound] = "SoundSettings.json";
        fileNames[Define.SaveFiles.Control] = "ControlSettings.json";
        fileNames[Define.SaveFiles.KeyBinding] = "OverrideBindings.json";
        
        //json load로 기존 설정 불러오기
        _playerData = new PlayerData();
        _playerData = Managers.Data.LoadFromJson<PlayerData>(Define.SaveFiles.Player, _playerData);
        
        //기존 설정 적용하기
        Screen.fullScreen = _playerData.isFullScreen;
        Screen.SetResolution(_playerData.MyResolution.width,_playerData.MyResolution.height,_playerData.isFullScreen);
        DisplaySettings.SetQualityLevel(_playerData.DisplayQuality);
    }

    public void SaveToJson(Define.SaveFiles fileType, string data)
    {
        //TODO: change from local to DB
        File.WriteAllText(GetFilePath(fileType), data);
    }

    public void SaveToJson<T>(Define.SaveFiles fileType, T data)
    {
        //TODO: change from local to DB
        File.WriteAllText(GetFilePath(fileType),JsonUtility.ToJson(data));
    }

    public string LoadFromJson(Define.SaveFiles fileType)
    {
        //TODO: change from local to DB
        if (File.Exists(GetFilePath(fileType)))
        {
            //TODO: change from local to DB
            return File.ReadAllText(GetFilePath(fileType));
        }
        else
        {
            return null;
        }
    }
    
    public T LoadFromJson<T>(Define.SaveFiles fileType, T classToLoad)
    {
        if (File.Exists(GetFilePath(fileType)))
        {
            //TODO: change from local to DB
            string str = File.ReadAllText(GetFilePath(fileType));
            return JsonUtility.FromJson<T>(str);
        }
        else
        {
            return classToLoad;
        }
    }

    string GetFilePath(Define.SaveFiles fileType)
    {
        return Application.persistentDataPath +"\\"+ fileNames[fileType];
    }
    
    public void SavePlayerData()
    {
        //설정 저장버튼 누른거니까 서버한테 패킷 보내기
        CS_SetSetting packet = new CS_SetSetting();
        packet.SteamId = Managers.Steam._steamId;
        packet.InstanceId = 0; //임시값
        packet.MouseSensitivity = _playerData.MouseSensitivity;
        packet.IsFullScreen = _playerData.isFullScreen;
        packet.DisplayQuality = (int)_playerData.DisplayQuality;
        packet.Width = _playerData.MyResolution.width;
        packet.Height = _playerData.MyResolution.height;

        Managers.Network._roomSession.Send(packet);

        //로컬에 json으로 저장
        SaveToJson<PlayerData>(Define.SaveFiles.Player,_playerData);
    }

    //서버에 저장된 설정을 불러와서 적용
    public void ApplyServerSavedSetting(SC_GetSetting packet)
    {
        bool isSettingExist = packet.IsSettingExist;

        if (isSettingExist)
        {
            ulong steamId = packet.SteamId;
            int instanceId = packet.InstanceId; //내 생각엔 이걸 저장하는게 아닌듯. 
            float mouseSensitivity = packet.MouseSensitivity;
            bool isFullScreen = packet.IsFullScreen;
            int displayQuality = packet.DisplayQuality;
            int width = packet.Width;
            int height = packet.Height;

            //instanceId를 _playerData의 currentLocale에 적용
            _playerData.MouseSensitivity = mouseSensitivity;
            _playerData.isFullScreen = isFullScreen;
            _playerData.DisplayQuality = (Define.DisplayQuality)displayQuality;
            _playerData.MyResolution.width = width;
            _playerData.MyResolution.height = height;

            SaveToJson<PlayerData>(Define.SaveFiles.Player, _playerData); //이건 json으로 저장하는거. 아직 설정 적용한건 아님.

            //설정 적용
            Screen.fullScreen = isFullScreen;
            Screen.SetResolution(width, height, isFullScreen);
            DisplaySettings.SetQualityLevel((Define.DisplayQuality)displayQuality);
        }
        else //세팅이 없다면 이미 있는 세팅 그냥 그대로 씀
        {
            return;
        }
    }
}