using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class bl_RoomInfo : MonoBehaviour  {

    public Text RoomNameUI;
    public Text MapNameUI;
    public Text MaxPlayerUI;
    public Text TypeUI;
    public Text ButtonText;
    public Image StatusImg;
    public GameObject lockedUI;
    [Space(7)]
    public Color AvailableColor;
    public Color FullColor;

    [HideInInspector]
    public RoomInfo m_Room;

    /// <summary>
    /// 
    /// </summary>
    public void GetInfo(RoomInfo r)
    {
        m_Room = r;
        RoomNameUI.text = r.Name;      
        MapNameUI.text = r.RoomScene();
        MaxPlayerUI.text = r.PlayerCount + " / " + r.MaxPlayers;
        lockedUI.SetActive(!string.IsNullOrEmpty(RoomPassword));

        bool b = r.GetRoomState();
        TypeUI.text = (b == true) ? "Playing" : "Waiting";

        if (r.PlayerCount >= r.MaxPlayers)
        {
            ButtonText.text = "Full";
            StatusImg.color = FullColor;
        }
        else
        {
            ButtonText.text = "Join";
            StatusImg.color = AvailableColor;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void EnterRoom()
    {
        if (!string.IsNullOrEmpty(RoomPassword))
        {
            bl_Lobby.Instance.WaitForRoomPassword(this);
            return;
        }
        if (m_Room.PlayerCount < m_Room.MaxPlayers)
        {
            PhotonNetwork.JoinRoom(m_Room.Name);
            bl_CoopUtils.GetLobbyUI.ChangeWindow(5);
        }
        else
        {
            Debug.Log("This Room is Full");
        }
    }

    public string RoomPassword
    {
        get
        {
            string p = "";
            if (m_Room.CustomProperties.ContainsKey(PropertiesKeys.RoomPassword))
            {
                p = (string)m_Room.CustomProperties[PropertiesKeys.RoomPassword];
            }
            return p;
        }
    }
}