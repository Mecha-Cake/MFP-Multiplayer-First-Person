using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_VehicleCameraRig : MonoBehaviour
{

    public Transform LookAtTarget;
    public Transform CameraPosition;

    private void OnDrawGizmosSelected()
    {
        if (CameraPosition != null)
        {
            Matrix4x4 tempMat = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(CameraPosition.position, CameraPosition.rotation, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, 50, 15, 0.5f, 1);
            Gizmos.matrix = tempMat;
        }
        if(LookAtTarget != null)
        {
            Gizmos.DrawSphere(LookAtTarget.position, 0.5f);
            Gizmos.DrawLine(CameraPosition.position, LookAtTarget.position);
        }
    }
}