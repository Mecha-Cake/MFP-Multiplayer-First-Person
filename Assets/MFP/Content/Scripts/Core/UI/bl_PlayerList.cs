using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class bl_PlayerList : MonoBehaviour {

    public Text PlayerName;
    public Text Ping;
    public GameObject KickButton = null;

    private Player cachePlayer = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="PName"></param>
    /// <param name="ping"></param>
    public void GetInfo(string PName, string ping, Player player)
    {
        PlayerName.text = PName;
        Ping.text = ping;
        //Appear kick button only to the master client.
        bool _active = (PhotonNetwork.IsMasterClient && player != PhotonNetwork.LocalPlayer ) ? true : false;
        KickButton.SetActive(_active);
        cachePlayer = player;
    }
    /// <summary>
    /// 
    /// </summary>
    public void KickPlayer()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (cachePlayer != null)
        {
            PhotonNetwork.CloseConnection(cachePlayer);
        }
        else
        {
            Debug.LogWarning("This Player doesn't exit!");
        }
    }
}