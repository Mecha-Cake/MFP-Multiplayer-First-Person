using UnityEngine;

public class bl_VehicleEnterTrigger : MonoBehaviour
{

    [SerializeField] private bl_VehicleManager Vehicle = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    void OnTriggerEnter(Collider c)
    {
        if (c.transform.tag == bl_PlayerPhoton.PlayerTag)
        {
            Vehicle.OnEnterDetector();
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
            Vehicle.OnExitDetector();
        }
    }

}