using UnityEngine;
using System;
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using Photon.Pun;
using Photon.Realtime;

public static class bl_CoopUtils
{

    public static void LoadLevel(string scene)
    {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
 SceneManager.LoadScene(scene);
#else
        Application.LoadLevel(scene);
#endif
    }

    public static void LoadLevel(int scene)
    {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
 SceneManager.LoadScene(scene);
#else
        Application.LoadLevel(scene);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string CoopColorStr(string text)
    {
        return "<color=#5F9DF5>" + text + "</color>";
    }

    /// <summary>
    /// 
    /// </summary>
    public static string GetGuid
    {
        get
        {
            string s = Guid.NewGuid().ToString();
            return s;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static bl_LobbyUI GetLobbyUI
    {
        get
        {
            bl_LobbyUI l = GameObject.FindWithTag("GameController").GetComponent<bl_LobbyUI>();

            return l;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static bl_PreScene GetPreScene
    {
        get
        {
            bl_PreScene ps = GameObject.FindWithTag("GameController").GetComponent<bl_PreScene>();
            return ps;
        }
    }
    /// <summary>
    /// Get ClampAngle
    /// </summary>
    /// <param name="ang"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float ClampAngle(float ang, float min, float max)
    {
        if (ang < -360f)
        {
            ang += 360f;
        }
        if (ang > 360f)
        {
            ang -= 360f;
        }
        return UnityEngine.Mathf.Clamp(ang, min, max);
    }
    /// <summary>
    /// obtain only the first two values
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static string GetDoubleChar(float f)
    {
        return f.ToString("00");
    }
    /// <summary>
    /// obtain only the first three values
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static string GetThreefoldChar(float f)
    {
        return f.ToString("000");
    }

    public static string GetTimeFormat(float m, float s)
    {
        return string.Format("{0:00}:{1:00}", m, s);
    }

    public static bool CursorAlwaysActive = false;

    /// <summary>
    /// Helper for Cursor locked in Unity 5
    /// </summary>
    /// <param name="mLock">cursor state</param>
    public static void LockCursor(bool mLock)
    {
#if UNITY_5_3_OR_NEWER
        if (mLock == true)
        {
            if (CursorAlwaysActive) return;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
#else
        Screen.lockCursor = mLock;
#endif
    }
    /// <summary>
    /// 
    /// </summary>
    public static bool GetCursorState
    {
        get
        {
#if UNITY_5_3_OR_NEWER
            if (Cursor.visible && Cursor.lockState != CursorLockMode.Locked)
            {
                return false;
            }
            else
            {
                return true;
            }
#else
            return Screen.lockCursor;
#endif
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static float HorizontalAngle(Vector3 direction)
    {
        return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string IntToRegionCode(int id)
    {
        switch (id)
        {
            case 0:
                return "asia";
            case 1:
                return "au";
            case 2:
                return "eu";
            case 3:
                return "jp";
            case 4:
                return "us";
            case 5:
                return "cae";
            case 6:
                return "sa";
            case 7:
                return "usw";
            case 8:
                return "in";
            case 9:
                return "kr";
            case 10:
                return "za";
            case 11:
                return "ru";
            case 12:
                return "rue";
        }

        return "none";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static int RegionCodeToInt(string code)
    {
        switch (code)
        {
            case "asia":
                return 0;
            case "au":
                return 1;
            case "wu":
                return 2;
            case "jp":
                return 3;
            case "us":
                return 4;
            case "cae":
                return 5;
            case "sa":
                return 6;
            case "usw":
                return 7;
            case "in":
                return 8;
            case "kr":
                return 9;
            case "za":
                return 10;
            case "ru":
                return 11;
            case "rue":
                return 12;

        }
        return 5; // equal a none
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public static string RegionToString(string code)
    {
        switch (code)
        {
            case "asia":
                return "ASIA";
            case "au":
                return "AUSTRALIA";
            case "eu":
                return "EUROPE";
            case "jp":
                return "JAPAN";
            case "us":
                return "USA";
            case "sa":
                return "SOUTH AMERICA";
            case "cae":
                return "CANADA";
            case "usw":
                return "USA WEST";
            case "in":
                return "INDIA";
            case "kr":
                return "SOUTH KOREA";
        }
        return code; // equal a none
    }
}