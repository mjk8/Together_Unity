using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance; 
    static Managers Instance {get { Init(); return _instance; } } 
    
    ResourceManager _resource = new ResourceManager();
    UIManager _ui = new UIManager();
    SceneManagerEx _scene = new SceneManagerEx();
    SoundManager _sound = new SoundManager();
    PoolManager _pool = new PoolManager();
    DataManager _data = new DataManager();
    NetworkManager _network = new NetworkManager();
    PlayerManager _player = new PlayerManager();
    WebManager _web = new WebManager();
    InputManager _input = new InputManager();
    RoomManager _room = new RoomManager();
    DedicatedManager _dedicated = new DedicatedManager();
    private ObjectManager _object = new ObjectManager();
    JobTimerManager _job = new JobTimerManager();
    TimeManager _time = new TimeManager();
    GameManager _game = new GameManager();
    ItemManager _item = new ItemManager();
    KillerManager _killer = new KillerManager();
    SteamManager _steam = new SteamManager();
    InventoryManager _inventory = new InventoryManager();
    EffectsManager _effects = new EffectsManager();
    
    
    public static  ResourceManager Resource { get { return Instance._resource;} }
    public static UIManager UI { get { return Instance._ui; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static DataManager Data { get { return Instance._data; } }
    public static NetworkManager Network { get { return Instance._network; } }
    public static PlayerManager Player { get { return Instance._player; } }
    public static InputManager Input { get { return Instance._input; } }
    public static RoomManager Room { get { return Instance._room; } }
    public static DedicatedManager Dedicated { get { return Instance._dedicated; } }
    public static ObjectManager Object { get { return Instance._object; } }
    public static JobTimerManager Logic { get { return Instance._job; } }
    public static TimeManager Time { get { return Instance._time; } }
    public static JobTimerManager Job { get { return Instance._job; } }
    public static GameManager Game { get { return Instance._game; } }
    public static ItemManager Item { get { return Instance._item; } }
    public static KillerManager Killer { get { return Instance._killer; } }
    public static SteamManager Steam { get { return Instance._steam; } }
    
    public static InventoryManager Inventory { get { return Instance._inventory; } }
    public static EffectsManager Effects { get { return Instance._effects; }
        set { Instance._effects = value; }
    }

    void Start()
    {
        Init();
        WebManager.Init();
    }

    
    void Update()
    {
        _network.Update(); //받은 패킷 처리
        _job.Flush(); //일감 처리(움직임 패킷 여기서 처리)
    }

    private void FixedUpdate()
    {
       // _logic.FixedUpdate(); //게임 로직 업데이트(게임로직 주기마다 실행되어야하는)
    }

    static void Init()
    {
        if (_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            _instance = go.GetComponent<Managers>();
            _instance._sound.Init();
            _instance._data.Init();
            _instance._pool.Init();
            _instance._steam.Init();//네트워크 매니저보다 먼저 실행되어야 함.
            _instance._network.Init();
            _instance._player.Init();
            _instance._ui.Init();
            _instance._input.Init();
            _instance._object.Init();
            _instance._game.Init();
            _instance._item.Init();
            _instance._killer.Init();
            
        }
    }
    
    
    public static void Clear()
    {
        //TODO: 씬 넘어갈때 클리어해야할 것들 여기에 다 추가 해야함
        Sound.Clear();
        Scene.Clear();
        UI.Clear();
        Pool.Clear();
        Inventory.Clear();
        Item.Clear();
        Game.Clear();
    }

    private void OnApplicationQuit()
    {
        _network.OnQuitUnity();
    }
}
