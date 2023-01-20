/////////////////////////////////////////////////////////////////////////////////
//////////////////// bl_EventHandler.cs/////////////////////////////////////////
////////////////////Use this to create new internal events///////////////////////
//this helps to improve the communication of the script through delegated events/
////////////////////////////////Briner Games/////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class bl_EventHandler
{
    //Call all script when Fall Events
    public delegate void FallEvent(float m_amount);
    public static FallEvent OnFall;

    public delegate void RoundEnd();
    public static RoundEnd OnRoundEnd;
    //small impact
    public delegate void SmallImpact();
    public static SmallImpact OnSmallImpact;
    //Log event
    public delegate void LogWindow(bl_LogInfo _info);
    public static LogWindow OnLogWindow;
    //OnPhotonInstantiate
    public delegate void PhotonInstantiate(PhotonMessageInfo _info);
    public static PhotonInstantiate PhotonInstantiateEvent;
    //ON LOCAL PLAYER INSTANTIATE
    public delegate void LocalPlayerSpawn(GameObject player);
    public static LocalPlayerSpawn LocalPlayerSpawnEvent;
    //ON LOCAL PLAYER INSTANTIATE
    public delegate void LocalPlayerVehicle(bool Enter, VehicleType type);
    public static LocalPlayerVehicle LocalPlayerVehicleEvent;
    //ON LOCAL PLAYER DEATH
    public delegate void LocalPlayerDeath();
    public static LocalPlayerDeath LocalPlayerDeathEvent;


    /// <summary>
    /// Callet event when recive Fall Impact
    /// </summary>
    /// <param name="m_amount"></param>
    public static void EventFall(float m_amount)
    {
        if (OnFall != null)
            OnFall(m_amount);
    }

    /// <summary>
    /// Call This when room is finish a round
    /// </summary>
    public static void OnRoundEndEvent()
    {
        if (OnRoundEnd != null)
            OnRoundEnd();
    }
    /// <summary>
    /// 
    /// </summary>
    public static void OnSmallImpactEvent()
    {
        if (OnSmallImpact != null)
            OnSmallImpact();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_info"></param>
    public static void OnLogMsnEvent(bl_LogInfo _info)
    {
        if (OnLogWindow != null)
            OnLogWindow(_info);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_info"></param>
    public static void OnPhotonInstantiate(PhotonMessageInfo _info)
    {
        if (PhotonInstantiateEvent != null)
            PhotonInstantiateEvent(_info);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void OnLocalPlayerSpawn(GameObject player)
    {
        if (LocalPlayerSpawnEvent != null)
            LocalPlayerSpawnEvent(player);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void OnLocalPlayerVehicle(bool Enter, VehicleType type)
    {
        if (LocalPlayerVehicleEvent != null)
            LocalPlayerVehicleEvent(Enter, type);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void OnLocalPlayerDeath()
    {
        if (LocalPlayerDeathEvent != null)
            LocalPlayerDeathEvent();
    }
}