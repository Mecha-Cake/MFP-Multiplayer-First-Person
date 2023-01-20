using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class bl_PhotonEvent : bl_PhotonHelper, IEventSystemHandler
{
    /// <summary>
    /// When event is called.
    /// </summary>
    public Trigger m_Trigger = Trigger.OnEnter;
    /// <summary>
    /// Thats target will be receive the called.
    /// </summary>
    public RpcTarget m_Target = RpcTarget.All;
    /// <summary>
    /// events called when is day
    /// </summary>
    [System.Serializable]
    public class OnDay : UnityEvent { }
    // Event delegates 
    [SerializeField]
    private OnDay m_Event = new OnDay();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    void OnTriggerEnter(Collider c)
    {
        if (m_Trigger != Trigger.OnEnter)
            return;

        if (c.tag == bl_PlayerPhoton.PlayerTag)
        {
            OnEvent();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnEvent(bool isLocal = false)
    {
        if (!this.enabled)
            return;

        if (isLocal || PhotonNetwork.OfflineMode)
        {
            m_Event.Invoke();
        }
        else
        {
            //Send RPC to master to invoke 
            photonView.RPC("RpcInvokeEvent", m_Target);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [PunRPC]
    void RpcInvokeEvent()
    {
        if (!this.enabled)
            return;

        m_Event.Invoke();
    }

    [System.Serializable]
    public enum Trigger
    {
        OnEnter,
        OnExit,
        OnMouseDown,
        OnMouseUp,
        OnActive,
        OnDesactive,
    }
}