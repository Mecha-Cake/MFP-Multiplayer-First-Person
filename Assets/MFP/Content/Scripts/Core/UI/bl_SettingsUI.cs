using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_SettingsUI : MonoBehaviour
{
    public Text QualityText = null;
    public Text ResolutionText = null;
    public Text fullscreenText;
    public Text VolumenText = null;

    private static bl_SettingsUI _instance;
    public static bl_SettingsUI Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_SettingsUI>(); }
            if(_instance == null && bl_UIManager.Instance != null)
            {
                _instance = bl_UIManager.Instance.transform.GetComponentInChildren<bl_SettingsUI>(true);
            }
            return _instance;
        }
    }
}