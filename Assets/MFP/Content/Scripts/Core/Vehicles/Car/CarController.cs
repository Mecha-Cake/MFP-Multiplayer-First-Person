using UnityEngine;
//Script from vehicles standard asset from unity technologies
//Example of how sync a car movement.
    internal enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }

    public enum SpeedType
    {
        MPH,
        KPH
    }

public class CarController : bl_PhotonHelper
{
    [SerializeField]
    private CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;
    [SerializeField]
    private WheelCollider[] m_WheelColliders = new WheelCollider[4];
    [SerializeField]
    private GameObject[] m_WheelMeshes = new GameObject[4];
    [SerializeField]
    private WheelEffects[] m_WheelEffects = new WheelEffects[4];
    [SerializeField]
    private Vector3 m_CentreOfMassOffset = Vector3.zero;
    [SerializeField]
    private float m_MaximumSteerAngle = 30;
    [Range(0, 1)]
    [SerializeField]
    private float m_SteerHelper = 0.5f; // 0 is raw physics , 1 the car will grip in the direction it is facing
    [Range(0, 1)]
    [SerializeField]
    private float m_TractionControl = 0.1f; // 0 is no traction control, 1 is full interference
    [SerializeField]
    private float m_FullTorqueOverAllWheels = 1;
    [SerializeField]
    private float m_ReverseTorque = 0;
    [SerializeField]
    private float m_MaxHandbrakeTorque = 0;
    [SerializeField]
    private float m_Downforce = 100f;
    [SerializeField]
    public SpeedType m_SpeedType;
    [SerializeField]
    private float m_Topspeed = 200;
    [SerializeField]
    private static int NoOfGears = 5;
    [SerializeField]
    private float m_RevRangeBoundary = 1f;
    [SerializeField]
    private float m_SlipLimit = 15;
    [SerializeField]
    private float m_BrakeTorque = 0;
    [HideInInspector]
    public float CarVelocity = 0;
    private Quaternion[] m_WheelMeshLocalRotations;
    private Vector3 m_Prevpos, m_Pos;
    private float m_SteerAngle;
    private int m_GearNum;
    private float m_GearFactor;
    private float m_OldRotation;
    private float m_CurrentTorque;
    private Rigidbody m_Rigidbody;
    private const float k_ReversingThreshold = 0.01f;

    public bool Skidding { get; private set; }
    public float BrakeInput { get; private set; }
    public float CurrentSteerAngle { get { return m_SteerAngle; } }
    public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 2.23693629f; } }
    public float MaxSpeed { get { return m_Topspeed; } }
    public float Revs { get; private set; }
    public float AccelInput { get; private set; }
    public float VelocityMagnitude { get; set; }
    public Vector3 Velocity { get; set; }


    //cache local input move vars for sync
    public float m_brakeInput { get; private set; }
    public float m_steerin { get; private set; }
    public float m_accel { get; private set; }
    public float m_handbrake { get; private set; }
    private float FactorForce = 1;
    private Vector3 FactorDirection;
    private bool isFactorOn = false;
    private float smootSteer = 0;

    // Use this for initialization
    private void Start()
    {
        m_WheelMeshLocalRotations = new Quaternion[4];
        for (int i = 0; i < 4; i++)
        {
            m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
        }
        m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

        m_MaxHandbrakeTorque = float.MaxValue;

        m_Rigidbody = GetComponent<Rigidbody>();
        m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Move(float steering, float accel, float footbrake, float handbrake, bool remote, Vector3 velocity)
    {
        m_steerin = steering;
        m_accel = accel;
        m_brakeInput = footbrake;
        m_handbrake = handbrake;

        //clamp input values
        steering = Mathf.Clamp(steering, -1, 1);
        AccelInput = accel = Mathf.Clamp(accel, 0, 1);
        BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
        handbrake = Mathf.Clamp(handbrake, 0, 1);

        Velocity = m_Rigidbody.velocity;
        VelocityMagnitude = Velocity.magnitude;

        //Set the steer on the front wheels.
        //Assuming that wheels 0 and 1 are the front wheels.
        m_SteerAngle = steering * m_MaximumSteerAngle;
        smootSteer = (!remote) ? m_SteerAngle : Mathf.Lerp(smootSteer, m_SteerAngle, Time.deltaTime * 8);
        m_WheelColliders[0].steerAngle = smootSteer;
        m_WheelColliders[1].steerAngle = smootSteer;

        if (!remote)
        {
            SteerHelper();
            CapSpeed();
        }
        ApplyDrive(accel, footbrake);

        //Set the handbrake.
        //Assuming that wheels 2 and 3 are the rear wheels.
        if (handbrake > 0f)
        {
            float hbTorque = handbrake * m_MaxHandbrakeTorque;
            m_WheelColliders[2].brakeTorque = hbTorque;
            m_WheelColliders[3].brakeTorque = hbTorque;
        }

        CalculateRevs();
        if (!remote)
        {
            GearChanging();
        }
        AddDownForce();
        CheckForWheelSpin();
        TractionControl();
        if (remote)
        {
            m_Rigidbody.velocity = velocity;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void FixedUpdate()
    {
        WheelPosition();
    }

    /// <summary>
    /// 
    /// </summary>
    public void WheelPosition()
    {
        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 position;
            m_WheelColliders[i].GetWorldPose(out position, out quat);
            m_WheelMeshes[i].transform.position = position;
            m_WheelMeshes[i].transform.rotation = quat;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void GearChanging()
    {
        if (!photonView.IsMine) return;
        float f = Mathf.Abs(CurrentSpeed / MaxSpeed);
        float upgearlimit = (1 / (float)NoOfGears) * (m_GearNum + 1);
        float downgearlimit = (1 / (float)NoOfGears) * m_GearNum;

        if (m_GearNum > 0 && f < downgearlimit)
        {
            m_GearNum--;
        }

        if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
        {
            m_GearNum++;
        }
    }


    // simple function to add a curved bias towards 1 for a value in the 0-1 range
    private static float CurveFactor(float factor)
    {
        return 1 - (1 - factor) * (1 - factor);
    }


    // unclamped version of Lerp, to allow value to exceed the from-to range
    private static float ULerp(float from, float to, float value)
    {
        return (1.0f - value) * from + value * to;
    }

    /// <summary>
    /// 
    /// </summary>
    private void CalculateGearFactor()
    {
        if (!photonView.IsMine) return;
        float f = (1 / (float)NoOfGears);
        // gear factor is a normalized representation of the current speed within the current gear's range of speeds.
        // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
        var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(CurrentSpeed / MaxSpeed));
        m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
    }

    /// <summary>
    /// 
    /// </summary>
    private void CalculateRevs()
    {
        // calculate engine revs (for display / sound)
        // (this is done in retrospect - revs are not used in force/power calculations)
        CalculateGearFactor();
        float gearNumFactor = m_GearNum / (float)NoOfGears;
        float revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
        float revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
        Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
    }

    public void ApplyFactor(bool apply, Vector3 Direction, float Force)
    {
        FactorDirection = Direction;
        isFactorOn = apply;
        FactorForce = Force * 0.1f;
    }

    private void CapSpeed()
    {
        CarVelocity = VelocityMagnitude;
        switch (m_SpeedType)
        {
            case SpeedType.MPH:

                CarVelocity *= 2.23693629f;
                if (CarVelocity > m_Topspeed)
                {
                    m_Rigidbody.velocity = (m_Topspeed / 2.23693629f) * Velocity.normalized;
                }
                break;

            case SpeedType.KPH:
                CarVelocity *= 3.6f;
                if (CarVelocity > m_Topspeed)
                {
                    m_Rigidbody.velocity = (m_Topspeed / 3.6f) * Velocity.normalized;
                }
                break;
        }
        if (isFactorOn) { m_Rigidbody.velocity += FactorDirection * FactorForce; }
    }


    private void ApplyDrive(float accel, float footbrake)
    {

        float thrustTorque;
        switch (m_CarDriveType)
        {
            case CarDriveType.FourWheelDrive:
                thrustTorque = accel * (m_CurrentTorque / 4f);
                for (int i = 0; i < 4; i++)
                {
                    m_WheelColliders[i].motorTorque = thrustTorque;
                }
                break;

            case CarDriveType.FrontWheelDrive:
                thrustTorque = accel * (m_CurrentTorque / 2f);
                m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustTorque;
                break;

            case CarDriveType.RearWheelDrive:
                thrustTorque = accel * (m_CurrentTorque / 2f);
                m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = thrustTorque;
                break;

        }

        for (int i = 0; i < 4; i++)
        {
            if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, Velocity) < 50f)
            {
                m_WheelColliders[i].brakeTorque = m_BrakeTorque * footbrake;
            }
            else if (footbrake > 0)
            {
                m_WheelColliders[i].brakeTorque = 0f;
                m_WheelColliders[i].motorTorque = -m_ReverseTorque * footbrake;
            }
        }
    }


    private void SteerHelper()
    {
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelhit;
            m_WheelColliders[i].GetGroundHit(out wheelhit);
            if (wheelhit.normal == Vector3.zero)
                return; // wheels arent on the ground so dont realign the rigidbody velocity
        }

        // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
        if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
        {
            var turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
            Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
            m_Rigidbody.velocity = velRotation * Velocity;
        }
        m_OldRotation = transform.eulerAngles.y;
    }


    // this is used to add more grip in relation to speed
    private void AddDownForce()
    {
        m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up * m_Downforce * VelocityMagnitude);
    }


    // checks if the wheels are spinning and is so does three things
    // 1) emits particles
    // 2) plays tiure skidding sounds
    // 3) leaves skidmarks on the ground
    // these effects are controlled through the WheelEffects class
    private void CheckForWheelSpin()
    {
        // loop through all wheels
        for (int i = 0; i < 4; i++)
        {
            WheelHit wheelHit;
            m_WheelColliders[i].GetGroundHit(out wheelHit);

            // is the tire slipping above the given threshhold
            if (Mathf.Abs(wheelHit.forwardSlip) >= m_SlipLimit || Mathf.Abs(wheelHit.sidewaysSlip) >= m_SlipLimit)
            {
                m_WheelEffects[i].EmitTyreSmoke();

                // avoiding all four tires screeching at the same time
                // if they do it can lead to some strange audio artefacts
                if (!AnySkidSoundPlaying())
                {
                    m_WheelEffects[i].PlayAudio();
                }
                continue;
            }

            // if it wasnt slipping stop all the audio
            if (m_WheelEffects[i].PlayingAudio)
            {
                m_WheelEffects[i].StopAudio();
            }
            // end the trail generation
            m_WheelEffects[i].EndSkidTrail();
        }
    }

    // crude traction control that reduces the power to wheel if the car is wheel spinning too much
    private void TractionControl()
    {
        WheelHit wheelHit;
        switch (m_CarDriveType)
        {
            case CarDriveType.FourWheelDrive:
                // loop through all wheels
                for (int i = 0; i < 4; i++)
                {
                    m_WheelColliders[i].GetGroundHit(out wheelHit);

                    AdjustTorque(wheelHit.forwardSlip);
                }
                break;

            case CarDriveType.RearWheelDrive:
                m_WheelColliders[2].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);

                m_WheelColliders[3].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
                break;

            case CarDriveType.FrontWheelDrive:
                m_WheelColliders[0].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);

                m_WheelColliders[1].GetGroundHit(out wheelHit);
                AdjustTorque(wheelHit.forwardSlip);
                break;
        }
    }


    private void AdjustTorque(float forwardSlip)
    {
        if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
        {
            m_CurrentTorque -= 10 * m_TractionControl;
        }
        else
        {
            m_CurrentTorque += 10 * m_TractionControl;
            if (m_CurrentTorque > m_FullTorqueOverAllWheels)
            {
                m_CurrentTorque = m_FullTorqueOverAllWheels;
            }
        }
    }


    private bool AnySkidSoundPlaying()
    {
        for (int i = 0; i < 4; i++)
        {
            if (m_WheelEffects[i].PlayingAudio)
            {
                return true;
            }
        }
        return false;
    }
}