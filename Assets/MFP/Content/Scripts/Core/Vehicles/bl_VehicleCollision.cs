using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class bl_VehicleCollision : bl_PhotonHelper
{

    [Range(1,100)] public float MaxCollisionDamage = 90;
    public float MinCollisionForceToDamage = 20;
    [Range(1, 100)] public float DeathlyImpactForce = 60;

    public bool DamagePlayersByCollision = false;
    [SerializeField] private AudioClip CollisionAudio = null;

    private List<Collider> LocalColliders = new List<Collider>();
    private bl_VehicleManager Vehicle;
    private float LastTime;
    public bool StopDetect { get; set; }
    private Rigidbody m_Rigidbody;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        LocalColliders.AddRange(transform.GetComponentsInChildren<Collider>());
        Vehicle = GetComponent<bl_VehicleManager>();
        LastTime = Time.time;
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine || StopDetect) return;
        if (LocalColliders.Contains(collision.collider) || collision.transform.tag == "NonDamage")
            return;

        float impact = collision.relativeVelocity.magnitude;
        float relativeForce = Mathf.Clamp01(impact / DeathlyImpactForce);
        float damage = MaxCollisionDamage * relativeForce;

        if (DamagePlayersByCollision && m_Rigidbody.velocity.magnitude > 1)
        {
            if (collision.transform.tag == bl_PlayerPhoton.PlayerTag || collision.transform.tag == bl_PlayerPhoton.RemoteTag)
            {
                    collision.transform.GetComponent<bl_PlayerDamage>().DoDamage((int)damage);
                    return;
            }
        }

        float dot = Vector3.Dot(-transform.up, collision.contacts[0].normal);
        bool isDown = (dot < -0.9);
        float mxf = (isDown) ? MinCollisionForceToDamage * 2.2f : MinCollisionForceToDamage;

        if (impact > mxf && Time.time > LastTime)
        {
            AudioSource.PlayClipAtPoint(CollisionAudio, transform.position, 0.2f + relativeForce);
            Vehicle.OnDamage((int)damage);
            LastTime = Time.time + 0.5f;
        }
    }

    public bool isLocalOwner { get { return Vehicle.isPlayerIn; } }
}