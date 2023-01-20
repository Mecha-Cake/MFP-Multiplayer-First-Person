
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class bl_PhotonHelper : MonoBehaviourPun {


    /// <summary>
    /// Find a player gameobject by the viewID 
    /// </summary>
    /// <param name="view"></param>
    /// <returns></returns>
    public GameObject FindPlayerRoot(int view)
    {
        PhotonView m_view = PhotonView.Find(view);

        if (m_view != null)
        {
            return m_view.gameObject;
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    ///  get a photonView by the viewID
    /// </summary>
    /// <param name="view"></param>
    /// <returns></returns>
    public PhotonView FindPlayerView(int view)
    {
        PhotonView m_view = PhotonView.Find(view);

        if (m_view != null)
        {
            return m_view;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public Transform Root
    {
        get
        {
            return GetComponent<Transform>().root;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public Transform Parent
    {
        get
        {
            return GetComponent<Transform>().parent;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private AudioSource _source = null;
    public AudioSource Source
    {
        get
        {
            if (_source == null)
            {
                _source = this.GetComponent<AudioSource>();
            }
            return _source;
        }
    }
    /// <summary>
    /// True if the PhotonView is "mine" and can be controlled by this client.
    /// </summary>
    /// <remarks>
    /// PUN has an ownership concept that defines who can control and destroy each PhotonView.
    /// True in case the owner matches the local PhotonPlayer.
    /// True if this is a scene photonview on the Master client.
    /// </remarks>
    public bool isMine
    {
        get
        {
            if (photonView == null) return true;

            return (photonView.IsMine);
        }
    }
    /// <summary>
    /// Get Photon.connect
    /// </summary>
    public bool isConnected
    {
        get
        {
            return PhotonNetwork.IsConnected;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public GameObject FindPhotonPlayer(Player p)
    {
        GameObject player = GameObject.Find(p.NickName);
        if (player == null)
        {
            return null;
        }
        return player;
    }
    /// <summary>
    /// 
    /// </summary>
    public string LocalName
    {
        get
        {
            if (PhotonNetwork.LocalPlayer != null && isConnected)
            {
                string n = PhotonNetwork.LocalPlayer.NickName;
                return n;
            }
            else
            {
                return "None";
            }
        }
    }

    /// <summary>
    /// Get All Player in Room
    /// </summary>
    public List<Player> AllPlayerList
    {
        get
        {
            List<Player> p = new List<Player>();

            foreach (Player pp in PhotonNetwork.PlayerList)
            {
                p.Add(pp);
            }
            return p;
        }
    }
    /// <summary>
    /// Create Photon Events
    /// </summary>
    /// <param name="id"></param>
    /// <param name="content"></param>
    /// <param name="b"></param>
    /// <param name="options"></param>
    public void NewPhotonEvent(int id, object content, bool b, RaiseEventOptions options)
    {
        if (!isConnected)
            return;

        byte mByte = (byte)id;
        SendOptions so = new SendOptions() { DeliveryMode = DeliveryMode.Unreliable,};
        PhotonNetwork.RaiseEvent(mByte, content, options, so);
    }
    /// <summary>
    /// 
    /// </summary>
    public bool IsMaster
    {
        get
        {
            bool b = false;
            if (PhotonNetwork.IsMasterClient)
            {
                b = true;
            }
            return b;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    public void NetworkDestroy(GameObject go)
    {
        PhotonNetwork.Destroy(go);
    }

    /// <summary>
    /// Get the lobby manager gameObject
    /// Use this only when is in lobby scene.
    /// </summary>
    private GameObject _lobbyManager = null;
    public GameObject LobbyManager()
    {
        if (_lobbyManager == null)
        {
            if (GameObject.FindWithTag("GameController") != null)
            {
                _lobbyManager = GameObject.FindWithTag("GameController");
            }
        }
        return _lobbyManager;
    }

    private GameObject _gameController = null;
    public GameObject GameManager()
    {
        if (_gameController == null)
        {
            if (GameObject.FindWithTag("GameController") != null)
            {
                _gameController = GameObject.FindWithTag("GameController");
            }
        }
        return _gameController;
    }
}