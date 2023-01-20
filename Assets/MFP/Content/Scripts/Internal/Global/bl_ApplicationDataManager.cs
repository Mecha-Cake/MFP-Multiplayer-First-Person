using UnityEngine;
using System.Collections;

public static class bl_ApplicationDataManager
{
    /// <summary>
    /// 
    /// </summary>
    public static string FrameRate
    {
        get
        {
            int frames = Mathf.Max(Mathf.RoundToInt(Time.smoothDeltaTime * 1000f), 1);
            return string.Format("{0} ({1}ms)", 1000 / frames, frames);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool IsDesktop
    {
        get
        {
            return ((Application.platform == RuntimePlatform.OSXPlayer) || (Application.platform == RuntimePlatform.WindowsPlayer));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool IsMobile
    {
        get
        {
            return ((Application.platform == RuntimePlatform.IPhonePlayer) || (Application.platform == RuntimePlatform.Android));
        }
    }
}