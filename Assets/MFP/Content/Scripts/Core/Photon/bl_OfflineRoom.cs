using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[DefaultExecutionOrder(-1000)]
public class bl_OfflineRoom : MonoBehaviour, IConnectionCallbacks
{
    [Header("Offline Room")]
    [Range(1, 10)] public int maxPlayers = 1;

    [Header("References")]
    public GameObject PhotonObject;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
        if (!PhotonNetwork.IsConnected && !PhotonNetwork.InRoom)
        {
            if (bl_GameData.Instance.offlineMode)
            {
                PhotonNetwork.OfflineMode = true;
                PhotonNetwork.NickName = "Offline Player";
                Instantiate(PhotonObject);
            }
            else
            {
                PhotonNetwork.OfflineMode = false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnConnectedToMaster()
    {
        Debug.Log("Offline Connected to Master");
        Hashtable roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[PropertiesKeys.TimeRoomKey] = 0;
        roomOption[PropertiesKeys.RoomRoundKey] = "0";
        roomOption[PropertiesKeys.SceneNameKey] = "Offline Room";
        roomOption[PropertiesKeys.RoomState] = "True";

        PhotonNetwork.CreateRoom("Offline Room", new RoomOptions()
        {
            MaxPlayers = (byte)maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            PublishUserId = true,
            EmptyRoomTtl = 0,
        }, null);
    }


    public void OnConnected()
    {
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {      
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {     
    }

    public void OnDisconnected(DisconnectCause cause)
    {     
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {      
    }
}