using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System;

[DefaultExecutionOrder(-10)]
public class bl_PhotonCallbacks : MonoBehaviourPunCallbacks
{

    public Action<Player> OnPlayerEnter;
    public Action<Player> OnMasterClientSwitchedEvent;
    public Action<Player> OnPlayerLeft;

    public Action OnLeft;
    public Action OnJoined;
    public Action OnJoinLobby;
    public Action<DisconnectCause> OnDisconnect;

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if(OnPlayerEnter != null)
        {
            OnPlayerEnter.Invoke(newPlayer);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        if (OnMasterClientSwitchedEvent != null)
        {
            OnMasterClientSwitchedEvent.Invoke(newMasterClient);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (OnPlayerLeft != null)
        {
            OnPlayerLeft.Invoke(otherPlayer);
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        if (OnLeft != null)
        {
            OnLeft.Invoke();
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (OnJoined != null)
        {
            OnJoined.Invoke();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        if (OnDisconnect != null)
        {
            OnDisconnect.Invoke(cause);
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        if (OnJoinLobby != null)
        {
            OnJoinLobby.Invoke();
        }
    }

    private static bl_PhotonCallbacks _instance;
    public static bl_PhotonCallbacks Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_PhotonCallbacks>(); }
            return _instance;
        }
    }
}