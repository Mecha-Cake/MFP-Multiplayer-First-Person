////////////////////////////////////////////////////////////////////////////////
//////////////////// bl_PlayerSync.cs///////////////////////////////////////////
////////////////////use this for the synchronize position , rotation, states,/// 
///////////////////etc ...   via photon/////////////////////////////////////////
////////////////////////////////Briner Games////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class bl_PlayerSync : bl_PhotonHelper, IPunObservable
{
    /// <summary>
    /// the player's team is not ours
    /// </summary>
    [HideInInspector]
    public string RemoteTeam;
    /// <summary>
    /// the current state of the current weapon
    /// </summary>
    public string WeaponState;
    /// <summary>
    /// the object to which the player looked
    /// </summary>
    public Transform HeatTarget;
    /// <summary>
    /// smooth interpolation amount
    /// </summary>
    public float SmoothingDelay = 8f;
    [Space(5)]
    //Script Needed
    [Header("Necessary script")]
    public bl_PlayerAnimator m_PlayerAnimation;

    private float m_Distance;
    private float m_Angle;

    private Vector3 m_Direction;
    private Vector3 m_NetworkPosition;
    private Vector3 m_StoredPosition;

    private Quaternion m_NetworkRotation;

    //private
    private bl_PlayerMovement Controller;
    private bl_PlayerCar PlayerCar;
    private GameObject CurrenGun;
    private int PlayerModelID = -1;
#pragma warning disable 0414
    [SerializeField]
    bool ObservedComponentsFoldoutOpen = true;
#pragma warning disable 0414

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (!PhotonNetwork.IsConnected)
            Destroy(this);

        if (!this.isMine)
        {
            if (HeatTarget.gameObject.activeSelf == false)
            {
                HeatTarget.gameObject.SetActive(true);
            }
        }
        Controller = this.GetComponent<bl_PlayerMovement>();
        PlayerCar = GetComponent<bl_PlayerCar>();
        m_StoredPosition = transform.position;
        m_NetworkPosition = Vector3.zero;

        m_NetworkRotation = Quaternion.identity;
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (photonView.IsMine)
        {
            SyncLocalPlayerModel();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SyncLocalPlayerModel()
    {
        SetPlayerModel(bl_GameData.Instance.LocalPlayerModel);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_PhotonCallbacks.Instance.OnPlayerEnter += OnPlayerEnter;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        if(bl_PhotonCallbacks.Instance != null)
        bl_PhotonCallbacks.Instance.OnPlayerEnter -= OnPlayerEnter;
    }

    /// <summary>
    /// serialization method of photon
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            this.m_Direction = transform.position - this.m_StoredPosition;
            this.m_StoredPosition = transform.position;

            stream.SendNext(transform.position);
            stream.SendNext(this.m_Direction);
            stream.SendNext(transform.rotation);
            //We own this player: send the others our data
            if (!PlayerCar.isPlayerInsideVehicle)
            {
                stream.SendNext(HeatTarget.position);
                stream.SendNext(HeatTarget.rotation);
            }
            else
            {
                if (PlayerCar.isInVehicle)// is driver
                {
                    stream.SendNext(PlayerCar.Vehicle.HeatLook.position);
                    stream.SendNext(PlayerCar.Vehicle.HeatLook.rotation);
                }
                else // is passenger
                {
                    stream.SendNext(PlayerCar.m_Seat.HeatLook.transform.position);
                    stream.SendNext(PlayerCar.m_Seat.HeatLook.transform.rotation);
                }
            }
            stream.SendNext(Controller.m_PlayerState);
            stream.SendNext(Controller.grounded);
            stream.SendNext(Controller.vel);
        }
        else
        {
            //Network player, receive data
            this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
            this.m_Direction = (Vector3)stream.ReceiveNext();
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            this.m_NetworkPosition += this.m_Direction * lag;
            this.m_Distance = Vector3.Distance(transform.position, this.m_NetworkPosition);
            this.m_NetworkRotation = (Quaternion)stream.ReceiveNext();
            this.m_Angle = Quaternion.Angle(transform.rotation, this.m_NetworkRotation);

            HeadPos = (Vector3)stream.ReceiveNext();
            HeadRot = (Quaternion)stream.ReceiveNext();
            m_state = (PlayerState)stream.ReceiveNext();
            m_grounded = (bool)stream.ReceiveNext();
            NetVel = (Vector3)stream.ReceiveNext();
        }
    }

    private Vector3 HeadPos = Vector3.zero;// Head Look to
    private Quaternion HeadRot = Quaternion.identity;
    private PlayerState m_state;
    private bool m_grounded;
    private string RemotePlayerName = string.Empty;
    private int CurNetGun;
    private Vector3 NetVel;

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        ///if the player is not ours, then
        if (photonView == null || isConnected == false)
            return;

        if (isMine == true)
        {
            m_PlayerAnimation.m_PlayerState = Controller.m_PlayerState;//send the state of player local for remote animation*/
            m_PlayerAnimation.grounded = Controller.grounded;
            m_PlayerAnimation.velocity = Controller.vel;
        }
        else
        {
            RemotePlayerName = photonView.Owner.NickName;
            if (!PlayerCar.isPlayerInsideVehicle)
            {
                transform.position = Vector3.MoveTowards(transform.position, this.m_NetworkPosition, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, this.m_NetworkRotation, this.m_Angle * (1.0f / PhotonNetwork.SerializationRate));
            }

            //Get information from other client
            this.HeatTarget.position = Vector3.Lerp(this.HeatTarget.position, HeadPos, Time.deltaTime * this.SmoothingDelay);
            this.HeatTarget.rotation = HeadRot;
            m_PlayerAnimation.m_PlayerState = m_state;//send the state of player local for remote animation*/
            m_PlayerAnimation.grounded = m_grounded;
            m_PlayerAnimation.velocity = NetVel;

            gameObject.name = RemotePlayerName;
        }
    }

    public void SetPlayerModel(int ID)
    {
        PlayerModelID = ID;
        photonView.RPC(nameof(RpcGetPlayerModel), RpcTarget.All, ID);
    }

    [PunRPC]
    public void RpcGetPlayerModel(int ID)
    {
        m_PlayerAnimation.SetPlayerModel(ID);
    }

    void OnPlayerEnter(Player player)
    {
        photonView.RPC("RpcGetPlayerModel", player, PlayerModelID);
    }
}