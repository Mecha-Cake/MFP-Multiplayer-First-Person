using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class bl_PlayerUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup DamageAlpha = null;
    [SerializeField] private Text HealthText = null;
    public GameObject DeathUI;
    [SerializeField] private Text RespawnCountText = null;

    public void OnDamage()
    {
        StopCoroutine("DamageEffect");
        StartCoroutine("DamageEffect");
    }

    public void SetHealth(int health)
    {
        HealthText.text = string.Format("{0}<size=14> HEALTH</size>", health);
    }

    public void UpdateRespawnText(float t)
    {
        RespawnCountText.text = t.ToString("0.0");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DamageEffect()
    {
        while(DamageAlpha.alpha < 1)
        {
            DamageAlpha.alpha += Time.deltaTime * 7;
            yield return null;
        }
        yield return new WaitForSeconds(2);
        while (DamageAlpha.alpha > 0)
        {
            DamageAlpha.alpha -= Time.deltaTime;
            yield return null;
        }
    }
}