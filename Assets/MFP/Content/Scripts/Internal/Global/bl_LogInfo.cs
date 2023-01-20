using UnityEngine;

public class bl_LogInfo
{

    public string m_Sender = "None";
    public string m_Message = "";
    public Color m_Color = new Color(1, 1, 1, 0.9f);

    public bool isLocalMessage = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_message"></param>
    public bl_LogInfo(string _message, bool isLocal = false)
    {
        m_Sender = "[Server]";
        m_Message = _message;
        isLocalMessage = isLocal;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_message"></param>
    /// <param name="_color"></param>
    /// <param name="isLocal"></param>
    public bl_LogInfo(string _message,Color _color, bool isLocal = false)
    {
        m_Sender = "[Server]";
        m_Message = _message;
        m_Color = _color;
        isLocalMessage = isLocal;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <param name="m"></param>
    public bl_LogInfo(string sender, string msn, bool isLocal = false)
    {
        m_Sender = sender;
        m_Message = msn;
        isLocalMessage = isLocal;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <param name="m"></param>
    public bl_LogInfo(string sender, string msn, Color c, bool isLocal = false)
    {
        m_Sender = sender;
        m_Message = msn;
        m_Color = c;
        isLocalMessage = isLocal;
    }
}