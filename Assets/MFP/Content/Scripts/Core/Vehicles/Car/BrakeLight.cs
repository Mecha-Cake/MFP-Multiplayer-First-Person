using UnityEngine;
using System.Collections;
using Photon.Pun;

public class BrakeLight : MonoBehaviour
{
    public CarController car; // reference to the car controller, must be dragged in inspector
    public PhotonView View;
    public bl_VehicleManager Manager;
    public Light[] m_LightsBreaks;
    [SerializeField]
    private float MaxBreakIntensity = 4;
    private float LightIntensity = 0;


    private Renderer m_Renderer;


    private void Start()
    {
        m_Renderer = GetComponent<Renderer>();
    }


    private void FixedUpdate()
    {
        if (View.IsMine)
        {
            // enable the Renderer when the car is braking, disable it otherwise.
            m_Renderer.enabled = car.BrakeInput > 0f;
        }
        else
        {
            m_Renderer.enabled = Manager.Input1 > 0f;
        }

        if (car.BrakeInput > 0)
        {
            LightIntensity = Mathf.Lerp(LightIntensity, MaxBreakIntensity, Time.deltaTime * 2);
        }
        else
        {
            LightIntensity = Mathf.Lerp(LightIntensity, 0, Time.deltaTime * 2);
        }
        for (int i = 0; i < m_LightsBreaks.Length; i++)
        {
            m_LightsBreaks[i].intensity = LightIntensity;
        }
    }

}