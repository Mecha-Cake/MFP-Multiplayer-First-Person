using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class bl_DraggableObject : bl_PhotonHelper
{
    public float DragForce = 600;
    public float Damping = 6;

    private Rigidbody m_RigidBody;
    public bool isDrag { get; set; }
    public Vector3 CameraPoint { get; set; }
    private Transform Join;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Take()
    {
        if (photonView.Owner != PhotonNetwork.LocalPlayer)
        {
            photonView.RequestOwnership();
        }
        m_RigidBody.useGravity = true;
        m_RigidBody.isKinematic = false;
        Join = CreateJoin(m_RigidBody, CameraPoint);
        isDrag = true;
        photonView.RPC("RpcDraggable", RpcTarget.Others, true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Drop()
    {
        if (Join != null)
        {
            Destroy(Join.gameObject);
            Join = null;
        }
        isDrag = false;
        photonView.RPC("RpcDraggable", RpcTarget.Others, false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Drag(Vector3 position)
    {
        if (Join == null) return;
        Join.position = position;

        if(Vector3.Distance(Join.position, transform.position) > 2)
        {
            Drop();
            bl_UIManager.Instance.SetHandIcon(0);
        }
    }

    Transform CreateJoin(Rigidbody rb, Vector3 attachmentPosition)
    {
        GameObject go = new GameObject("DragJoin");
        go.hideFlags = HideFlags.HideInHierarchy;
        go.transform.position = attachmentPosition;

        Rigidbody newRb = go.AddComponent<Rigidbody>();
        newRb.isKinematic = true;

        ConfigurableJoint joint = go.AddComponent<ConfigurableJoint>();
        joint.connectedBody = rb;
        joint.configuredInWorldSpace = true;
        joint.xDrive = NewJointDrive(DragForce, Damping);
        joint.yDrive = NewJointDrive(DragForce, Damping);
        joint.zDrive = NewJointDrive(DragForce, Damping);
        joint.slerpDrive = NewJointDrive(DragForce, Damping);
        joint.rotationDriveMode = RotationDriveMode.Slerp;

        return go.transform;
    }

    private JointDrive NewJointDrive(float force, float damping)
    {
        JointDrive drive = new JointDrive();
        drive.positionSpring = force;
        drive.positionDamper = damping;
        drive.maximumForce = Mathf.Infinity;
        return drive;
    }

    [PunRPC]
    void RpcDraggable(bool take)
    {
        isDrag = take;
        m_RigidBody.useGravity = !take;
        m_RigidBody.isKinematic = take;
    }

    public string OwnerID { get { if (photonView.Owner != null) return photonView.Owner.UserId; else return "Offline Player"; } }
}