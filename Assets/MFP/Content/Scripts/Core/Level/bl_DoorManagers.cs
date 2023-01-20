using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class bl_DoorManagers : MonoBehaviourPunCallbacks
{
    public List<bl_SimpleDoor> MapDoors = new List<bl_SimpleDoor>();
    public List<int> DoorsStates = new List<int>();
    private bool initializated = false;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        Initialized();
    }

    /// <summary>
    /// 
    /// </summary>
    private void Initialized()
    {
        if (initializated) return;

        for (int i = 0; i < MapDoors.Count; i++) { DoorsStates.Add(0); }
        initializated = true;
    }


    public void RegisteDoor(bl_SimpleDoor door)
    {
        if (!MapDoors.Contains(door))
        {
            MapDoors.Add(door);
        }
    }

    public void ChangeDoorState(bl_SimpleDoor door, int state)
    {
        if (MapDoors.Contains(door))
        {
            int index = MapDoors.FindIndex(x => x == door);
            DoorsStates[index] = state;
            //we don't need buffer the states
            photonView.RPC("RpcDoorState", RpcTarget.Others, index, state);
        }
        else
        {
            Debug.LogWarning("Door is not register in this map door list.");
        }
    }

    [PunRPC]
    void RpcDoorState(int doorId, int state)
    {
        DoorsStates[doorId] = state;
        MapDoors[doorId].ApplyState(state);
    }

    [PunRPC]
    void RpcAllDoorState(int[] state)
    {
        Initialized();
        for (int i = 0; i < state.Length; i++)
        {
            DoorsStates[i] = state[i];
            MapDoors[i].ApplyStateInsta(state[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //send the door states to the new player
            photonView.RPC("RpcAllDoorState", newPlayer, DoorsStates.ToArray());
        }
    }
}