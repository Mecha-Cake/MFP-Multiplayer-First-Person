using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
public class bl_PlayerPhoton : bl_PhotonHelper {

    [Header("when the player is our disable these scripts")]
    public List<MonoBehaviour> Local_DisabledScripts = new List<MonoBehaviour>();
    [Header("when the player is Not our disable these scripts")]
    public List<MonoBehaviour> Remote_DisabledScripts = new List<MonoBehaviour>();
    public GameObject LocalObject;
    public GameObject RemoteObject;
    public Camera PlayerCamera;

    public const string PlayerTag = "Player";
    public const string RemoteTag = "NetworkPlayer";

    void Awake()
    {
#if UNITY_EDITOR
        bool found = false;
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i].Contains(RemoteTag))
            {
                found = true;
            }
        }
        if (!found) { Debug.LogError("Please add the tag: <color=#4ECDC4>" + RemoteTag + "</color> in the tag list"); }
#endif
        if (isMine)
        {
            LocalPlayer();
        }
        else
        {
            NetworkPlayer();
        }
    }
    /// <summary>
    /// We call this function only if we are Remote player
    /// </summary>
    public void NetworkPlayer()
    {
        foreach (MonoBehaviour script in Remote_DisabledScripts)
        {
            Destroy(script);
        }
        LocalObject.SetActive(false);
        this.gameObject.tag = RemoteTag;

    }
    /// <summary>
    /// We call this function only if we are Local player
    /// </summary>
    public void LocalPlayer()
    {
        gameObject.name = PhotonNetwork.NickName;
        foreach (MonoBehaviour script in Local_DisabledScripts)
        {
            Destroy(script);
        }
        RemoteObject.SetActive(false);
        this.gameObject.tag = PlayerTag;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool isLocalPlayer
    {
        get
        {
            return isMine;
        }
    }
}