using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Random = UnityEngine.Random;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using Photon.Pun;
using Photon.Realtime;

public class bl_Lobby : MonoBehaviourPunCallbacks
{

    [Separator("Photon Settings")]
    public string GameVersion = "1.0";
    public string PlayerNamePrefix = "Guest";
    public string ServerHost = "usw";
    public PunLogLevel LogLevelType = PunLogLevel.Full;
    [Separator("Global Settings")]
    public int[] RoomTime;
    [HideInInspector]public int r_Time = 0;

    public int[] MaxPlayers;
    [HideInInspector]public int m_MaxPlayer = 0;
    [Space(5)]
    public GameObject PhotonEvent = null;
    [Separator("Scene Manager")]
    public List<_Scenes> SceneManager = new List<_Scenes>();
    [HideInInspector] public int CurrentScene = 0;

    private bool GamePerRounds = false;
    private bool Joining = false;
    private bl_LobbyUI UI;
    private bool tryMatchMaking = false;
    public bl_RoomInfo pendingRoomInfo { get; set; }
    private bool quitting = false;

    /// <summary>
    /// 
    /// </summary>
    public void Awake()
    {
        bl_GameData.Instance.Init();
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;

        // if you wanted more debug out, turn this on:
        PhotonNetwork.LogLevel = LogLevelType;
        UI = FindObjectOfType<bl_LobbyUI>();
    }
    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (FindObjectOfType<bl_PhotonRaiseEvent>() == null)
        {
            Instantiate(PhotonEvent);
        }
        int random = Random.Range(0, 9998);
        string playerName = string.Format("{0}({1})", PlayerNamePrefix, random);
        GetComponent<bl_LobbyUI>().PlayerNameImput.text = playerName;
    }
    /// <summary>
    /// Switch connection type
    /// </summary>
    /// <param name="type"></param>
    public void Connect()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
        {
            Debug.LogWarning("You need your appID, read the documentation for more info!");
            return;
        }
        //Connect to select region
        PhotonNetwork.GameVersion = GameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void GamePerRound(bool b) { GamePerRounds = b; }
    /// <summary>
    /// Create a new room photon
    /// </summary>
    /// <param name="mInput"></param>
    /// <param name="MaxPlayers"></param>
    public void CreateRoom(InputField mInput)
    {
        //Avoid to request double connection.
        if (Joining)
        {
            Debug.Log("Already Request a connection, wait for server response.");
            return;
        }
        bool isPublic = string.IsNullOrEmpty(UI.PasswordInput.text);
       if (!String.IsNullOrEmpty(mInput.text))
        {
           //Create hastable to send room information from lobby.
            ExitGames.Client.Photon.Hashtable roomOption = new ExitGames.Client.Photon.Hashtable();
           //Add information in hashtable.
            roomOption[PropertiesKeys.TimeRoomKey] = RoomTime[r_Time];
            roomOption[PropertiesKeys.RoomRoundKey] = GamePerRounds ? "1" : "0";
            roomOption[PropertiesKeys.SceneNameKey] = SceneManager[CurrentScene].SceneName;
            roomOption[PropertiesKeys.RoomState] = "False";
            roomOption[PropertiesKeys.RoomPassword] = UI.PasswordInput.text;

            string[] properties = new string[5];          
            properties[0] = PropertiesKeys.RoomRoundKey;
            properties[1] = PropertiesKeys.TimeRoomKey;
            properties[2] = PropertiesKeys.SceneNameKey;
            properties[3] = PropertiesKeys.RoomState;
            properties[4] = PropertiesKeys.RoomPassword;

            int mp = MaxPlayers[m_MaxPlayer];
            PhotonNetwork.CreateRoom(mInput.text, new RoomOptions() { MaxPlayers = (byte)mp ,
                CleanupCacheOnLeave = true,
                CustomRoomProperties = roomOption,
                CustomRoomPropertiesForLobby = properties,
            }, null);
            Joining = true;
        }
        else
        {
            Debug.Log("Room Name can not be empty!");
        }
    }

    void CreateRoomAutomatically()
    {
        string rn = string.Format("Room {0}", Random.Range(0, 9999));

        //Create hastable to send room information from lobby.
        ExitGames.Client.Photon.Hashtable roomOption = new ExitGames.Client.Photon.Hashtable();
        //Add information in hashtable.
        roomOption[PropertiesKeys.TimeRoomKey] = RoomTime[r_Time];
        roomOption[PropertiesKeys.RoomRoundKey] = "0";
        roomOption[PropertiesKeys.SceneNameKey] = SceneManager[CurrentScene].SceneName;
        roomOption[PropertiesKeys.RoomState] = "False";
        roomOption[PropertiesKeys.RoomPassword] = "";

        string[] properties = new string[5];

        properties[0] = PropertiesKeys.RoomRoundKey;
        properties[1] = PropertiesKeys.TimeRoomKey;
        properties[2] = PropertiesKeys.SceneNameKey;
        properties[3] = PropertiesKeys.RoomState;
        properties[4] = PropertiesKeys.RoomPassword;

        PhotonNetwork.JoinOrCreateRoom(rn, new RoomOptions()
        {
            MaxPlayers = 8,
            CleanupCacheOnLeave = true,
            CustomRoomProperties = roomOption,
            CustomRoomPropertiesForLobby = properties
        }, null);
        Joining = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        if (PhotonNetwork.IsConnected)
        {
            quitting = true;
            PhotonNetwork.Disconnect();
        }
        else
        {
            Application.Quit();
        }
#endif
    }

    /// <summary>
    /// This new feature of photon to connect to server easy.
    /// For call this you need not state already connect.
    /// </summary>
    /// <param name="code"></param>
    bool WaitForConnectToNew = false;
    public void ChangeServer(int region)
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            WaitForConnectToNew = true;
            ServerHost = bl_CoopUtils.IntToRegionCode(region);
            Debug.LogWarning("Wait for the current server disconnect and reconnect to new.");
            return;
        }
        //If disconect, connect again to the select region
        PhotonNetwork.ConnectToRegion(bl_CoopUtils.IntToRegionCode(region));
        ServerHost = bl_CoopUtils.IntToRegionCode(region);
        WaitForConnectToNew = false;
        this.GetComponent<bl_LobbyUI>().ChangeWindow(6);
    }

    /// <summary>
    /// 
    /// </summary>
    public void QuickPlay()
    {
        if (!PhotonNetwork.IsConnected || Joining)
            return;

        tryMatchMaking = true;
        UI.ShowLookingUI(true);
        PhotonNetwork.JoinRandomRoom();
    }


    // We have two options here: we either joined(by title, list or random) or created a room.
    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        
        Hashtable e = new Hashtable();
        e.Add("PName", PhotonNetwork.LocalPlayer.NickName);
        bool master = (PhotonNetwork.LocalPlayer.IsMasterClient) ? true : false;
        e.Add("Ready", master);
        PhotonNetwork.LocalPlayer.SendReady(master);
        ExitGames.Client.Photon.SendOptions so = new SendOptions() { Reliability = true };
        PhotonNetwork.RaiseEvent(EventID.PlayerJoinPre, e, new RaiseEventOptions() { Receivers = ReceiverGroup.All }, so);
        UI.ShowLookingUI(false);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (tryMatchMaking)
        {
            Invoke("CreateRoomAutomatically", 2);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Joining = false;
        UI.ShowLookingUI(false);
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        this.GetComponent<bl_LobbyUI>().ChangeWindow(5);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if(quitting)
        {
            Application.Quit();
            return;
        }
        base.OnDisconnected(cause);
        Debug.Log("Disconnected from Photon.");
        //When have a pending new connection to a new server.
        if (WaitForConnectToNew)
        {
            //Connect auto again
            ChangeServer(bl_CoopUtils.RegionCodeToInt(ServerHost));
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to master");
        PhotonNetwork.JoinLobby();
    }

    public void OnFailedToConnectToPhoton(object parameters)
    {
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
    }

    public override void OnLeftRoom()
    {
        Joining = false;
    }

    public void WaitForRoomPassword(bl_RoomInfo room)
    {
        pendingRoomInfo = room;
        UI.RoomPasswordWindow.SetActive(true);
    }

    public static bl_Lobby _Lobby;
    public static bl_Lobby Instance
    {
        get
        {
            if(_Lobby == null) { _Lobby = FindObjectOfType<bl_Lobby>(); }
            return _Lobby;
        }
    }

    [System.Serializable]
    public class _Scenes
    {
        public string SceneName = "";
        public string MapName = "";
        public Sprite PreviewImage = null;
    }
}