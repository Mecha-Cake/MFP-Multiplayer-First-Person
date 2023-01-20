using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class bl_PickUpItem : MonoBehaviour, IEventSystemHandler
{
    [Separator("Settings")]
    public string ItemName = string.Empty; //name to display in the UI when pickUP
    public bool InvokeInServer = false; //Invoke delegates in all client or just in the local player?
    public bool Move = true;
    public bool CanVehiclePickup = true;
    public Vector3 RotateDirection = new Vector3(0, 1, 0);
    [Separator("References")]
    [SerializeField]private AudioClip PickupSound = null;

    [Serializable]
    public class OnPickupEvent : UnityEvent { }
    [SerializeField]
    private OnPickupEvent OnPickup = new OnPickupEvent();//Here all function that will be called when this item has ben pickup

    private bl_PickUpManager Manager;
    private float origPosY;
    private float startOffset;


    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        Manager = FindObjectOfType<bl_PickUpManager>();
        if(Manager != null)
        {
            Manager.CacheThis(gameObject);
        }
        else
        {
            Debug.LogWarning("Need a 'bl_PickUpManager' in scene for use Pickup object.");
        }
        origPosY = transform.position.y;
        startOffset = UnityEngine.Random.value * 3f;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    void OnTriggerEnter(Collider c)
    {
        if (c.transform.tag == bl_PlayerPhoton.PlayerTag)
        {
            bl_PlayerPhoton p = c.GetComponent<bl_PlayerPhoton>();
            if (p.isLocalPlayer)
            {
                PickUp();
            }
        }
        //Pick up with vehicles
        if (CanVehiclePickup)
        {
            if (c.transform.tag == "Vehicle")
            {
                if (c.transform.root.GetComponent<bl_VehicleManager>() != null)
                {
                    bl_VehicleManager cm = c.transform.root.GetComponent<bl_VehicleManager>();
                    if (cm.inMyControl)
                    {
                        PickUp();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {
        if (!Move)
            return;

        transform.Rotate(RotateDirection);
        transform.position = new Vector3(base.transform.position.x, this.origPosY + (Mathf.Sin((this.startOffset + Time.realtimeSinceStartup) * 4f) * 0.08f), transform.position.z);
    }

    /// <summary>
    /// 
    /// </summary>
    void PickUp()
    {
        if (Manager != null)
        {
            //called all functions in the list of delegates.
            if(this.enabled && OnPickup != null)
            {
                OnPickup.Invoke();
            }
            //Play Sound if is assigned
            if (PickupSound)
            {
                AudioSource.PlayClipAtPoint(PickupSound, transform.position);
            }
            if (!string.IsNullOrEmpty(ItemName))
            {
                Manager.OnPickUp(ItemName,gameObject.name, InvokeInServer);
            }
            //destroy this item in all clients in room
            Manager.DestroyItem(gameObject);
        }
    }

    public void InvokeLocal()
    {
        //called all functions in the list of delegates.
        if (this.enabled && OnPickup != null)
        {
            OnPickup.Invoke();
        }
    }

}