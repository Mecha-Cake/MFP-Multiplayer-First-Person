using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class bl_ChatRoom : bl_PhotonHelper {

    /// <summary>
    /// This style use guiSkin because inputField Ugui cause
    /// interference with other gui in playtime
    /// </summary>
    public GUISkin m_Skin;
    [Space(5)]

    public bool IsVisible = true;
    public bool WithSound = true;
    public int MaxMsn = 7;
    [Space(5)]
    public AudioClip MsnSound;

    [HideInInspector] public List<string> messages = new List<string>();
    public static readonly string ChatRPC = "Chat";
    private float m_alpha = 2f;
    private bool isChat = false;
    private string inputLine = "";
    private Text ChatText;

    private void Awake()
    {
        ChatText = bl_UIManager.Instance.ChatText;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnGUI()
    {
        if (ChatText == null)
            return;

        if (m_alpha > 0.0f && !isChat)
        {
            m_alpha -= Time.deltaTime / 2;
        }
        else if (isChat)
        {
            m_alpha = 10;
        }

        GUI.skin = m_Skin;     
        if (!this.IsVisible || !PhotonNetwork.InRoom)
        {
            return;
        }

        if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
        {
            if (!string.IsNullOrEmpty(this.inputLine) && isChat && bl_CoopUtils.GetCursorState)
            {
                this.photonView.RPC("Chat", RpcTarget.All, this.inputLine);
                this.inputLine = "";
                GUI.FocusControl("");
                isChat = false;
                return; // printing the now modified list would result in an error. to avoid this, we just skip this single frame
            }
            else if (!isChat && bl_CoopUtils.GetCursorState)
            {
                GUI.FocusControl("ChatInput");
                isChat = true;
            }
            else
            {
                if (isChat)
                {
                    Closet();
                }
            }
        }
        if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t'))
        {
            Event.current.Use();
        }
        GUI.color = new Color(1, 1, 1, m_alpha);
        GUI.SetNextControlName("");
        GUILayout.BeginArea(new Rect(Screen.width / 2 - 150, Screen.height - 35, 300, 50));
        GUILayout.BeginHorizontal();
        GUI.SetNextControlName("ChatInput");
        inputLine = GUILayout.TextField(inputLine);
        if (GUILayout.Button("Send", "box", GUILayout.ExpandWidth(false)))
        {
            this.photonView.RPC("Chat", RpcTarget.All, this.inputLine);
            this.inputLine = "";
            GUI.FocusControl("");
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

    }
    /// <summary>
    /// 
    /// </summary>
    void Closet()
    {
        isChat = false;
        GUI.FocusControl("");
    }
    /// <summary>
    /// Sync Method
    /// </summary>
    /// <param name="newLine"></param>
    /// <param name="mi"></param>
    [PunRPC]
    public void Chat(string newLine, PhotonMessageInfo mi)
    {
        m_alpha = 7;
        string senderName = "anonymous";

        if (mi.Sender != null)
        {
            if (!string.IsNullOrEmpty(mi.Sender.NickName))
            {
                senderName = mi.Sender.NickName;
            }
            else
            {
                senderName = "player " + mi.Sender.UserId;
            }
        }

        this.messages.Add("<color=#478FF5>[" + senderName + "]</color>: " + newLine);
        if (MsnSound != null && WithSound)
        {
            GetComponent<AudioSource>().PlayOneShot(MsnSound);
        }
        if (messages.Count > MaxMsn)
        {
            messages.RemoveAt(0);
        }

        ChatText.text = "";
        foreach (string m in messages)
        {
            ChatText.text += m + "\n";
        }
    }
    /// <summary>
    /// Local Method
    /// </summary>
    /// <param name="newLine"></param>
    public void AddLine(string newLine)
    {
        m_alpha = 7;
        this.messages.Add(newLine);
        if (messages.Count > MaxMsn)
        {
            messages.RemoveAt(0);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void Refresh()
    {
        ChatText.text = "";
        foreach (string m in messages)
            ChatText.text += m + "\n";
    }
}