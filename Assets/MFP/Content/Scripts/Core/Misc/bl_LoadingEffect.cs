using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class bl_LoadingEffect : MonoBehaviour {

    public float m_Speed = 500;
    public  bool Loading = false;

    private Image mImage = null;
    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        mImage = this.GetComponent<Image>();
    }
    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (Loading)
        {
            this.transform.Rotate(((Vector3.forward * this.m_Speed) * Time.deltaTime), Space.World);//rotate effect
            Color alpha = mImage.color;
            if (alpha.a < 1f)
            {
                alpha.a = Mathf.Lerp(alpha.a, 1f, Time.deltaTime * 2);//fade out
            }
            mImage.color = alpha;
        }
        else
        {
            Color alpha = mImage.color;
            if (alpha.a > 0f)
            {
                alpha.a = Mathf.Lerp(alpha.a, 0f, Time.deltaTime * 2);//fade in
            }
            mImage.color = alpha;
        }
    }
}
