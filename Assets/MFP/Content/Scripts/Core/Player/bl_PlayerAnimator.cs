//////////////////////////////////////////////////////////////////////////////
// bl_PlayerAnimations.cs
//
// - was ordered to encourage TPS player animations using legacy animations,
//  and heat look controller from Unity technologies.
//
//                           Lovatto Studio
/////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class bl_PlayerAnimator : MonoBehaviour
{

    public PlayerState m_PlayerState = PlayerState.Stand;
    [Header("Player Models")]
    public bl_PlayerModel[] PlayerModels;
    [Separator("References")]
    public Transform PlayerRoot;
    public Transform mTarget = null;
    public AudioSource FootStepSource;
    

    [HideInInspector] public bool m_Update = true;
    [HideInInspector] public bool grounded = true;
    [HideInInspector] public Vector3 velocity = Vector3.zero;
    [HideInInspector] public Vector3 DirectionalSpeed = Vector3.zero;
    [HideInInspector] public float m_PlayerSpeed;
    [HideInInspector] public float lastYRotation;
    private bool isDeath = false;
    public  bl_PlayerCar PlayerCar { get; set; }
    public bl_PlayerModel ActualPlayerModel { get; set; }
    private bool receivedPlayerModel = false;
    public int CurrentModelID { get; set; } = -1;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        PlayerCar = PlayerRoot.GetComponent<bl_PlayerCar>();
        if (!receivedPlayerModel)
        {
            foreach (bl_PlayerModel m in PlayerModels)
            {
                m.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (!m_Update || isDeath || ActualPlayerModel == null)
            return;

        ControllerInfo();
        Animate();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLocalDeath()
    {
        isDeath = true;
        ModelAnimator.Play("Death", 0, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnRemoteDeath(int t)
    {
        isDeath = true;
        ModelAnimator.Play("Death", 0, 0);
        Destroy(gameObject, t);
    }


    /// <summary>
    /// Get current player information for send to animator
    /// </summary>
    void ControllerInfo()
    {
        DirectionalSpeed = PlayerRoot.InverseTransformDirection(velocity);
        DirectionalSpeed.y = 0;
        m_PlayerSpeed = Mathf.Lerp(m_PlayerSpeed, velocity.magnitude, Time.deltaTime * 8);
    }

    /// <summary>
    /// 
    /// </summary>
    void Animate()
    {
        ModelAnimator.SetFloat("Speed", m_PlayerSpeed);
        ModelAnimator.SetFloat("Z", DirectionalSpeed.z);
        ModelAnimator.SetFloat("X", DirectionalSpeed.x);
        ModelAnimator.SetBool("Jump", !grounded && !PlayerCar.isInVehicle);
        ModelAnimator.SetBool("Crouch", grounded && m_PlayerState == PlayerState.Crouch);
    }

    public void SetPlayerModel(int ID)
    {
        if (ID < 0 || ID > PlayerModels.Length - 1 || CurrentModelID == ID) return;

        CurrentModelID = ID;
        foreach (bl_PlayerModel m in PlayerModels)
        {
            m.gameObject.SetActive(false);
        }
        ActualPlayerModel = PlayerModels[ID];
        PlayerModels[ID].gameObject.SetActive(true);
        PlayerModels[ID].GetComponent<bl_PlayerFootStep>().Initialized();
        receivedPlayerModel = true;
        var vehicle = transform.GetComponentInParent<bl_VehicleManager>();
        if (vehicle != null && vehicle.PlayerVisibleInside)
        {
            ModelAnimator.SetInteger("Vehicle", 2);
        }
    }

    public Animator ModelAnimator
    {
        get
        {
            if(ActualPlayerModel == null)
            {
                return PlayerModels[0].m_Animator;
            }
            return ActualPlayerModel.m_Animator;
        }
    }

    public Transform Target
    {
        get
        {
            if ((mTarget != null && !PlayerCar.isPlayerInsideVehicle) || !PlayerCar.isLocal)
            {
                return mTarget;
            }
            else
            {
                if (PlayerCar.isInVehicle)
                {
                    return PlayerCar.Vehicle.HeatLook;
                }
                else
                {
                    return PlayerCar.m_Seat.HeatLook;
                }
            }
        }
    }
}