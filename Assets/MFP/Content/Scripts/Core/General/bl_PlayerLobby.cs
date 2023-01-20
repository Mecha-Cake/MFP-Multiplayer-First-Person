using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class bl_PlayerLobby : MonoBehaviour {

    public Player m_Player;
    public Text PlayerNameText = null;
    public Text ReadyText = null;
    public GameObject isMaster = null;
    [SerializeField]private GameObject OnMaster = null;
    [HideInInspector]
    public string PlayerName = "";
    [HideInInspector]
    public bool Ready = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="n"></param>
    /// <param name="b"></param>
    public void GetInfo(Player player, bool b = false)
    {
        string n = player.NickName;
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.Log("Not room for player list");
            Destroy(this.gameObject);
        }

        PlayerName = n;
        PlayerNameText.text = PlayerName;
        ReadyText.text = (b) ? "Ready" : "Not Ready";

        bool m = (player.IsMasterClient) ? true : false;
        isMaster.SetActive(m);
        OnMaster.SetActive(m);
    }

    public void SetState(bool state)
    {
        ReadyText.text = (state) ? "Ready" : "Not Ready";
    }

    public void Kick()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Only Masterclient can kick players");
            return;
        }
        PhotonNetwork.CloseConnection(m_Player);
        Destroy(gameObject);
    }
}