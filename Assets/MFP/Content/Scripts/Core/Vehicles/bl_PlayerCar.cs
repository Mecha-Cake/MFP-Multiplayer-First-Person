using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public class bl_PlayerCar : bl_PhotonHelper {

    /// <summary>
    /// Objects which are deactivated when the local client entering a car.
    /// </summary>
    public GameObject LocalObjects = null;
    /// <summary>
    /// Objects which are deactivated when the Remote client entering a car.
    /// </summary>
    public GameObject RemoteObjects = null;
    public List<MonoBehaviour> PlayerScripts = new List<MonoBehaviour>();
    public bool isInVehicle { get; set; }
    public bl_VehicleManager Vehicle { get; set; }
    public bl_Passenger Passenger { get; set; }
    public bl_Passenger.Seat m_Seat { get; set; }
    public bool isPassenger { get; set; }

    /// <summary>
    /// This is called when Local player enter in car
    /// you can write here if you need do something in this event.
    /// </summary>
    public void OnEnterLocal(bl_VehicleManager vehicle)
    {
        foreach (MonoBehaviour m in PlayerScripts)
        {
            m.enabled = false;
        }
        if (vehicle.PlayerVisibleInside)
        {
            RemoteObjects.SetActive(true);
            PlayerAnim.ModelAnimator.SetInteger("Vehicle", 2);
        }
        LocalObjects.SetActive(false);
        chararcterController.enabled = false;
        isInVehicle = true;
        Vehicle = vehicle;
    }

    /// <summary>
    /// This is called when Local player exit from car
    /// you can write here if you need do something in this event.
    /// </summary>
    public void OnExitLocal(bl_VehicleManager vehicle, bool byDeath)
    {
        if (!byDeath)
        {
            foreach (MonoBehaviour m in PlayerScripts)
            {
                m.enabled = true;
            }
            chararcterController.enabled = true;
        }
        if (vehicle.PlayerVisibleInside)
        {
            PlayerAnim.ModelAnimator.SetInteger("Vehicle", 0);
            RemoteObjects.SetActive(false);
        }
        LocalObjects.SetActive(true);
        isInVehicle = false;
        Vehicle = null;
    }

    public void OnEnterInSeat(bl_Passenger p, bl_Passenger.Seat s)
    {
        foreach (MonoBehaviour m in PlayerScripts)
        {
            m.enabled = false;
        }
        chararcterController.enabled = false;
        LocalObjects.SetActive(false);
        s.Passenger = gameObject;
        m_Seat = s;
        Passenger = p;
        isPassenger = true;
    }

    public void OnExitFromSeat(bool byDeath = false)
    {
        if (!byDeath)
        {
            foreach (MonoBehaviour m in PlayerScripts)
            {
                m.enabled = true;
            }
            chararcterController.enabled = true;
        }
        LocalObjects.SetActive(true);
        m_Seat = null;
        Passenger = null;
        isPassenger = false;
    }

    /// <summary>
    /// Wait a moment after exit of vehicle for avoid
    /// collision with vehicle colliders.
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForCC()
    {
        yield return new WaitForSeconds(1f);
        chararcterController.enabled = true;
    }

    /// <summary>
    /// /// This is called when Remote player enter in car
    /// you can write here if you need do something in this event.
    /// </summary>
    void OnEnterNetwork(VehicleType vt, int vehicleID, bool playerVisible)
    {
        if (!playerVisible)
        {
            RemoteObjects.SetActive(false);
        }
        chararcterController.enabled = false;
        isInVehicle = true;
        StopCoroutine(WaitForCC());
        if(vt == VehicleType.Jet) { GetComponent<bl_DrawName>().ShowUI(false); }

        GameObject vehicle = PhotonView.Find(vehicleID).gameObject;
        Vehicle = vehicle.GetComponent<bl_VehicleManager>();

        transform.parent = Vehicle.PlayerHolder;
        transform.localPosition = Vehicle.DriverPosition;
        transform.localEulerAngles = Vehicle.DriverRotation;
        PlayerAnim.ModelAnimator.SetInteger("Vehicle", 2);
    }
    /// <summary>
    /// This is called when Remote player exit from car
    /// you can write here if you need do something in this event.
    /// </summary>
    void OnExitNetwork(VehicleType vt)
    {
        transform.parent = null;
        RemoteObjects.SetActive(true);
        StartCoroutine(WaitForCC());
        isInVehicle = false;
        Vehicle = null;
        if(vt == VehicleType.Jet) { GetComponent<bl_DrawName>().ShowUI(true); }
        PlayerAnim.ModelAnimator.SetInteger("Vehicle", 0);
    }

    private CharacterController chararcterController
    {
        get
        {
            return this.GetComponent<CharacterController>();
        }
    }

    [PunRPC]
    void NetworkCarEvent(bool enter, VehicleType vt, int vehicleID, bool playerVisible)
    {
        if (enter)//when enter in a vehicle
        {
            OnEnterNetwork(vt, vehicleID, playerVisible);
        }
        else //when exit of a vehicle
        {
            OnExitNetwork(vt);
        }
    }

    [PunRPC]
    void RpcPassengerEvent(bool enter, int seat, int VehicleID)
    {
        if (enter)
        {
            CancelInvoke("EnableController");
            GameObject vehicle = PhotonView.Find(VehicleID).gameObject;
            Vehicle = vehicle.GetComponent<bl_VehicleManager>();
            Passenger = vehicle.GetComponent<bl_Passenger>();
            m_Seat = Passenger.Seats[seat];
            m_Seat.isOcuped = true;
            m_Seat.Passenger = gameObject;
            chararcterController.enabled = false;
            transform.parent = Passenger.playerHolder;
            transform.localPosition = m_Seat.Position;
            transform.localEulerAngles = m_Seat.Rotation;
            PlayerAnim.ModelAnimator.SetInteger("Vehicle", 1);
            if (!m_Seat.PlayerVisibleInside)
            {
                RemoteObjects.SetActive(false);
            }
        }
        else
        {
            transform.parent = null;
            isPassenger = false;
            m_Seat.isOcuped = false;
            m_Seat.Passenger = null;
            m_Seat = null;
            RemoteObjects.SetActive(true);
            Passenger = null;
            PlayerAnim.ModelAnimator.SetInteger("Vehicle", 0);
            Invoke("EnableController", 1);
        }
        isPassenger = enter;
    }

    void EnableController()
    {
        chararcterController.enabled = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ExitVehicle()
    {
        if (isInVehicle)
        {
            Vehicle.OnExit(false);
        }
        else if (isPassenger)
        {
            Passenger.ExitSeat();
        }
    }

    public bool isPlayerInsideVehicle
    {
        get
        {
            return (isInVehicle || isPassenger);
        }
    }

    public bool isLocal { get {  if (photonView != null) { return photonView.IsMine; } else { return true; } } }

    private bl_PlayerAnimator pAnimator;
    private bl_PlayerAnimator PlayerAnim
    {
        get
        {
            if(pAnimator == null)
            {
                pAnimator = GetComponent<bl_PlayerSync>().m_PlayerAnimation;
            }
            return pAnimator;
        }
    }

}