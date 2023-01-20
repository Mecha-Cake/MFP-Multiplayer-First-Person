using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using Photon.Pun;
using Photon.Realtime;

public class bl_PhotonRaiseEvent : MonoBehaviourPunCallbacks
{

    static readonly RaiseEventOptions EventsAll = new RaiseEventOptions();
    protected int LoadIn = 5;
    protected string NextLevel = "";
    protected bool IsLobby = true;
    protected bool Registered = false;

    public AudioClip CountDownSound = null;

    public const string PhotonEventName = "PhotonEvent";
    private AudioSource Source;
    

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        this.gameObject.name = PhotonEventName;
        if (!Registered)
        {
            EventsAll.Receivers = ReceiverGroup.All;
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }
        //make this relevant object in the scenes
        DontDestroyOnLoad(this.gameObject);
        Source = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnEvent(EventData data)
    {
        if (data.CustomData == null) return;

        switch (data.Code)
        {
            case EventID.LoadSyncLevel:
                Hashtable hash = new Hashtable();
                hash = (Hashtable)data.CustomData;
                string s = (string)hash["Level"];
                NextLevel = s;
                InvokeRepeating("InvokeLoad", 1, 1);
                break;

            case EventID.PlayerJoinPre:
                //
                break;
        }
    }

    /// <summary>
    /// CountDown for load level
    /// </summary>
    void InvokeLoad()
    {
            LoadIn--;
            if (IsLobby)
            {
                if (LoadIn >= 0)
                {
                    bl_PreScene.SendChat("Server", "Starting in: " + LoadIn);
                    if (CountDownSound != null) { Source.clip = CountDownSound; Source.Play(); }
                }
                if (LoadIn == 1)
                {
                    bl_CoopUtils.GetLobbyUI.Fade();
                }
            }
            if (LoadIn == 0)
            {
                LoadIn = 5;
                CancelInvoke("InvokeLoad");
                if (bl_PreScene.isReady)
                {
                    IsLobby = false;
                    PhotonNetwork.IsMessageQueueRunning = false;
                    bl_CoopUtils.LoadLevel(NextLevel);
                }
                else
                {
                    bl_CoopUtils.GetPreScene.LevelLoaded(0);
                }
            }
    }
    /// <summary>
    /// 
    /// </summary>
    public override void OnLeftRoom()
    {
        IsLobby = true;
    }

    private static bl_PhotonRaiseEvent _instance;
    public static bl_PhotonRaiseEvent Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_PhotonRaiseEvent>(); }
            return _instance;
        }
    }
}