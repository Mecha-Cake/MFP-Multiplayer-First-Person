using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class bl_ServerCapacityChecker : MonoBehaviour, IConnectionCallbacks
{
    public GameObject MaxCapacityReachedUI;

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnConnected()
    {
    }

    public void OnConnectedToMaster()
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
       if(cause == DisconnectCause.MaxCcuReached)
        {
            MaxCapacityReachedUI?.SetActive(true);
        }
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
    }
}