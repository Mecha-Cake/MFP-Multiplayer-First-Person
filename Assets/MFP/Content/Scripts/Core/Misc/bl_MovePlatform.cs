using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public class bl_MovePlatform : bl_PhotonHelper, IPunObservable
{

    [Separator("Settings")]
    public List<PositionInfo> Positions = new List<PositionInfo>();
    public bool PingPong = false;

    [Separator("Multiplayer")]
    public bool Sync = true;

    private int CurrentPosition = 0;
    private Transform m_Transform;
    private bool isForward = true;
    private Transform CurrentLocalPlayer;

    private Vector3 m_Direction;
    private Vector3 m_NetworkPosition;
    private Vector3 m_StoredPosition;
    private float m_Distance;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (Sync)
        {
            if (!view.ObservedComponents.Contains(this))
            {
                view.ObservedComponents.Add(this);
            }
        }
        m_StoredPosition = transform.position;
        m_NetworkPosition = Vector3.zero;
    }

    void OnEnable()
    {
        if (!PhotonNetwork.IsConnected)
            return;
        bl_EventHandler.PhotonInstantiateEvent += this.OnPhotonInstantiateEvent;
        bl_PhotonCallbacks.Instance.OnMasterClientSwitchedEvent += (OnMasterClientSwitched);
    }

    void OnDisable()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        bl_EventHandler.PhotonInstantiateEvent -= this.OnPhotonInstantiateEvent;
        if (bl_PhotonCallbacks.Instance != null)
        {
            bl_PhotonCallbacks.Instance.OnMasterClientSwitchedEvent -= (OnMasterClientSwitched);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        m_Transform = transform;
        //add the current position as first position in the list
        PositionInfo[] all = Positions.ToArray();
        List<PositionInfo> newall = new List<PositionInfo>();
        PositionInfo npi = new PositionInfo();
        npi.Position = m_Transform.position;
        npi.Speed = all[0].Speed;
        npi.DelayToNext = all[0].DelayToNext;

        newall.Add(npi);
        newall.AddRange(all);
        Positions = newall;

        if (Sync)
        {
            if (!isMine)
                return;
        }

        StartCoroutine(Process());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    void OnTriggerEnter(Collider c)
    {

        if(c.transform.root.GetComponent<bl_PlayerMovement>() != null)
        {
            if (c.transform.root.GetComponent<bl_PlayerMovement>().IsLocalPlayer)
            {
                 c.transform.parent = transform;
                CurrentLocalPlayer = c.transform;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    void OnTriggerExit(Collider c)
    {

        if (c.transform == CurrentLocalPlayer)
        {
            CurrentLocalPlayer.parent = null;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!Sync)
            return;

        if (stream.IsWriting)
        {

            this.m_Direction = transform.position - this.m_StoredPosition;
            this.m_StoredPosition = transform.position;

            stream.SendNext(transform.position);
            stream.SendNext(this.m_Direction);
        }
        else
        {
            this.m_NetworkPosition = (Vector3)stream.ReceiveNext();
            this.m_Direction = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            this.m_NetworkPosition += this.m_Direction * lag;

            this.m_Distance = Vector3.Distance(transform.position, this.m_NetworkPosition);

        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (!Sync)
            return;

        if (view == null || view.IsMine == true || PhotonNetwork.IsConnected == false)
        {
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, this.m_NetworkPosition, this.m_Distance * (1.0f / PhotonNetwork.SerializationRate));
    }


    //when MasterClient instantiate set it as owner of this view
    private void OnPhotonInstantiateEvent(PhotonMessageInfo info)
    {
        if (!Sync)
            return;

        if (info.Sender.IsMasterClient)
        {
            info.photonView.RequestOwnership();
        }
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!Sync)
            return;

        if (newMasterClient == PhotonNetwork.LocalPlayer)
        {
            view.RequestOwnership();
            StartCoroutine(Process(true));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = bl_ColorHelper.HexToColor("30fbd6");
        for (int i = 0; i < Positions.Count; i++)
        {
            if (i >= 1)
            {
                Gizmos.DrawLine(Positions[i - 1].Position, Positions[i].Position);
            }
            else { Gizmos.DrawLine(transform.position, Positions[i].Position); }
        }
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
        if(Positions.Count > 0)
        Gizmos.DrawWireCube(Positions[Positions.Count - 1].Position, new Vector3(1, 1, 1));
    }


    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Process(bool force = false)
    {
        while (true)
        {
            if (!force)
            {
                if (Sync) { if (!isMine) { yield break; } }
            }

            while (Vector3.Distance(m_Transform.localPosition, GetCurrentPosition.Position) > 0.02f)
            {
                m_Transform.localPosition = Vector3.MoveTowards(m_Transform.localPosition, GetCurrentPosition.Position, Time.deltaTime * GetCurrentPosition.Speed);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(GetCurrentPosition.DelayToNext);
            GoToNext();
        }
    }

    /// <summary>
    /// Select the next position
    /// </summary>
    void GoToNext()
    {
        if (PingPong)
        {
            if (isForward)
            {
                if (CurrentPosition < Positions.Count - 1)
                {
                    CurrentPosition = (CurrentPosition + 1) % Positions.Count;
                }
                else
                {
                    isForward = !isForward;
                    CurrentPosition = Positions.Count - 1;
                }
            }
            else
            {
                if(CurrentPosition > 0)
                {
                    CurrentPosition = (CurrentPosition - 1) % Positions.Count;
                }
                else
                {
                    isForward = !isForward;
                    CurrentPosition = 1;
                }
            }
        }
        else
        {
            CurrentPosition = (CurrentPosition + 1) % Positions.Count;
        }      
    }

    public PositionInfo GetCurrentPosition
    {
        get
        {
            return Positions[CurrentPosition];
        }
    }

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


    [System.Serializable]
    public class PositionInfo
    {
        public Vector3 Position;
        [Range(1,100)]public float Speed;
        [Range(0,20)]public float DelayToNext;
    }

}