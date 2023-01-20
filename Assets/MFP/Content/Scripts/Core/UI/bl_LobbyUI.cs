using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class bl_LobbyUI : MonoBehaviourPunCallbacks
{
    [Separator("Lobby Windows")]
    public GameObject PlayerNameWindow = null;
    public GameObject SearchWindow = null;
    public GameObject HostRoomWindow = null;
    public GameObject MenuWindow = null;
    public GameObject PreSceneWindow = null;
    public GameObject ConnectingWindow = null;
    public GameObject OptionsWindow = null;
    public GameObject SelectServerWindow = null;
    public GameObject QuickPlayUI = null;
    public GameObject RoomPasswordWindow;
    [SerializeField] private GameObject CharacterSelectionWindow = null;

    [Space(7)]
    public Transform RoomPanel = null;
    public GameObject RoomInfoPrefab;
    [Space(5)]
    [Separator("UI Reference")]
    public Text PhotonStatusText = null;
    public Text PhotonRegionText = null;
    public Image FadeImage = null;
    public InputField RoomNameInput = null;
    public InputField PasswordInput = null;
    public InputField PlayerNameImput = null;
    [Space(5)]   
    public Text TimeText;
    public Text MaxPlayersText;
    public Text RoomListText = null;
    public Text MapNameText = null;
    public Image MapPreviewImage = null;

    //Privates
    private List<GameObject> CacheRoomList = new List<GameObject>();
    private bool OptionMenuOpen = false;
    private bool isFadeEvent = false;
    private Dictionary<string, RoomInfo> cachedRoomList;

    void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();

        CheckPlayerName();

        if (Lobby.RoomTime[Lobby.r_Time] <= 0)
        {
            TimeText.text = "∞";
        }
        else
        {
            TimeText.text = (Lobby.RoomTime[Lobby.r_Time] / 60) + " <size=12>Min</size>";
        }
        MaxPlayersText.text = "Players: " + Lobby.MaxPlayers[Lobby.m_MaxPlayer];
        MapPreviewImage.sprite = Lobby.SceneManager[Lobby.CurrentScene].PreviewImage;
        MapNameText.text = Lobby.SceneManager[Lobby.CurrentScene].MapName;
        if (RoomNameInput != null) { RoomNameInput.text = "Room (" + Random.Range(0, 9999) + ")"; }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        bl_PhotonCallbacks.Instance.OnJoinLobby+=(OnJoinedLobby);
    }
    /// <summary>
    /// 
    /// </summary>
    public override void OnDisable()
    {
        base.OnDisable();
        bl_PhotonCallbacks.Instance.OnJoinLobby-=(OnJoinedLobby);
        StopAllCoroutines();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowLookingUI(bool show)
    {
        if (QuickPlayUI == null)
            return;

        QuickPlayUI.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void ChangeWindow(int id)
    {
        switch (id)
        {

            case 0 :
                PlayerNameWindow.SetActive(true);
                MenuWindow.SetActive(false);
                HostRoomWindow.SetActive(false);
                PreSceneWindow.SetActive(false);
                SearchWindow.SetActive(false);
                ConnectingWindow.SetActive(false);
                CharacterSelectionWindow.SetActive(false);
                break;
            case 1:
                PlayerNameWindow.SetActive(false);
                MenuWindow.SetActive(true);
                HostRoomWindow.SetActive(false);
                PreSceneWindow.SetActive(false);
                SearchWindow.SetActive(true);
                ConnectingWindow.SetActive(false);
                CharacterSelectionWindow.SetActive(false);
                break;
            case 2:// Host Room
                PlayerNameWindow.SetActive(false);
                MenuWindow.SetActive(true);
                HostRoomWindow.SetActive(true);
                PreSceneWindow.SetActive(false);
                SearchWindow.SetActive(false);
                CharacterSelectionWindow.SetActive(false);
                if (OptionMenuOpen) { ChangeWindow(3); }
                break;
            case 3://Options
                if (!OptionsWindow.activeSelf) { OptionsWindow.SetActive(true); }
                OptionMenuOpen = !OptionMenuOpen;
                Animation a = OptionsWindow.GetComponent<Animation>();
                if (OptionMenuOpen)
                {
                    a["OptionsWindow"].speed = 1.0f;
                    a.CrossFade("OptionsWindow", 0.2f);
                }
                else
                {
                    if (a["OptionsWindow"].time == 0.0f)
                    {
                        a["OptionsWindow"].time = a["OptionsWindow"].length;
                    }
                    a["OptionsWindow"].speed = -1.0f;
                    a.CrossFade("OptionsWindow", 0.2f);
                }
                break;
            case 4://Menu Window
                PlayerNameWindow.SetActive(false);
                MenuWindow.SetActive(true);
                HostRoomWindow.SetActive(true);
                PreSceneWindow.SetActive(false);
                SearchWindow.SetActive(false);
                CharacterSelectionWindow.SetActive(false);
                break;
            case 5://Create Room
                PlayerNameWindow.SetActive(false);
                MenuWindow.SetActive(false);
                HostRoomWindow.SetActive(false);
                PreSceneWindow.SetActive(true);
                SearchWindow.SetActive(false);
                CharacterSelectionWindow.SetActive(false);
                if (OptionMenuOpen) { ChangeWindow(3); }
                break;
            case 6://Connecting...
                StartCoroutine(FadeIn(1));
                PlayerNameWindow.SetActive(false);
                MenuWindow.SetActive(false);
                HostRoomWindow.SetActive(false);
                PreSceneWindow.SetActive(false);
                SearchWindow.SetActive(false);
                ConnectingWindow.SetActive(true);
                SelectServerWindow.SetActive(false);
                CharacterSelectionWindow.SetActive(false);
                if (OptionMenuOpen) { ChangeWindow(3); }
                break;
            case 7://Select server
                if(PhotonNetwork.IsConnected){ PhotonNetwork.Disconnect();}

                PlayerNameWindow.SetActive(false);
                MenuWindow.SetActive(false);
                HostRoomWindow.SetActive(false);
                PreSceneWindow.SetActive(false);
                SearchWindow.SetActive(false);
                ConnectingWindow.SetActive(false);
                SelectServerWindow.SetActive(true);
                CharacterSelectionWindow.SetActive(false);
                break;
            case 8:// Character Selection
                PlayerNameWindow.SetActive(false);
                MenuWindow.SetActive(true);
                HostRoomWindow.SetActive(false);
                PreSceneWindow.SetActive(false);
                SearchWindow.SetActive(false);
                CharacterSelectionWindow.SetActive(true);
                if (OptionMenuOpen) { ChangeWindow(3); }
                break;
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> ri)
    {
        CacheRoomList.ForEach(x => Destroy(x));
        CacheRoomList.Clear();

        UpdateCachedRoomList(ri);
        RoomListText.canvasRenderer.SetAlpha(0);
        RoomListText.CrossFadeAlpha(1, 2, true);

        InstanceRoomList();
    }

    /// <summary>
    /// 
    /// </summary>
    void InstanceRoomList()
    {
        //Update List
        if (cachedRoomList.Count > 0)
        {
            RoomListText.text = string.Empty;
            foreach (RoomInfo info in cachedRoomList.Values)
            {
                GameObject entry = Instantiate(RoomInfoPrefab);
                entry.transform.SetParent(RoomPanel, false);
                entry.GetComponent<bl_RoomInfo>().GetInfo(info);
                CacheRoomList.Add(entry);
            }
        }
        else
        {
            RoomListText.text = "There are no available rooms, create one to get started.";
        }
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    public void ChangerTime(bool b)
    {
        if (b)
        {
            if (Lobby.r_Time < Lobby.RoomTime.Length)
            {
                Lobby.r_Time++;
                if (Lobby.r_Time > (Lobby.RoomTime.Length - 1))
                {
                    Lobby.r_Time = 0;

                }

            }
        }
        else
        {
            if (Lobby.r_Time < Lobby.RoomTime.Length)
            {
                Lobby.r_Time--;
                if (Lobby.r_Time < 0)
                {
                    Lobby.r_Time = Lobby.RoomTime.Length - 1;

                }
            }
        }
        if(Lobby.RoomTime[Lobby.r_Time] <= 0)
        {
            TimeText.text = "∞";
        }
        else
        TimeText.text = (Lobby.RoomTime[Lobby.r_Time] / 60) + " <size=12>Min</size>";
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    public void ChangeMaxPlayer(bool b)
    {
        if (b)
        {
            if (Lobby.m_MaxPlayer < Lobby.MaxPlayers.Length)
            {
                Lobby.m_MaxPlayer++;
                if (Lobby.m_MaxPlayer > (Lobby.MaxPlayers.Length - 1))
                {
                    Lobby.m_MaxPlayer = 0;

                }

            }
        }
        else
        {
            if (Lobby.m_MaxPlayer < Lobby.MaxPlayers.Length)
            {
                Lobby.m_MaxPlayer--;
                if (Lobby.m_MaxPlayer < 0)
                {
                    Lobby.m_MaxPlayer = Lobby.MaxPlayers.Length - 1;

                }
            }
        }
        MaxPlayersText.text = "Players: "+ Lobby.MaxPlayers[Lobby.m_MaxPlayer] ;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="b"></param>
    public void ChangeMap(bool b)
    {
        if (b)
        {
            if (m_Lobby.CurrentScene < m_Lobby.SceneManager.Count)
            {
                m_Lobby.CurrentScene++;
                if (m_Lobby.CurrentScene > (m_Lobby.SceneManager.Count - 1))
                {
                    m_Lobby.CurrentScene = 0;
                }
            }
        }
        else
        {
            if (m_Lobby.CurrentScene < m_Lobby.SceneManager.Count)
            {
                m_Lobby.CurrentScene--;
                if (m_Lobby.CurrentScene < 0)
                {
                    m_Lobby.CurrentScene = Lobby.SceneManager.Count - 1;
                }
            }
        }
        MapPreviewImage.sprite = Lobby.SceneManager[Lobby.CurrentScene].PreviewImage;
        MapNameText.text = Lobby.SceneManager[Lobby.CurrentScene].MapName;
    }
    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonStatusText != null)
            {
                PhotonStatusText.text = "Connection State: " + bl_CoopUtils.CoopColorStr(PhotonNetwork.NetworkingClient.State.ToString());
                
            }
            if (PhotonRegionText != null)
            {
                PhotonRegionText.text = "Connected to: " + bl_CoopUtils.CoopColorStr(bl_CoopUtils.RegionToString(PhotonNetwork.CloudRegion));
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="n"></param>
    public void UpdatePlayerName(InputField n)
    {
        if (System.String.IsNullOrEmpty(n.text))
        {
            Debug.Log("Player Name can not be empty!");
        }
        else
        {
            PhotonNetwork.NickName = n.text;
            //  ChangeWindow(1);
            ChangeWindow(6);
            Lobby.Connect();
        }
    }

    public void SetRoomPassword(InputField field)
    {
        string pass = field.text;
        if (string.IsNullOrEmpty(pass)) return;

        if (bl_Lobby.Instance.pendingRoomInfo.m_Room.PlayerCount < bl_Lobby.Instance.pendingRoomInfo.m_Room.MaxPlayers)
        {
            if(pass != bl_Lobby.Instance.pendingRoomInfo.RoomPassword)
            {
                Debug.Log("Wrong Password");
                return;
            }
            PhotonNetwork.JoinRoom(bl_Lobby.Instance.pendingRoomInfo.m_Room.Name);
            ChangeWindow(5);
            RoomPasswordWindow.SetActive(false);
        }
        else
        {
            Debug.Log("This Room is Full");
            RoomPasswordWindow.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public IEnumerator FadeIn(float t)
    {
        if (FadeImage == null || isFadeEvent)
            yield return null;

        isFadeEvent = true;
        FadeImage.gameObject.SetActive(true);
        Color c = FadeImage.color;
        while (t > 0.0f)
        {
            t -= Time.deltaTime;
            c.a = t;
            FadeImage.color = c;
            yield return null;
        }
        FadeImage.gameObject.SetActive(false);
        isFadeEvent = false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public IEnumerator FadeOut(float t)
    {
        if (FadeImage == null || isFadeEvent)
            yield return null;

        isFadeEvent = true;
        FadeImage.gameObject.SetActive(true);
        Color c = FadeImage.color;
        while (c.a < t)
        {
            c.a += Time.deltaTime;
            FadeImage.color = c;
            yield return null;
        }
        isFadeEvent = false;
    }
    /// <summary>
    /// 
    /// </summary>
    public void Fade()
    {
        StartCoroutine(FadeOut(2));
    }

    private bl_Lobby m_Lobby;
    private bl_Lobby Lobby
    {
        get
        {
            if (m_Lobby == null)
            {
                m_Lobby = this.GetComponent<bl_Lobby>();
            }
            return m_Lobby;
        }
    }

    public override void OnJoinedLobby()
    {
        ChangeWindow(1);
    }

    void CheckPlayerName()
    {
        if (string.IsNullOrEmpty(PhotonNetwork.NickName))
        {
            ChangeWindow(0);
        }
        else
        {
            ChangeWindow(6);
            Lobby.Connect();
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("We joined master.");
    }

    public static bl_LobbyUI _Lobby;
    public static bl_LobbyUI Instance
    {
        get
        {
            if (_Lobby == null) { _Lobby = FindObjectOfType<bl_LobbyUI>(); }
            return _Lobby;
        }
    }
}