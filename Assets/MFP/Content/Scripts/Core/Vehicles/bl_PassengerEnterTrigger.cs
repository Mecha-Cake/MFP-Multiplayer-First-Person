using UnityEngine;
using System.Collections;

public class bl_PassengerEnterTrigger : MonoBehaviour
{
    public int SeatID = 0;
    [SerializeField] private bl_Passenger Vehicle = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    void OnTriggerEnter(Collider c)
    {
        if (Vehicle.isSeatOcuped(SeatID))
            return;

        if (c.transform.tag == bl_PlayerPhoton.PlayerTag)
        {
            Vehicle.TriggerEnter(SeatID);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    void OnTriggerExit(Collider c)
    {
        if (c.transform.tag == bl_PlayerPhoton.PlayerTag)
        {
            Vehicle.TriggerExit();
        }
    }
}