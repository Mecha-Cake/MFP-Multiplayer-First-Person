using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class bl_KillZone : MonoBehaviour
{

    public bool InstaKill = true;
    private BoxCollider m_Collider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(bl_PlayerPhoton.PlayerTag))
        {
            other.GetComponent<bl_PlayerDamage>().DoDamage(100);
        }
        else if (other.CompareTag("Vehicle"))
        {
            other.transform.root.GetComponent<bl_VehicleManager>().OnDamage(200);
        }
    }

    private void OnDrawGizmos()
    {
        if(m_Collider != null)
        {
            Gizmos.color = new Color(0, 0, 0, 0.3f);
            Gizmos.DrawCube(m_Collider.bounds.center, m_Collider.bounds.size);
        }
        else
        {
            m_Collider = GetComponent<BoxCollider>();
            m_Collider.isTrigger = true;
        }
    }
}