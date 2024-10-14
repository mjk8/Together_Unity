using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum SaveFiles
    {
        Player,
        Display,
        Sound,
        Control,
        KeyBinding
    }

    public enum Scene
    {
        //Scene Types that can occur
        Unknown,
        Lobby,
        InGame,
        ServerTest
    }
    
    public enum SceneUIType
    {
        MainMenuUI,
        LobbyUI,
        RoomUI,
        InGameUI,
        PlayerDeadUI,
        ObserveUI,
        WinnerUI
    }

    public enum UIEvent
    {
        //UI Events that can occur
        Click,
        Drag
    }

    public enum Settings
    {
        Display,
        Sound,
        Control,
        KeyBinding
    }

    public enum Sound
    {
        Bgm,
        Effects,
        Heartbeat,
    }


    public enum DisplayQuality
    {
        Low,
        Medium,
        High,
    }

    public enum MainMenuButtons
    {
        StartGame,
        Shop,
        Settings,
        EndGame
    }

    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Delete,
        Patch,
    }

    public enum SupportedLanguages
    {
        Korean,
        English
    }
}
