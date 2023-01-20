using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class bl_PlayerListManager : MonoBehaviour {

    [Separator("Animation Settings")]
    public Animation m_Animation = null;
    public string AnimationName = "";
    [Range(0.1f,5f)]
    public float ShowSpeed = 1.3f;
    [Separator("References")]
    public GameObject JoinButton = null;
    public GameObject RespawnButton = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="forward"></param>
    public void ShowPlayerList(bool forward)
    {
        if (m_Animation == null)
        {
            Debug.Log("Player List is not assigned in inspector!");
        }
        if (!forward)
        {
            gameObject.SetActive(true);
            m_Animation[AnimationName].speed = 1.3f;
            m_Animation.CrossFade(AnimationName, 0.25f);
        }
        else
        {
            if (m_Animation[AnimationName].time == 0.0f)
            {
                m_Animation[AnimationName].time = m_Animation[AnimationName].length;
            }
            m_Animation[AnimationName].speed = -1.0f;
            m_Animation.CrossFade(AnimationName, 0.25f);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="FirstTime"></param>
    public void SpawnEvent(bool FirstTime)
    {
        if (FirstTime)
        {
            JoinButton.SetActive(false);
            RespawnButton.SetActive(true);
        }
        else
        {

        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="hide">1 = true</param>
    public void HideButton(int hide)
    {
        if (hide == 1)
        {
            RespawnButton.SetActive(false);
        }
        else
        {
            RespawnButton.SetActive(true);
        }
    }
}