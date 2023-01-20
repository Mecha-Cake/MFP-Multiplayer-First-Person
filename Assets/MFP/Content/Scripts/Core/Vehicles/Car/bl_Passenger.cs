using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class bl_Passenger : bl_PhotonHelper
{
    [Separator("Vehicle Seats")]
    public List<Seat> Seats = new List<Seat>();

    private bool LocalOnTrigger = false;
    public GameObject Player { get; set; }
    private bl_PlayerCar PlayerCar;
    private int SeatTriggerID = -1;
    private bl_VehicleManager VehicleManager;
    private bl_VehicleUI VehicleUI;
    private bool isLocalIn = false;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        VehicleManager = GetComponent<bl_VehicleManager>();
        VehicleUI = FindObjectOfType<bl_VehicleUI>();
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (Player != null)
        {
            InputControll();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void InputControll()
    {
        if (Input.GetKeyDown(VehicleManager.EnterKey))
        {
            if (PlayerCar.isPassenger && isLocalIn)
            {
                ExitSeat();
            }
            else if(LocalOnTrigger && !Seats[SeatTriggerID].isOcuped)
            {
                EnterInSeat();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void EnterInSeat()
    {
        Seat s = Seats[SeatTriggerID];
        bl_EventHandler.OnLocalPlayerVehicle(true, VehicleType.Car);
        PlayerCar.OnEnterInSeat(this, s);
        s.CameraView.SetActive(true);
        Player.transform.parent = VehicleManager.PlayerHolder;
        Player.transform.localPosition = s.Position;
        Player.transform.localEulerAngles = s.Rotation;
        s.isOcuped = true;
        isLocalIn = true;
        LocalOnTrigger = false;
        VehicleUI.SetEnterUI(false);
        LocalPlayerView.RPC("RpcPassengerEvent", RpcTarget.OthersBuffered, true, SeatTriggerID, photonView.ViewID);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ExitSeat(bool byDeath = false)
    {
        if (SeatTriggerID < 0)
            return;

        Seat s = Seats[SeatTriggerID];
        Player.transform.parent = null;
        Player.transform.position = s.ExitPoint.position;
        Player.transform.rotation = s.ExitPoint.rotation;
        s.Passenger = null;
        s.isOcuped = false;
        isLocalIn = false;
        LocalOnTrigger = false;
        s.CameraView.SetActive(false);
        PlayerCar.OnExitFromSeat(byDeath);
        bl_EventHandler.OnLocalPlayerVehicle(false, VehicleType.Car);
        LocalPlayerView.RPC("RpcPassengerEvent", RpcTarget.OthersBuffered, false, SeatTriggerID, photonView.ViewID);
    }

    public void TriggerEnter(int id)
    {
        if (Player == null)
            return;
        if (PlayerCar.isPassenger)
            return;
        if (isSeatOcuped(id))
            return;

        SeatTriggerID = id;
        LocalOnTrigger = true;
        VehicleUI.SetEnterUI(true, VehicleManager.EnterKey);
    }

    public void TriggerExit()
    {
        SeatTriggerID = -1;
        LocalOnTrigger = false;
        VehicleUI.SetEnterUI(false);
    }

    public void SetDamage(int damage, bool byExplosion)
    {
        for (int i = 0; i < Seats.Count; i++)
        {
            if (Seats[i].isOcuped && Seats[i].Passenger != null)
            {
                if (!Seats[i].Passenger.GetComponent<bl_PlayerDamage>().DoDamage(damage * 4))//make sure to passenger die because otherwise require kick out over network.
                {
                    //if they for some reason don't die after vehicle explosion...
                }
                
            }
        }
    }

    public bool isSeatOcuped(int id) { return Seats[id].isOcuped; }
    public bool hasAnySeatOcuped
    {
        get
        {
            for (int i = 0; i < Seats.Count; i++)
            {
                if (Seats[i].isOcuped) { return true; }
            }
            return false;
        }
    }

    void OnEnable()
    {
        bl_EventHandler.LocalPlayerSpawnEvent += OnLocalPlayerSpawn;
    }
    void OnDisable()
    {
        bl_EventHandler.LocalPlayerSpawnEvent -= OnLocalPlayerSpawn;
    }
    public PhotonView LocalPlayerView { get; set; }
    void OnLocalPlayerSpawn(GameObject player)
    {
        Player = player;
        LocalPlayerView = Player.GetComponent<PhotonView>();
        PlayerCar = Player.GetComponent<bl_PlayerCar>();
    }

    void OnDrawGizmosSelected()
    {
        if (playerHolder == null)
            return;

        Gizmos.matrix = playerHolder.localToWorldMatrix;
        for (int i = 0; i < Seats.Count; i++)
        {
            Color c = Color.blue;
            Gizmos.color = c;
            Gizmos.DrawWireSphere(Seats[i].Position, 0.5f);
            c.a = 0.4f;
            Gizmos.color = c;
            Gizmos.DrawSphere(Seats[i].Position, 0.5f);
           
        }
        Gizmos.matrix = Matrix4x4.identity;
    }

    public Transform playerHolder
    {
        get
        {
            if(VehicleManager == null) { VehicleManager = GetComponent<bl_VehicleManager>(); }
            return VehicleManager.PlayerHolder;
        }
    }

    [System.Serializable]
    public class Seat
    {
        public string Name;
        public Vector3 Position;
        public Vector3 Rotation;
        public GameObject CameraView;
        public Transform ExitPoint;
        public GameObject Passenger;
        public bool PlayerVisibleInside = true;
       [HideInInspector] public bool isOcuped = false;

        private Transform _heatLook;
        public Transform HeatLook
        {
            get
            {
                if(_heatLook == null)
                {
                    _heatLook = CameraView.transform.GetChild(0);
                }
                return _heatLook;
            }
        }
    }
}