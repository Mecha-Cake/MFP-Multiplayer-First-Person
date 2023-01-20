using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class bl_VehicleUI : MonoBehaviour
{

    [SerializeField] private GameObject EnterVehicleUI = null;
    [SerializeField] private Text EnterVehicleText = null;
    public Text SpeedometerText;
    [SerializeField] private Text VehicleHealthText = null;

    private void Awake()
    {
        EnterVehicleUI.SetActive(false);
        VehicleHealthText.gameObject.SetActive(false);
    }

    public void SetEnterUI(bool Active, KeyCode key = KeyCode.None)
    {
        if (EnterVehicleText != null)
        {
            string t = string.Format("PRESS <color=#4D90F0>'{0}'</color> TO ENTER", key.ToString().ToUpper());
            EnterVehicleText.text = t;
        }
        EnterVehicleUI.SetActive(Active);
    }

    public void UpdateSpeedometer(float v, VehicleType vehicleType, SpeedType dt = SpeedType.KPH)
    {
        if (vehicleType == VehicleType.Car)
        {
            SpeedometerText.text = string.Format("{0}<size=12>\nK/H</size>", v.ToString("f0"));
        }
        else if (vehicleType == VehicleType.Jet)
        {
            SpeedometerText.text = string.Format("{0}<size=12>\nK/H</size>", v.ToString("f0"));
        }
    }

    public void OnEnter(VehicleType vt)
    {
        VehicleHealthText.gameObject.SetActive(true);
        if (vt == VehicleType.Car)
        {
            SpeedometerText.gameObject.SetActive(true);
        }
        else if (vt == VehicleType.Jet)
        {

        }
    }

    public void OnExit(VehicleType vt)
    {
        VehicleHealthText.gameObject.SetActive(false);
        if (vt == VehicleType.Car)
        {
            SpeedometerText.gameObject.SetActive(false);
        }
        else if (vt == VehicleType.Jet)
        {

        }
    }

    public void SetHealth(int health)
    {
        string t = string.Format("{0}<size=14> VEHICLE HEALTH</size>", health);
        VehicleHealthText.text = t;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_EventHandler.LocalPlayerDeathEvent += OnLocalDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.LocalPlayerDeathEvent -= OnLocalDeath;
    }

    void OnLocalDeath()
    {
        EnterVehicleUI.SetActive(false);
    }
}