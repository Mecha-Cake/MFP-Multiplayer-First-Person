using UnityEngine;
using System.Collections.Generic;

public class bl_GameOptions : MonoBehaviour {

    public bool isRoom = false;
    public bool ApplyResolutionInStart = false;
    [Space(7)]
    private int CurrentQuality = 0;
    private int CurrentRS = 0;
    private float volume = 1.0f;
    private FullScreenMode currentFullscreenMode = FullScreenMode.FullScreenWindow;
    private ResolutionData[] resolutions;

    public const string QUALITY = "MFPQuality";
    public const string RESOLUTION = "MFPResolution";
    public const string VOLUME = "MFPVolume";
    public const string FULLSCREEN_MODE = "MFPFullscreenMode";

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        var r = Screen.resolutions;
        var list = new List<ResolutionData>();
        for (int i = 0; i < r.Length; i++)
        {
            var cr = r[i];
            if (!list.Exists(x => x.resolution.width == cr.width) && !list.Exists(x => x.resolution.height == cr.height))
            {
                list.Add(new ResolutionData()
                {
                    Index = i,
                    resolution = cr
                });
            }
        }
        resolutions = list.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        LoadOptions();
    }

    /// <summary>
    /// 
    /// </summary>
    void LoadOptions()
    {
        CurrentQuality = PlayerPrefs.GetInt(QUALITY, 3);
        CurrentRS = PlayerPrefs.GetInt(RESOLUTION, 0);
        currentFullscreenMode = (FullScreenMode)PlayerPrefs.GetInt(FULLSCREEN_MODE, 1);
        if (bl_UIManager.Instance == null) return;

        if (bl_SettingsUI.Instance != null)
        {
            bl_SettingsUI.Instance.QualityText.text = QualitySettings.names[CurrentQuality];
            bl_SettingsUI.Instance.ResolutionText.text = Screen.resolutions[CurrentRS].width + " X " + Screen.resolutions[CurrentRS].height;
            bl_SettingsUI.Instance.fullscreenText.text = currentFullscreenMode.ToString().ToUpper();
        }

        if (ApplyResolutionInStart)
        {
            Screen.SetResolution(Screen.resolutions[CurrentRS].width, Screen.resolutions[CurrentRS].height, false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="forward"></param>
    public void ChangeQuality(bool forward)
    {
        if (forward)
        {
            CurrentQuality = (CurrentQuality + 1) % QualitySettings.names.Length;
        }
        else
        {
            if (CurrentQuality != 0)
            {
                CurrentQuality = (CurrentQuality - 1) % QualitySettings.names.Length;
            }
            else
            {
                CurrentQuality = (QualitySettings.names.Length - 1);
            }
        }
        bl_SettingsUI.Instance.QualityText.text = QualitySettings.names[CurrentQuality];
        QualitySettings.SetQualityLevel(CurrentQuality);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="forward"></param>
    public void ChangeFullscreenMode(bool forward)
    {
        int index = (int)currentFullscreenMode;
        if (forward) index = (index + 1) % 3;
        else
        {
            if (index > 0) index--; else index = 3;
        }
        currentFullscreenMode = (FullScreenMode)index;
        bl_SettingsUI.Instance.fullscreenText.text = currentFullscreenMode.ToString().ToUpper();
    }

    /// <summary>
    /// Change resolution of screen
    /// NOTE: this work only in build game, this not work in
    /// Unity Editor.
    /// </summary>
    /// <param name="b"></param>
    public void Resolution(bool b)
    {
        CurrentRS = (b) ? (CurrentRS + 1) % resolutions.Length : (CurrentRS != 0) ? (CurrentRS - 1) % resolutions.Length : CurrentRS = (resolutions.Length - 1);
        bl_SettingsUI.Instance.ResolutionText.text = resolutions[CurrentRS].resolution.width + " X " + resolutions[CurrentRS].resolution.height;

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="v"></param>
    public void Volumen(float v)
    {
        volume = v;
        //Enabled this line for Apply in runTime
        if (isRoom)
        {
            AudioListener.volume = volume;
        }
        if (bl_SettingsUI.Instance.VolumenText != null)
        {
            bl_SettingsUI.Instance.VolumenText.text = (volume * 100).ToString("00") + "%";
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void Apply()
    {
        PlayerPrefs.SetInt(RESOLUTION, CurrentRS);
        PlayerPrefs.SetInt(QUALITY, CurrentQuality);
        PlayerPrefs.SetFloat(VOLUME, volume);

        var res = resolutions[CurrentRS].resolution;
        Screen.SetResolution(res.width, res.height, currentFullscreenMode);
    }

    [SerializeField]
    public class ResolutionData
    {
        public int Index;
        public Resolution resolution;
    }
}