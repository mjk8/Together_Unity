using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx
{
    public Define.Scene SceneType = Define.Scene.Lobby;
    
	public void LoadScene(Define.Scene type)
    {
        Managers.Clear();
        SceneType = type;
        SceneManager.LoadScene(GetSceneName(type));
    }

    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    public void Clear()
    {
        // Implement Scene clear if necessary
    }
    
    /// <summary>
    /// InGame씬에서 daySeconds만큼 낮을 유지하고, changeSeconds만큼 걸쳐서 일몰로 변화
    /// </summary>
    /// <param name="daySeconds"></param>
    /// <param name="changeSeconds"></param>
    public void SimulateDayToSunset(float daySeconds, float changeSeconds)
    {
        //현재 씬이 InGame인지 확인
        if (SceneType != Define.Scene.InGame)
        {
            Debug.LogError("InGame씬이 아닙니다.");
            return;
        }
        //@Scene게임오브젝트에 있는 InGameDayCycleEffect컴포넌트를 찾아서 사용
        InGameDayCycleEffect dayCycleEffect = GameObject.Find("@Scene").GetComponent<InGameDayCycleEffect>();
        dayCycleEffect.SimulateDayToSunset(daySeconds, changeSeconds);
    }
    
    /// <summary>
    /// InGame씬에서 daySeconds만큼 일몰을 유지하고, changeSeconds만큼 걸쳐서 밤으로 변화
    /// </summary>
    /// <param name="daySeconds"></param>
    /// <param name="changeSeconds"></param>
    public void SimulateSunsetToNight(float daySeconds, float changeSeconds)
    {
        //현재 씬이 InGame인지 확인
        if (SceneType != Define.Scene.InGame)
        {
            Debug.LogError("InGame씬이 아닙니다.");
            return;
        }
        
        //@Scene게임오브젝트에 있는 InGameDayCycleEffect컴포넌트를 찾아서 사용
        InGameDayCycleEffect dayCycleEffect = GameObject.Find("@Scene").GetComponent<InGameDayCycleEffect>();
        dayCycleEffect.SimulateSunsetToNight(daySeconds, changeSeconds);
    }
    
    /// <summary>
    /// InGame씬에서 daySeconds만큼 밤을 유지하고, changeSeconds만큼 걸쳐서 새벽으로 변화
    /// </summary>
    /// <param name="daySeconds"></param>
    /// <param name="changeSeconds"></param>
    public void SimulateNightToSunrise(float daySeconds, float changeSeconds)
    {
        //현재 씬이 InGame인지 확인
        if (SceneType != Define.Scene.InGame)
        {
            Debug.LogError("InGame씬이 아닙니다.");
            return;
        }
        
        //@Scene게임오브젝트에 있는 InGameDayCycleEffect컴포넌트를 찾아서 사용
        InGameDayCycleEffect dayCycleEffect = GameObject.Find("@Scene").GetComponent<InGameDayCycleEffect>();
        dayCycleEffect.SimulateNightToDay(daySeconds, changeSeconds);
    }

    public void EndGameAndReturnToLobby()
    {
        Managers.Scene.LoadScene(Define.Scene.Lobby);
        Managers.UI.LoadScenePanel(Define.SceneUIType.LobbyUI);
        Managers.Network._dedicatedServerSession.Disconnect();
    }
}
