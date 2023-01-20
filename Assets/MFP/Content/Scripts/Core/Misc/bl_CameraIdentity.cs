using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_CameraIdentity : MonoBehaviour
{
    private Camera m_Camera;

    private void OnEnable()
    {
        if(m_Camera == null)
        {
            m_Camera = GetComponent<Camera>();
        }
        if (m_Camera != null)
            bl_GameController.Instance.CameraRendered = m_Camera;
    }
}