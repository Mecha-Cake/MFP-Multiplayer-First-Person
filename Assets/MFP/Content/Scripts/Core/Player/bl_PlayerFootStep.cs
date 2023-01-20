using UnityEngine;
using System.Collections;

public class bl_PlayerFootStep : MonoBehaviour
{

    private bl_PlayerMovement PlayerMovement = null;
    private bl_PlayerAnimator PlayerAnimator;
    private AudioSource StepSource;


    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (PlayerMovement == null)
        {
            PlayerMovement = transform.GetComponentInParent<bl_PlayerMovement>();
        }
        PlayerAnimator = transform.GetComponentInParent<bl_PlayerAnimator>();
        StepSource = PlayerAnimator.FootStepSource;
        StartCoroutine(FootStepUpdate());
    }

    /// <summary>
    /// 
    /// </summary>
    public void Initialized()
    {
        PlayerMovement = transform.root.GetComponent<bl_PlayerMovement>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator FootStepUpdate()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            float vel = PlayerAnimator.velocity.magnitude;
            if (PlayerAnimator.grounded && vel > 0.2f && !PlayerAnimator.PlayerCar.isInVehicle)
            {
                AudioClip a = GetFloorStepAudio;
                if (a != null)
                {
                    StepSource.clip = a;
                    StepSource.Play();
                }
                float interval = PlayerMovement.minInterval * (PlayerMovement.runSpeed) / (vel);
                interval = Mathf.Clamp(interval, PlayerMovement.minInterval, 1);
                yield return new WaitForSeconds(interval);
            }
            else
            {
                yield return 0;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private AudioClip GetFloorStepAudio
    {
        get
        {
            if (PlayerMovement == null) return null;
            return PlayerMovement.Footsteps[0].StepsSound[Random.Range(0, PlayerMovement.Footsteps[0].StepsSound.Length)];
        }
    }
}