using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;

public class bl_PhotonStatic : bl_PhotonHelper {

    [Separator("Settings")]
    [Range(1,5)]public float RandomTime = 2;
    [Range(1,5)]public float UpdateEach = 10;
	[Separator("References")]
    [SerializeField]private Text AllRoomText = null;
    [SerializeField]private Text AllPlayerText = null;
    [SerializeField]private Text AllPlayerInRoomText = null;
    [SerializeField]private Text AllPlayerInLobbyText = null;

    private float GetTime;
    private int AllRooms;
    private int AllPlayers;
    private int AllPlayerInRoom;
    private int AllPlayerInLobby;
    private Animator Anim;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        Anim = GetComponent<Animator>();
        Refresh();
        InvokeRepeating("UpdateRepeting", 1, UpdateEach);
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateRepeting()
    {
        Refresh();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Refresh()
    {
        StopAllCoroutines();
        StartCoroutine(GetStaticsIE());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator GetStaticsIE()
    {
        GetTime = RandomTime;
        while(GetTime > 0)
        {
            GetTime -= Time.deltaTime;
            AllRooms = Random.Range(0, 999);
            AllPlayers = Random.Range(0, 999);
            AllPlayerInRoom = Random.Range(0, 999);
            AllPlayerInLobby = Random.Range(0, 999);
            Set();
            yield return new WaitForEndOfFrame();
        }
        GetPhotonStatics();
        Set();
    }

    /// <summary>
    /// 
    /// </summary>
    void GetPhotonStatics()
    {
        AllRooms = PhotonNetwork.CountOfRooms;
        AllPlayers = PhotonNetwork.CountOfPlayers;
        AllPlayerInRoom = PhotonNetwork.CountOfPlayersInRooms;
        AllPlayerInLobby = PhotonNetwork.CountOfPlayersOnMaster;
    }

    /// <summary>
    /// 
    /// </summary>
    void Set()
    {
        AllRoomText.text = string.Format("ROOMS\n<size=28><b>{0}</b></size>", AllRooms);
        AllPlayerText.text = string.Format("PLAYERS\n<size=28><b>{0}</b></size>", AllPlayers);
        AllPlayerInRoomText.text = string.Format("PLAYERS IN ROOMS\n<size=28><b>{0}</b></size>", AllPlayerInRoom);
        AllPlayerInLobbyText.text = string.Format("PLAYERS IN LOBBY\n<size=28><b>{0}</b></size>", AllPlayerInLobby);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Hide()
    {
        if (Anim == null)
            return;

        Anim.SetBool("show", false);
    }
}