using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using Photon.Realtime;
using Photon.Pun;

public static class bl_PhotonExtensions {


    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="ready"></param>
    public static void SendReady(this Player player, bool ready)
    {
        if (player == null)
            return;

        player.SetCustomProperties(new Hashtable() { { PropertiesKeys.Ready, ready.ToString()} });
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="ready"></param>
    public static bool GetReady(this Player player)
    {
        if (player == null)
            return false;
        string bs = (string)player.CustomProperties[PropertiesKeys.Ready];
        bool b = (bs == "True") ? true : false;
        return b;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Obj"></param>
    /// <returns></returns>
    public static int GetViewID(this GameObject Obj)
    {
        PhotonView v = Obj.GetComponent<PhotonView>();

        return v.ViewID;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Play"></param>
    public static void SendRoomState(this Room room, bool Play = true)
    {
        string b = (Play) ? "True" : "False";
        room.SetCustomProperties(new Hashtable() { { PropertiesKeys.RoomState, b } });
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public static bool GetRoomState(this Room room)
    {
        if (room == null)
            return false;

        string p = (string)room.CustomProperties[PropertiesKeys.RoomState];
        bool b = (p == "True") ? true : false;
        return b;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public static bool GetRoomState(this RoomInfo room)
    {
        string p = (string)room.CustomProperties[PropertiesKeys.RoomState];
        bool b = (p == "True") ? true : false;
        return b;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public static string RoomScene(this Room room)
    {
        string r = (string)room.CustomProperties[PropertiesKeys.SceneNameKey];
        return r;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public static string RoomScene(this RoomInfo room)
    {
        string r = (string)room.CustomProperties[PropertiesKeys.SceneNameKey];
        return r;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    public static bool AllPlayersReady(this Player[] players)
    {
        bool b = true;
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].IsMasterClient)//avoid master client
            {
                if ((string)players[i].CustomProperties[PropertiesKeys.Ready] == "False")
                {
                    b = false;
                }
            }
        }
        return b;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    public static Player GetMasterClient(this Player[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].IsMasterClient)
            {
                return players[i];
            }
        }
        return null;
    }
}