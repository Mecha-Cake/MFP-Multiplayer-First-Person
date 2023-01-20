/////////////////////////////////////////////////////////////////////////////////
///////////////////////////////bl_RoundTime.cs///////////////////////////////////
///////////////Use this to manage time in rooms//////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
//////////////////////////////Lovatto Studio/////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class bl_RoundTime : MonoBehaviour {

    /// <summary>
    /// mode of the round room
    /// </summary>
    public RoundStyle m_RoundStyle;
    /// <summary>
    /// expected duration in round (automatically obtained)
    /// </summary>
    public int RoundDuration;
    [HideInInspector]
    public float CurrentTime;

    //private
    private const string StartTimeKey = "CurrentTimeRoom";       // the name of our "start time" custom property.
    private float m_Reference;
    private int m_countdown = 10;
    public static bool isFinish = false;
    private bl_GameController GameController;
    private bool Unlimited = false;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        GameController = FindObjectOfType<bl_GameController>();
        //When you start from room scene, return to lobby for connect to server first.
        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null)
        {
            bl_CoopUtils.LoadLevel(GameController.ReturnScene);
            return;
        }
        isFinish = false;
        if ((string)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.RoomRoundKey] == "1")
        {
            m_RoundStyle = RoundStyle.Rounds;
        }
        else
        {
            m_RoundStyle = RoundStyle.OneMacht;
        }
        GetTime();
    }

    void OnDisable()
    {
        isFinish = false;
    }
    /// <summary>
    /// get the current time and verify if it is correct
    /// </summary>
    void GetTime()
    {
        RoundDuration = (int)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.TimeRoomKey];
        if(RoundDuration <= 0)
        {
            Unlimited = true;
            bl_UIManager.Instance.TimeText.text = "∞";
            bl_UIManager.Instance.TimeText.fontSize = 18;
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            m_Reference = (float)PhotonNetwork.Time;

            Hashtable startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_Reference);
            PhotonNetwork.CurrentRoom.SetCustomProperties(startTimeProp);
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(StartTimeKey))
            {
                m_Reference = (float)PhotonNetwork.CurrentRoom.CustomProperties[StartTimeKey];
                if (!bl_UIManager.Instance.TimeUI.activeSelf)
                {
                    bl_UIManager.Instance.TimeUI.SetActive(true);
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    void FixedUpdate()
    {
        if (Unlimited) return;

        //Calculate the time server
        float t_time = RoundDuration - ((float)PhotonNetwork.Time - m_Reference);
        if (t_time > 0)
        {
            CurrentTime = t_time;
        }
        else if (t_time <= 0.001 && GetTimeServed == true)//Round Finished
        {
            CurrentTime = 0;
            
            bl_EventHandler.OnRoundEndEvent();
            if (!isFinish)
            {
                isFinish = true;
                InvokeRepeating("countdown", 1, 1);
            }
        }
        else//even if I do not photonnetwork.time then obtained to regain time
        {
            Refresh();
        }
        TimeLogic();

    }
    /// <summary>
    /// 
    /// </summary>
    void TimeLogic()
    {
        //Convert in time format
        int normalSecons = 60;
        float remainingTime = Mathf.CeilToInt(CurrentTime);
        int m_Seconds = Mathf.FloorToInt(remainingTime % normalSecons);
        int m_Minutes = Mathf.FloorToInt((remainingTime / normalSecons) % normalSecons);
        string t_time = bl_CoopUtils.GetTimeFormat(m_Minutes, m_Seconds);

        //Update UI 
        if (bl_UIManager.Instance.TimeText != null)
        {
            bl_UIManager.Instance.TimeText.text = "<size=9>Remaining</size> \n" + t_time;
        }
        if (isFinish)
        {
            if (!bl_UIManager.Instance.FinalUI.activeSelf)
            {
                bl_UIManager.Instance.FinalUI.SetActive(true);
            }
            if (m_RoundStyle == RoundStyle.OneMacht)
            {
                bl_UIManager.Instance.FinalText.text = "Round finished, return to lobby in \n " + m_countdown;
            }
            else if (m_RoundStyle == RoundStyle.Rounds)
            {
                bl_UIManager.Instance.FinalText.text = "Round finished, restart in \n " + m_countdown;
            }
        }
    }

    /// <summary>
    /// with this fixed the problem of the time lag in the Photon
    /// </summary>
    void Refresh()
    {
        //Only masterClient can send the time.
        if (PhotonNetwork.IsMasterClient)
        {
            m_Reference = (float)PhotonNetwork.Time;

            Hashtable startTimeProp = new Hashtable();  // only use ExitGames.Client.Photon.Hashtable for Photon
            startTimeProp.Add(StartTimeKey, m_Reference);
            PhotonNetwork.CurrentRoom.SetCustomProperties(startTimeProp);
        }
        else//When is a normal player (Client)
        {
            //Receive the time reference from the server (masterClient side)
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(StartTimeKey))
            {
                m_Reference = (float)PhotonNetwork.CurrentRoom.CustomProperties[StartTimeKey];
                if (!bl_UIManager.Instance.TimeUI.activeSelf)
                {
                    bl_UIManager.Instance.TimeUI.SetActive(true);
                }
            }
        }
    }
    /// <summary>
    /// When round is finished, start this countdown for return to lobby
    /// You can change the second of countdown in the private variable "m_counddown".
    /// </summary>
    bool callFade = false;
    void countdown()
    {
        m_countdown--;

        //Do fade effect when is finished countdown.
        if (m_countdown == 1 && !callFade)
        {
            StartCoroutine(bl_UIManager.Instance.FadeOut(2));
        }

        if (m_countdown <= 0)
        {
            FinishGame();
            CancelInvoke("countdown");
            m_countdown = 10;
        }
    }
    /// <summary>
    /// When countdown is reached, this is call for determine if return
    /// to lobby or restart scene.
    /// </summary>
    void FinishGame()
    {
        bl_CoopUtils.LockCursor(false);
        if (m_RoundStyle == RoundStyle.OneMacht)
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                bl_CoopUtils.LoadLevel(GameController.ReturnScene);
            }
        }
        if (m_RoundStyle == RoundStyle.Rounds)
        {
            GetTime();
        }
    }
    /// <summary>
    /// Determines whether the time is long enough to receive the server response
    /// </summary>
    bool GetTimeServed
    {
        get
        {
            bool m_bool = false ;
            if (Time.timeSinceLevelLoad > 7)
            {
                m_bool = true;
            }
            return m_bool;
        }
    }

}