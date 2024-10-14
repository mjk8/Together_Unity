using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.Serialization;

public class SteamManager : MonoBehaviour
{
    public static GameObject root;
    public bool _isSteamInitialized = false;
    public ulong _steamId;
    public string _steamName;

    private Callback<GameRichPresenceJoinRequested_t> lobbyInviteCallback;

    //Managers Init과 함께 불리는 Init
    public void Init()
    {
        root = GameObject.Find("@Steam");
        if (root == null)
        {
            root = new GameObject { name = "@Steam" };
            Object.DontDestroyOnLoad(root);
        }

        if (SteamAPI.Init())
        {
            _isSteamInitialized = true;
            Debug.Log("Steamworks initialized successfully.");

            //내 스팀 이름 가져와서 저장
            SetName();

            //steamId 가져와서 저장
            SetSteamId();

            // 게임 초대 수락시 호출되는 콜백 함수
            lobbyInviteCallback = Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRoomJoinRequested);
        }
        else
        {
            Debug.LogError("Failed to initialize Steamworks.");
            _isSteamInitialized = false;
        }
    }

    public void SetRichPresenceForInvite(int roomId, string password)
    {
        // 연결 문자열을 설정합니다. 예를 들어, 로비 ID를 포함한 연결 문자열을 설정합니다.
        string connectString = "roomId=" + roomId + "/password=" + password;

        // 풍부한 존재감 데이터를 설정합니다.
        SteamFriends.SetRichPresence("connect", connectString);

        Debug.Log("Rich presence set with connect string: " + connectString);
    }

    public void GetFriendsList()
    {
        // 친구 목록을 저장할 리스트
        List<CSteamID> friends = new List<CSteamID>();

        // 친구 수를 가져옵니다
        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

        // 모든 친구를 리스트에 추가합니다
        for (int i = 0; i < friendCount; i++)
        {
            CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
            friends.Add(friendSteamID);
        }

        // 친구 목록을 출력하거나 필요한 작업을 수행합니다
        foreach (var friend in friends)
        {
            string friendName = SteamFriends.GetFriendPersonaName(friend);
            Debug.Log("Friend: " + friendName);
        }
    }

    public void InviteFriendToGame()
    {
        // 친구 목록을 저장할 리스트
        List<CSteamID> friends = new List<CSteamID>();

        // 친구 수를 가져옵니다
        int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);

        // 모든 친구를 리스트에 추가합니다
        for (int i = 0; i < friendCount; i++)
        {
            CSteamID friendSteamID = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
            friends.Add(friendSteamID);
        }


        // 친구 목록을 출력하거나 필요한 작업을 수행합니다
        foreach (var friend in friends)
        {
            string friendName = SteamFriends.GetFriendPersonaName(friend);
            if (friendName == "텍사스준구앞치마도둑")
            {
                SteamFriends.InviteUserToGame(friend, "");
                Debug.Log("Invited " + friendName + " to the game.");
                return;
            }
        }
    }

    private void OnGameRoomJoinRequested(GameRichPresenceJoinRequested_t joinRequested)
    {
        //joinRequested.m_steamIDFriend를 가지고 스팀이름을 얻어오기
        string friendName = SteamFriends.GetFriendPersonaName(joinRequested.m_steamIDFriend);
        Debug.Log("Game lobby join requested by: " + friendName);

        //joinRequested.m_rgchConnect내용 출력
        Debug.Log("Connect string: " + joinRequested.m_rgchConnect);
        
        int roomId=-1;
        string password="";

        // '/'로 분리하여 각각의 파트를 얻음
        string[] parts = joinRequested.m_rgchConnect.Split('/');
        
        // 각 파트를 '='로 분리하여 키와 값을 추출
        roomId = int.Parse(parts[0].Split('=')[1]);
        password = parts[1].Split('=')[1];
        
        Debug.Log("Room ID: " + roomId);
        Debug.Log("Password: " + password);

        //TODO: 룸id를 가지고 입장 
        Managers.Room.RequestEnterRoom(roomId, password, Managers.Player._myRoomPlayer.Name);
    }

    /// <summary>
    /// 내 스팀 이름을 가져와서 저장 및 사용
    /// </summary>
    private void SetName()
    {
        string steamUserName = SteamFriends.GetPersonaName();
        Debug.Log("My Steam Name: " + steamUserName);
    }

    /// <summary>
    /// steamId를 가져와서 저장
    /// </summary>
    private void SetSteamId()
    {
        _steamId = SteamUser.GetSteamID().m_SteamID;
        Debug.Log($"steamid: {_steamId}");
    }
}