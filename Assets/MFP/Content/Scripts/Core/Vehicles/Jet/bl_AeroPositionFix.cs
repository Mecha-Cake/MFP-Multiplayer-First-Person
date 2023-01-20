using UnityEngine;
using Photon.Pun;

public class bl_AeroPositionFix : bl_PhotonHelper {

    private Vector3 OriginPosition;
    private Quaternion OriginRotation;

    // Use this for initialization
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            OriginPosition = transform.position;
            OriginRotation = transform.rotation;
            photonView.RPC("GetOriginTransform", RpcTarget.OthersBuffered, transform.position, transform.rotation);
        }
    }
	

    [PunRPC]
    void GetOriginTransform(Vector3 p,Quaternion r)
    {
        OriginPosition = p;
        OriginRotation = r;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetPosition()
    {
        photonView.RPC("GetOriginTransform", RpcTarget.OthersBuffered, OriginPosition, OriginRotation);
    }


}