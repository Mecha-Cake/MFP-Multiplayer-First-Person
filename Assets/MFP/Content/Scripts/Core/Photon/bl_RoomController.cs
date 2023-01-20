using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using Photon.Realtime;
using Photon.Pun;

public class bl_RoomController : bl_PhotonHelper {

    [Separator("Inputs")]
    public KeyCode PauseKey = KeyCode.Escape;
    public KeyCode PauseMenuKey = KeyCode.M;
    public KeyCode PlayerListKey = KeyCode.Tab;
    [Separator("Player List")]
    public float UpdateListEach = 5f;
    public GameObject PlayerInfoPrefab;
    [Separator("Ping Settings")]
    public float UpdatePingEach = 5f;
    /// <summary>
    /// Max Ping to show message alert.
    /// </summary>
    public int MaxPing = 500;

    public static bool Pause = false;
    protected bool Lock = true;
    protected bool m_ShowPlayerList = false;

    private List<GameObject> CachePlayerList = new List<GameObject>();

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        Pause = false;
        InvokeRepeating("UpdatePlayerList", 1, UpdateListEach);
        InvokeRepeating("UpdatePing", 1, UpdatePingEach);
        UpdatePing();
        UpdatePlayerList();
    }
    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        Inputs();
    }

    /// <summary>
    /// 
    /// </summary>
    void Inputs()
    {
        //Show / Hide Pause Menu.
        if (Input.GetKeyDown(PauseMenuKey))
        {
            PauseEvent();
        }
        //Lock / Unlock cursor
        if (Input.GetKeyDown(PauseKey))
        {
            Lock = false;
            bl_CoopUtils.LockCursor(Lock);  
        }
        if (Input.GetMouseButtonDown(0) && bl_GameController.isPlaying && !Pause && !Lock && !m_ShowPlayerList)
        {
            Lock = true;
            bl_CoopUtils.LockCursor(Lock);  
        }

        if(Input.GetMouseButtonDown(0) && !Pause && bl_GameController.isPlaying && !m_ShowPlayerList)
        {
            Lock = true;
            bl_CoopUtils.LockCursor(Lock);
        }
        //Show / Hide PlayerList
        if (Input.GetKeyDown(PlayerListKey) && bl_GameController.isPlaying && !m_ShowPlayerList)
        {
            m_ShowPlayerList = true;
            bl_CoopUtils.LockCursor(false);
            bl_UIManager.Instance.m_ListManager.ShowPlayerList(true); 
        }
        if (Input.GetKeyUp(PlayerListKey) && bl_GameController.isPlaying && m_ShowPlayerList)
        {
            m_ShowPlayerList = false;
            bl_CoopUtils.LockCursor(true);
            bl_UIManager.Instance.m_ListManager.ShowPlayerList(false); 
        }
              
    }

    /// <summary>
    /// Show / Hide Pause Menu
    /// </summary>
    public void PauseEvent()
    {
        Pause = !Pause;
        bl_CoopUtils.LockCursor(!Pause);
        bl_UIManager.Instance.PauseMenuUI.SetActive(Pause);
    }
    /// <summary>
    /// 
    /// </summary>
    public void UpdatePlayerList()
    {
        if (!isConnected)
            return;

        //Removed old list
        if (CachePlayerList.Count > 0)
        {
            foreach (GameObject g in CachePlayerList)
            {
                Destroy(g);
            }
        }

        //Update List
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            GameObject r = Instantiate(PlayerInfoPrefab) as GameObject;
            CachePlayerList.Add(r);
            int pi;
            if (players[i].CustomProperties.ContainsKey("Ping"))
            {
                pi = (int)players[i].CustomProperties["Ping"];
            }
            else
            {
                pi = 0;
            }
            //Send information to each UI
            r.GetComponent<bl_PlayerList>().GetInfo(players[i].NickName, pi.ToString(),players[i]);
            r.transform.SetParent(bl_UIManager.Instance.PlayerListPanel, false);
        }

    }

    /// <summary>
    /// Update the player ping
    /// verify the state of ping
    /// </summary>
    void UpdatePing()
    {
        //Get ping from cloud
        int Ping = PhotonNetwork.GetPing();

        //Send ping to other player can access or see it.
        Hashtable PlayerPing = new Hashtable();
        PlayerPing.Add("Ping", Ping);
        PhotonNetwork.LocalPlayer.SetCustomProperties(PlayerPing);


        if (bl_UIManager.Instance.PingMsnUI != null)
        {
            //If ping mayor that max ping allowed
            if (Ping > MaxPing)
            {
                //Show alert mesagge
                //NOTE: you can write here your code for kick player if you want
                //when he has too ping.
                if (!bl_UIManager.Instance.PingMsnUI.activeSelf)
                {
                    bl_UIManager.Instance.PingMsnUI.SetActive(true);
                }
            }
            else
            {
                if (bl_UIManager.Instance.PingMsnUI.activeSelf)
                {
                    bl_UIManager.Instance.PingMsnUI.SetActive(false);
                }
            }
        }
    }

    public void LeaveRoom() { PhotonNetwork.LeaveRoom(); }

    private static bl_RoomController _instance;
    public static bl_RoomController Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_RoomController>(); }
            return _instance;
        }
    }
}