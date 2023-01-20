using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class bl_LogText : bl_PhotonHelper {

    public Text SenderText = null;
    public Text MsnText = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_info"></param>
    public void GetInfo(bl_LogInfo _info,float fade = 0f)
    {
        SenderText.text = "[" + _info.m_Sender + "]";
        SenderText.color = _info.m_Color;
        MsnText.text = _info.m_Message;
        //If use fade start corrutine
        if (fade > 0)
        {
            StartCoroutine(Fade(fade));
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    IEnumerator Fade(float t)
    {
        yield return new WaitForSeconds(t);

        float a = this.GetComponent<CanvasGroup>().alpha;
        while (a > 0)
        {
            a -= Time.deltaTime;
            this.GetComponent<CanvasGroup>().alpha = a;
            yield return null;
        }
        //When fade is completed
        GameManager().GetComponent<bl_LogTextManager>().UpdateMaxList(this.gameObject);
    }
}