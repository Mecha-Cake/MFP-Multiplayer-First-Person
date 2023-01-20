using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFP.Runtime;

public class bl_GameData : ScriptableObject
{
    [Header("Global")]
    public int LocalPlayerModel = 0;
    public bool offlineMode = false;
    public PreSceneStartRule preSceneStartRule = PreSceneStartRule.OnceAllReady;

    [Header("References")]
    public GameObject PlayerPrefab;

    private bool Initializated = false;
    
    public void Init()
    {
        if (Initializated) return;

        bl_GameData.Instance.LocalPlayerModel = PlayerPrefs.GetInt(PropertiesKeys.LocalCharacter, 0);
        Initializated = true;
    }

    private static bl_GameData m_Data;
    public static bl_GameData Instance
    {
        get
        {
            if (m_Data == null)
            {
                m_Data = Resources.Load("GameData", typeof(bl_GameData)) as bl_GameData;
            }
            return m_Data;
        }
    }
}