using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_FactorArea : MonoBehaviour
{
    public float Force = 5;
    public float Gravity = 1;

    private Dictionary<bl_VehicleManager, Collider> EnterVehicleRegister = new Dictionary<bl_VehicleManager,Collider>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(bl_PlayerPhoton.PlayerTag))
        {
            other.GetComponent<bl_PlayerMovement>().SetFactor(true, transform.right, Force, Gravity);
        }
        else if (other.CompareTag("Vehicle"))
        {
            bl_VehicleManager cc = other.transform.root.GetComponent<bl_VehicleManager>();
            if(cc != null && cc.isMine)
            {
                //because vehicle have various collider we not to be sure only the first one of they that enter in the trigger call the function
                //so simply add a register list of the enter collider, we check if this vehicle has not been register yet, otherwise just ignore.
                if (!EnterVehicleRegister.ContainsKey(cc))
                {
                    cc.SetFactor(true, transform.right, Force);
                    EnterVehicleRegister.Add(cc, other);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(bl_PlayerPhoton.PlayerTag))
        {
            other.GetComponent<bl_PlayerMovement>().SetFactor(false, Vector3.zero);
        }
        else if (other.CompareTag("Vehicle"))
        {
            bl_VehicleManager cc = other.transform.root.GetComponent<bl_VehicleManager>();
            if (cc != null && cc.isMine)
            {
                if (EnterVehicleRegister.ContainsKey(cc))
                {
                    //if is this the collider that register the vehicle
                    if (EnterVehicleRegister[cc] == other)
                    {
                        EnterVehicleRegister.Remove(cc);
                        cc.SetFactor(false, Vector3.zero, 1);
                    }
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, transform.right * Force);
    }
}