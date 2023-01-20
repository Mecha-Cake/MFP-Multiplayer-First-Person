using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;

public class bl_VehicleManager : bl_PhotonHelper, IPunObservable
{
    [Separator("Settings")]
    public VehicleType m_VehicleType = VehicleType.Car;
    public KeyCode EnterKey = KeyCode.E;
    public List<MonoBehaviour> VehicleScripts = new List<MonoBehaviour>();

    [Separator("Health")]
    public bool AutoRespawn = true;
    public int Health = 200;
    public int ExplosionPlayerDamage = 50;
    [Range(0.01f,2)] public float DamageMultiplier = 0.45f;
    public int RespawnIn = 5;
    public GameObject ExplosionParticles;

    [Separator("Driver")]
    public bool PlayerVisibleInside = true;
    public bool AnimatedEnter = true;
    public Vector3 DriverPosition;
    public Vector3 DriverRotation;
    [Range(0.01f, 1)] public float SteeringHandSpace = 0.1f;
    [Range(10, 180)] public float SteeringWheelMaxAngle = 90;
    [Separator("References")]

    public Transform ExitPoint = null;
    public Transform SteeringWheelPoint;
    public Transform PlayerHolder;

    private bool LocalInVehicle = false;
    public bool RemoteInVehicle = false;

    private CarController CarScript;
    private AeroplaneController JetScript;
    private PhotonView view;
    public GameObject Player { get; set; }
    private Animator m_Animator;
    private bl_VehicleUI VehicleUI;
    private bl_Passenger Passengers;
    private bl_VehicleCollision VehicleCollision;
    private float Vertical;
    private float Horizontal;
    public float Input1 { get; set; }
    public Vector3 Velocity { get; set; }
    private float Input2;
    private bool LocalOnTrigger = false;
    private bool AirBreak;
    private int InitialHealth = 100;

    private Vector3 InitialPosition;
    private Vector3 InitialRotation;
    private Rigidbody m_Rigidbody;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        view = GetComponent<PhotonView>();
        VehicleUI = FindObjectOfType<bl_VehicleUI>();
        Passengers = GetComponent<bl_Passenger>();
        m_Rigidbody = GetComponent<Rigidbody>();
        VehicleCollision = GetComponent<bl_VehicleCollision>();
        if (m_VehicleType == VehicleType.Car)
        {
            CarScript = GetComponent<CarController>();
        }
        else if (m_VehicleType == VehicleType.Jet)
        {
            JetScript = GetComponent<AeroplaneController>();
            m_Animator = GetComponent<Animator>();
        }
        if (!view.ObservedComponents.Contains(this))
        {
            view.ObservedComponents.Add(this);
        }
        if (PhotonNetwork.IsMasterClient)
        {
            InitialPosition = transform.position;
            InitialRotation = transform.eulerAngles;
            InitialHealth = Health;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (PhotonNetwork.IsMasterClient && view.Owner == null)
        {
            view.RequestOwnership();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (Player != null)
        {
            Inputs();
            Speedometer();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void FixedUpdate()
    {
        PositionControl();
    }

    /// <summary>
    /// 
    /// </summary>
    void PositionControl()
    {
        if (m_VehicleType == VehicleType.Car)
        {
            CarPosition();
        }
        else if (m_VehicleType == VehicleType.Jet)
        {
            m_Rigidbody.isKinematic = !photonView.IsMine;
            JetPosition();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CarPosition()
    {
        Velocity = CarScript.Velocity;
        if (isPlayerIn)
        {
            Horizontal = CarScript.m_steerin;
            Vertical = CarScript.m_accel;
            Input1 = CarScript.m_brakeInput;
            Input2 = CarScript.m_handbrake;
        }
        else if (inMyControl && !LocalInVehicle)
        {
            CarScript.Move(0, 0, 0, 0, false, Velocity);
        }
        else
        {
            CarScript.Move(Horizontal, Vertical, Input1, Input2, true, Velocity);
        }
        SteeringWheelControl();
    }

    /// <summary>
    /// 
    /// </summary>
    void SteeringWheelControl()
    {
        if (SteeringWheelPoint == null)
            return;

        Vector3 v = SteeringWheelPoint.localEulerAngles;
        v.z = -(Horizontal * SteeringWheelMaxAngle);
        SteeringWheelPoint.localEulerAngles = v;

    }

    /// <summary>
    /// 
    /// </summary>
    void JetPosition()
    {
        if (isPlayerIn)
        {
            Input1 = JetScript.RollInput;
            Input2 = JetScript.PitchInput;
            AirBreak = JetScript.AirBrakes;
            Horizontal = JetScript.YawInput;
            Vertical = JetScript.ThrottleInput;
        }
        else if (inMyControl && !LocalInVehicle)
        {
            m_Animator.SetInteger("GearState", 1);
        }
        else
        {
            JetScript.Move(Input1, Input2, Horizontal, Vertical, AirBreak);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Speedometer()
    {
        if (!isPlayerIn)
            return;
        float speed = 0;
        if (m_VehicleType == VehicleType.Car)
        {
            speed = CarScript.CarVelocity;
            VehicleUI.UpdateSpeedometer(speed, m_VehicleType, CarScript.m_SpeedType);
        }
        else if (m_VehicleType == VehicleType.Jet)
        {
            speed = JetScript.ForwardSpeed;
            VehicleUI.UpdateSpeedometer(speed, m_VehicleType);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Inputs()
    {
        if (Input.GetKeyDown(EnterKey))
        {
            if (isPlayerIn)
            {
                OnExit();
            }
            else if(LocalOnTrigger && !RemoteInVehicle)
            {
                OnEnter();
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void OnEnter()
    {
        if (view.Owner == null || view.Owner.UserId != PhotonNetwork.LocalPlayer.UserId)
        {
            view.RequestOwnership();
        }
        bl_EventHandler.OnLocalPlayerVehicle(true, m_VehicleType);
        foreach (MonoBehaviour m in VehicleScripts)
        {
            m.enabled = true;
        }
        if(m_VehicleType == VehicleType.Jet) { JetScript.Reset(); }

        if (AnimatedEnter) { bl_VehicleCamera.Instance.SetCarTarget(transform, Player.GetComponent<bl_PlayerPhoton>().PlayerCamera.transform); }
        else { bl_VehicleCamera.Instance.SetCarTarget(transform); }

        Player.transform.parent = PlayerHolder;
        Player.transform.localPosition = DriverPosition;
        Player.transform.localEulerAngles = DriverRotation;

        Player.GetComponent<bl_PlayerCar>().OnEnterLocal(this);
        LocalInVehicle = true;
        LocalOnTrigger = false;
        VehicleUI.SetEnterUI(false);
        VehicleUI.OnEnter(m_VehicleType);
        VehicleUI.SetHealth(Health);
        LocalPlayerView.RPC("NetworkCarEvent", RpcTarget.OthersBuffered, true, m_VehicleType, photonView.ViewID, PlayerVisibleInside);
        view.RPC(nameof(InAndOutEvent), RpcTarget.OthersBuffered, true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnExit(bool byDeath = false)
    {
        foreach (MonoBehaviour m in VehicleScripts)
        {
            m.enabled = false;
        }
        if (m_VehicleType == VehicleType.Jet) { JetScript.Immobilize(); }
        bl_VehicleCamera.Instance.Disable();
        if (Player != null)
        {
            Player.transform.parent = null;
            Player.transform.position = ExitPoint.position;
            Vector3 r = bl_VehicleCamera.Instance.transform.eulerAngles;
            r.x = Player.transform.eulerAngles.x;
            Player.transform.rotation = Quaternion.Euler(r);
            Player.GetComponent<bl_PlayerCar>().OnExitLocal(this, byDeath);
        }
        LocalInVehicle = false;
        RemoteInVehicle = false;
        VehicleUI.OnExit(m_VehicleType);
        bl_EventHandler.OnLocalPlayerVehicle(false, m_VehicleType);
        LocalPlayerView.RPC("NetworkCarEvent", RpcTarget.OthersBuffered, false, m_VehicleType, photonView.ViewID, PlayerVisibleInside);
        view.RPC(nameof(InAndOutEvent), RpcTarget.OthersBuffered, false);
    }

    [PunRPC]
    void InAndOutEvent(bool GetIn)
    {
        RemoteInVehicle = GetIn;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnEnterDetector()
    {
        if (isPlayerIn)
            return;

        LocalOnTrigger = true;
        VehicleUI.SetEnterUI(true, EnterKey);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnExitDetector()
    {
        LocalOnTrigger = false;
        VehicleUI.SetEnterUI(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (m_VehicleType == VehicleType.Car)
            {
                stream.SendNext(Horizontal);
                stream.SendNext(Vertical);
                stream.SendNext(Input1);
                stream.SendNext(Input2);
                stream.SendNext(Velocity);
            }
            else if (m_VehicleType == VehicleType.Jet)
            {
                stream.SendNext(Input1);
                stream.SendNext(Input2);
                stream.SendNext(AirBreak);
                stream.SendNext(Horizontal);
                stream.SendNext(Vertical);
            }
        }
        else
        {
            if (m_VehicleType == VehicleType.Car)
            {
                Horizontal = (float)stream.ReceiveNext();
                Vertical = (float)stream.ReceiveNext();
                Input1 = (float)stream.ReceiveNext();
                Input2 = (float)stream.ReceiveNext();
                Velocity = (Vector3)stream.ReceiveNext();
            }
            else if (m_VehicleType == VehicleType.Jet)
            {
                Input1 = (float)stream.ReceiveNext();
                Input2 = (float)stream.ReceiveNext();
                AirBreak = (bool)stream.ReceiveNext();
                Horizontal = (float)stream.ReceiveNext();
                Vertical = (float)stream.ReceiveNext();
            }
        }
    }

    #region Health
    /// <summary>
    /// 
    /// </summary>
    public void OnDamage(int impact)
    {
        if (isPlayerIn && Player != null)
        {
            Player.GetComponent<bl_PlayerDamage>().DoDamage(impact);
        }
        if(Passengers != null && Passengers.hasAnySeatOcuped)
        {
            Passengers.SetDamage(impact, false);
        }
        photonView.RPC("SetVehicleDamage", RpcTarget.All, impact);
    }

    [PunRPC]
    void SetVehicleDamage(int damage)
    {
        Health -= Mathf.FloorToInt((damage * DamageMultiplier));
        if (isPlayerIn)
        {
            VehicleUI.SetHealth(Mathf.Clamp(Health, 0, 1000));
        }
        if(Health < 1)
        {
            Health = 0;
            Destroyed();
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Destroyed()
    {
        if (ExplosionParticles != null)
        {
            GameObject g = Instantiate(ExplosionParticles, transform.position, Quaternion.identity) as GameObject;
            Destroy(g, 2);
        }

        StopRigid();
        if (inMyControl && AutoRespawn)
        {
            if(VehicleCollision != null) { VehicleCollision.StopDetect = true; }
            if (Passengers != null && Passengers.hasAnySeatOcuped)
            {
                Passengers.SetDamage(ExplosionPlayerDamage, true);
            }
            if (isPlayerIn)
            {
                if (!Player.GetComponent<bl_PlayerDamage>().DoDamage(ExplosionPlayerDamage))//apply damage when the vehicle explode with the player inside
                {
                    OnExit();//if the player not die with the explosion kick out from the vehicle
                }
            }
            Invoke("Respawn", RespawnIn);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void StopRigid()
    {
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
        m_Rigidbody.Sleep();
    }

    /// <summary>
    /// 
    /// </summary>
    void Respawn()
    {
        transform.position = InitialPosition;
        transform.eulerAngles = InitialRotation;
        photonView.RPC("SyncHealth", RpcTarget.All, InitialHealth);
        view.RPC("InAndOutEvent", RpcTarget.OthersBuffered, false);
    }

    [PunRPC]
    void SyncHealth(int health)
    {
        Health = health;
        if (VehicleCollision != null) { VehicleCollision.StopDetect = false; }
        if (AutoRespawn)
        gameObject.SetActive(true);
        StopRigid();
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public void SetFactor(bool apply, Vector3 direction, float Force)
    {
        if (m_VehicleType == VehicleType.Car)
        {
            CarScript.ApplyFactor(apply, direction, Force);
        }
        else if (m_VehicleType == VehicleType.Jet)
        {

        }
    }

    #region Callbacks
    [PunRPC]
    void InitialSync(Vector3 initPos, Vector3 initRot, int InitHealth)
    {
        InitialPosition = initPos;
        InitialRotation = initRot;
        InitialHealth = InitHealth;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        bl_EventHandler.LocalPlayerSpawnEvent += OnLocalPlayerSpawn;
        bl_PhotonCallbacks.Instance.OnPlayerEnter+=(OnPhotonPlayerConnected);
        bl_PhotonCallbacks.Instance.OnPlayerLeft+=(OnPhotonPlayerDisconnected);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        bl_EventHandler.LocalPlayerSpawnEvent -= OnLocalPlayerSpawn;
        if (bl_PhotonCallbacks.Instance != null)
        {
            bl_PhotonCallbacks.Instance.OnPlayerEnter -= (OnPhotonPlayerConnected);
            bl_PhotonCallbacks.Instance.OnPlayerLeft -= (OnPhotonPlayerDisconnected);
        }
    }


    public PhotonView LocalPlayerView { get; set; }
    void OnLocalPlayerSpawn(GameObject player)
    {
        Player = player;
        LocalPlayerView = Player.GetComponent<PhotonView>();
    }

    public void OnPhotonPlayerDisconnected(Player otherPlayer)
    {
        if (view == null)
            return;
        if (view.Owner == null || view.Owner.NickName == "Scene")
        {
            LocalInVehicle = false;
            view.RPC("InAndOutEvent", RpcTarget.OthersBuffered, false);
            return;
        }
        //is the player in car.
        if (view.Owner.NickName == otherPlayer.NickName)
        {
            LocalInVehicle = false;
            view.RPC("InAndOutEvent", RpcTarget.OthersBuffered, false);
        }
    }

    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && view.Owner == null)
        {
            view.RequestOwnership();
        }
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncHealth", newPlayer, Health);
            photonView.RPC("InitialSync", newPlayer, InitialPosition, InitialRotation, InitialHealth);
        }
    }
    #endregion

    #region Get
    public bool inMyControl
    {
        get
        {
            bool b = false;
            if (view.IsMine)
            {
                b = true;
            }
            return b;
        }
    }
    public bool isPlayerIn { get { return inMyControl && LocalInVehicle; } }
    public bool isUsed => isPlayerIn || RemoteInVehicle;

    private Transform _heatLook;
    public Transform HeatLook
    {
        get
        {
            if (_heatLook == null)
            {
                _heatLook = bl_VehicleCamera.Instance.transform.GetChild(0);
            }
            return _heatLook;
        }
    }
    public Vector3 SteeringHandRight { get { return SteeringWheelPoint.position + (SteeringWheelPoint.right * SteeringHandSpace); } }
    public Vector3 SteeringHandLeft { get { return SteeringWheelPoint.position - (SteeringWheelPoint.right * SteeringHandSpace); ; } }
    #endregion

    void OnDrawGizmosSelected()
    {
        if (PlayerHolder == null || !PlayerVisibleInside)
            return;

        Gizmos.matrix = PlayerHolder.localToWorldMatrix;
        Color c = Color.green;
        Gizmos.color = c;
        Gizmos.DrawWireSphere(DriverPosition, 0.5f);
        c.a = 0.4f;
        Gizmos.color = c;
        Gizmos.DrawSphere(DriverPosition, 0.5f);
        Gizmos.matrix = Matrix4x4.identity;
        if (SteeringWheelPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(SteeringWheelPoint.position, 0.1f);
            Gizmos.color = Color.yellow;
            Vector3 center = SteeringWheelPoint.position;
            Gizmos.DrawSphere(center + (SteeringWheelPoint.right * SteeringHandSpace), 0.1f);
            Gizmos.DrawSphere(center - (SteeringWheelPoint.right * SteeringHandSpace), 0.1f);
        }
    }
}