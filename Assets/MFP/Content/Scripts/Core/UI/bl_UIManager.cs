using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class bl_UIManager : MonoBehaviour
{
    public Canvas WorldCanvas;
    public Image BlackBackground = null;
    [SerializeField] private Text KeyInputText = null;
    public GameObject LogUIPrefab = null;
    public Transform LogWindowPanel = null;
    public Text ChatText;
    public GameObject StartWindow = null;
    public Text TimeText;
    public GameObject TimeUI;
    public GameObject PauseMenuUI;
    public GameObject PingMsnUI;
    public Text FinalText;
    public GameObject FinalUI;
    public Transform PlayerListPanel = null;
    public bl_PlayerListManager m_ListManager = null;
    public Image DragHandImage;
    public Sprite TakeIcon;
    public Sprite HoldIcon;

    private bl_GameController GameManager;

    private void Awake()
    {
        GameManager = bl_GameController.Instance;
    }

    /// <summary>
    /// When Player Die Destroy text
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.LocalPlayerSpawnEvent -= OnLocalSpawn;
    }
    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_EventHandler.LocalPlayerSpawnEvent += OnLocalSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalSpawn(GameObject player)
    {
        WorldCanvas.worldCamera = player.GetComponent<bl_PlayerPhoton>().PlayerCamera;
    }

    public void ShowInputText(bool show, string text = "")
    {
        KeyInputText.text = text;
        KeyInputText.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerator FadeIn(float t)
    {
        if (BlackBackground == null)
            yield return null;

        BlackBackground.gameObject.SetActive(true);
        Color c = BlackBackground.color;
        while (t > 0.0f)
        {
            t -= Time.deltaTime;
            c.a = t;
            BlackBackground.color = c;
            yield return null;
        }
        BlackBackground.gameObject.SetActive(false);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public IEnumerator FadeOut(float t)
    {
        if (BlackBackground == null)
            yield return null;
        BlackBackground.gameObject.SetActive(true);
        Color c = BlackBackground.color;
        while (c.a < t)
        {
            c.a += Time.deltaTime;
            BlackBackground.color = c;
            yield return null;
        }
    }

    public void SetHandIcon(int state)
    {
        if (state == 0)//hide
        {
            DragHandImage.gameObject.SetActive(false);
        }
        else if (state == 1)//show take icon
        {
            DragHandImage.sprite = TakeIcon;
            DragHandImage.gameObject.SetActive(true);
        }
        else if (state == 2)//show hold icon
        {
            DragHandImage.sprite = HoldIcon;
            DragHandImage.gameObject.SetActive(true);
        }
    }

    public void Respawn()
    {
        PauseMenuUI.SetActive(false);
        GameManager.SpawnPlayer();
    }

    public void Resume()
    {
        bl_RoomController.Instance.PauseEvent();
    }

    public void LeaveRoom()
    {
        bl_RoomController.Instance.LeaveRoom();
    }

    private static bl_UIManager _instance;
    public static bl_UIManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_UIManager>(); }
            return _instance;
        }
    }
}