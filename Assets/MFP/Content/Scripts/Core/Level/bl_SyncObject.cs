using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class bl_SyncObject : bl_PhotonHelper, IPunObservable
{
    [Range(1, 20)]
    public float LerpMovement = 15.0f;
    public bool TakeMaster = true;

    private Vector3 originPos = Vector3.zero;
    private Quaternion originRot = Quaternion.identity;

    private bool ReceiveData = false;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (!view.ObservedComponents.Contains(this))
        {
            view.ObservedComponents.Add(this);
            view.Synchronization = ViewSynchronization.UnreliableOnChange;
        }
    }

    private void OnEnable()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        bl_PhotonCallbacks.Instance.OnMasterClientSwitchedEvent += (OnMasterClientSwitched);
        bl_EventHandler.LocalPlayerSpawnEvent += OnPhotonInstantiate;
    }

    private void OnDisable()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        if (bl_PhotonCallbacks.Instance != null)
        {
            bl_PhotonCallbacks.Instance.OnMasterClientSwitchedEvent -= (OnMasterClientSwitched);
        }
        bl_EventHandler.LocalPlayerSpawnEvent -= OnPhotonInstantiate;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            //Network player, receive data
            originPos = (Vector3)stream.ReceiveNext();
            originRot = (Quaternion)stream.ReceiveNext();

            ReceiveData = true;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (view == null || view.IsMine == true || PhotonNetwork.IsConnected == false || !ReceiveData)
        {
            return;
        }

        m_Transform.position = Vector3.Lerp(m_Transform.position, originPos, Time.deltaTime * LerpMovement);
        m_Transform.rotation = Quaternion.Lerp(m_Transform.rotation, originRot, Time.deltaTime * LerpMovement);
    }

    //when MasterClient instantiate set it as owner of this view
    public void OnPhotonInstantiate(GameObject player)
    {
        if (player.GetComponent<PhotonView>().Owner.IsMasterClient)
        {
            view.RequestOwnership();
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        if (TakeMaster && newMasterClient == PhotonNetwork.LocalPlayer)
        {
            view.RequestOwnership();
        }
    }

    private Transform _Transform = null;
    private Transform m_Transform
    {
        get
        {
            if (_Transform == null)
            {
                _Transform = this.GetComponent<Transform>();
            }
            return _Transform;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (photonView == null) return;
        if (!photonView.ObservedComponents.Contains(this))
        {
            photonView.ObservedComponents.Add(this);
        }
    }
#endif

    private PhotonView _view = null;
    private PhotonView view
    {
        get
        {
            if (_view == null)
            {
                if (this.GetComponent<PhotonView>() == null)
                {
                    _view = this.gameObject.AddComponent<PhotonView>();
                }
                else { _view = this.GetComponent<PhotonView>(); }
            }
            return _view;
        }
    }
}