using UnityEngine;

public class bl_HeatLookMecanim : MonoBehaviour {

	
    [Range(0,1)]public float Weight = 0.95f;
    [Range(0,1)]public float Body = 0.8f;
    [Range(0,1)]public float Head = 1;
    [Range(0,1)]public float Eyes = 1;
    [Range(0,1)]public float Clamp = 0.75f;
    [Range(1,20)]public float Lerp = 8;

    private Animator animator;
    private Vector3 target;
    private Vector3 CachePosition;
    private Quaternion CacheRotation;
    private bl_PlayerAnimator PlayerAnim;
    private bl_PlayerCar PlayerCar;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        animator = GetComponent<Animator>();
        PlayerAnim = transform.root.GetComponentInChildren<bl_PlayerAnimator>();
        PlayerCar = PlayerAnim.PlayerCar;
    }

    void OnAnimatorIK(int layer)
    {
        if (Target == null)
            return;

        if (layer == 0)
        {
            animator.SetLookAtWeight(Weight, Body, Head, Eyes, Clamp);
            target = Vector3.Slerp(target, Target.position, Time.deltaTime * 8);
            animator.SetLookAtPosition(target);

            if (PlayerAnim.PlayerCar.isInVehicle && PlayerCar.Vehicle.SteeringWheelPoint != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
                animator.SetIKPosition(AvatarIKGoal.RightHand, PlayerCar.Vehicle.SteeringHandRight);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, PlayerCar.Vehicle.SteeringHandLeft);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.0f);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.0f);
            }
        }
    }

    private Transform Target
    {
        get
        {
            if(PlayerAnim == null) { PlayerAnim = transform.GetComponentInParent<bl_PlayerAnimator>(); }
            return PlayerAnim.Target;
        }
    }

}