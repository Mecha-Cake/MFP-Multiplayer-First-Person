using UnityEngine;


[RequireComponent(typeof(CarController))]
public class CarUserControl : bl_PhotonHelper
{

    private CarController m_Car; // the car controller we want to use

    private void Awake()
    {
        // get the car controller
        m_Car = GetComponent<CarController>();
    }

    private void FixedUpdate()
    {

        // pass the input to the car!
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
#if !MOBILE_INPUT
        float handbrake = (Input.GetKeyDown(KeyCode.Space)) ? 1 : 0;
        m_Car.Move(h, v, v, handbrake, false, Vector3.zero);
#else
            m_Car.Move(h, v, v, 0f,false);
#endif
    }

}