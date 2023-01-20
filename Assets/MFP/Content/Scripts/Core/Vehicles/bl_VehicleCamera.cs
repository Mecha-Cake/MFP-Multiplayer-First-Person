using UnityEngine;
using System.Collections;

public class bl_VehicleCamera : MonoBehaviour
{
    public AnimationCurve TransitionCurve;
    public float TransitionDuration = 0.5f;
    public float smoothing = 6f;

    private Transform lookAtTarget;
    private Transform positionTarget;

    private bool Hold = false;
    private bool Interrup = false;
    private float CameraTargetDistance = 1;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        //cache singleton
        bl_VehicleCamera.Instance.Disable();
    }

    /// <summary>
    /// 
    /// </summary>
    private void FixedUpdate()
    {
        if (lookAtTarget == null || Hold)
            return;

        UpdateCamera();
        DetectCollision();
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateCamera()
    {
        if (Interrup)
            return;

        transform.position = Vector3.Lerp(transform.position, positionTarget.position, Time.deltaTime * smoothing);
        transform.LookAt(lookAtTarget);
    }

    void DetectCollision()
    {
        //if there a obstacle detected
        Interrup = (Physics.CheckSphere(transform.position, 0.5f));
        if (Interrup)
        {
            transform.LookAt(lookAtTarget);
            if (Vector3.Distance(transform.position, lookAtTarget.position) >= CameraTargetDistance) { Interrup = false; }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position , 0.5f);
    }

    public void SetCarTarget(Transform car)
    {
        GetRig(car);
        gameObject.SetActive(true);
    }

    public void SetCarTarget(Transform car,Transform from)
    {
        Hold = true;
        bool f = GetRig(car);
        CameraTargetDistance = Vector3.Distance(lookAtTarget.position, positionTarget.position);
        gameObject.SetActive(true);
        if (f)
        {
            StartCoroutine(Transition(from));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Disable()
    {
        StopAllCoroutines();
        lookAtTarget = null;
        positionTarget = null;
        gameObject.SetActive(false);
        Hold = false;
    }

    bool GetRig(Transform car)
    {
        bl_VehicleCameraRig vr = car.GetComponentInChildren<bl_VehicleCameraRig>();
        if (vr == null) { Debug.LogWarning("Can't found camera rig references on child's."); return false; }

        lookAtTarget = vr.LookAtTarget;
        positionTarget = vr.CameraPosition;
        return true;
    }

    IEnumerator Transition(Transform From)
    {
        float d = 0;
        Vector3 po = From.position;
        Quaternion ro = From.rotation;
        transform.position = po;
        transform.rotation = ro;
        positionTarget.LookAt(lookAtTarget);
        float curve = 0;

        while(d < 1)
        {
            d += Time.deltaTime / TransitionDuration;
            curve = TransitionCurve.Evaluate(d);
            transform.position = Vector3.Lerp(po, positionTarget.position, curve);
            transform.rotation = Quaternion.Slerp(ro, positionTarget.rotation, curve);
            yield return null;
        }
        Hold = false;
    }

    private static bl_VehicleCamera _instance;
    public static bl_VehicleCamera Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_VehicleCamera>(); }
            return _instance;
        }
    }
}