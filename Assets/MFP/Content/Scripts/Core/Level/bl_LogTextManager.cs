using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class bl_LogTextManager : bl_PhotonHelper {

    [Separator("Setting")]
    [Range(1,12)]
    public int MaxMsnInList = 5;
    public bool FadeMsnInTime = true;
    public float FadeTime = 7f;

    private List<GameObject> cacheLogs = new List<GameObject>();

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        bl_EventHandler.OnLogWindow += this.OnLogMsnEvent;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.OnLogWindow -= this.OnLogMsnEvent;
    }

    /// <summary>
    /// Receive event info
    /// </summary>
    /// <param name="_info"></param>
    void OnLogMsnEvent(bl_LogInfo _info)
    {
        //If message local, just call function to add.
        if (_info.isLocalMessage || PhotonNetwork.OfflineMode)
        {
            AddNewLog(_info);
        }
        else
        {
            //is sync message, add all info in a string array to serialized and send in a RPC call.
            string[] texts = new string[3];
            texts[0] = _info.m_Sender;
            texts[1] = _info.m_Message;
            //Convert color to string for serialized.
            texts[2] = bl_ColorHelper.ColorToHex(_info.m_Color);

            photonView.RPC("RpcSyncMessage", RpcTarget.All, texts);
        }
    }
    
    [PunRPC]
    void RpcSyncMessage(string[] texts)
    {
        //Convert string / Hex color to color RGB.
        Color _color = bl_ColorHelper.HexToColor(texts[2]);
        //Create again the bl_LogInfo to create a new UI in Log window.
        bl_LogInfo _info = new bl_LogInfo(texts[0], texts[1],_color);
        AddNewLog(_info);
    }

    /// <summary>
    /// Add a new message with info in Log Window list.
    /// </summary>
    public void AddNewLog(bl_LogInfo _info)
    {
        GameObject log = (GameObject)Instantiate(bl_UIManager.Instance.LogUIPrefab);
        float _isFade = (FadeMsnInTime) ? FadeTime : 0;
        log.GetComponent<bl_LogText>().GetInfo(_info,_isFade);
        cacheLogs.Add(log);
        UpdateMaxList();

        log.transform.SetParent(bl_UIManager.Instance.LogWindowPanel, false);
    }

    /// <summary>
    /// Remove old entrys when is more than max msn.
    /// </summary>
    public void UpdateMaxList(GameObject reference = null)
    {
        //If not get a reference, just remove the first msn (most old).
        if (reference == null)
        {
            if (cacheLogs.Count > MaxMsnInList)
            {
                Destroy(cacheLogs[0]);
                cacheLogs.RemoveAt(0);
            }
        }
        else
        {
            //Verify it the reference is in the list.
            if (cacheLogs.Contains(reference))
            {
                cacheLogs.Remove(reference);
            }
            else
            {
                Debug.LogWarning("This gameobject: " + reference.name + " doesnt exits in list!");
                return;
            }
            Destroy(reference);
        }
    }
}