using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_PlayerModel : MonoBehaviour
{
    public string ModelName = "Model Name";
    public Animator m_Animator;
    public Sprite Preview;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(m_Animator == null && GetComponent<Animator>() != null)
        {
            m_Animator = GetComponent<Animator>();
        }
    }
#endif
}