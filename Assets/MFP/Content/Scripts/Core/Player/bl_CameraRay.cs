using UnityEngine;
using System.Collections;
using Photon.Pun;

public class bl_CameraRay : MonoBehaviour
{

    public float DistanceCheck = 2;

    private DetectType Detected = DetectType.None;
    private bl_SimpleDoor CacheSimpleDoor = null;
    private Transform m_Transform;
    private bl_DraggableObject Draggable;
    private bool BlockRay = false;


    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        m_Transform = transform;
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        RayDetection();
        InputControl();
        UpdateObject();
    }

    /// <summary>
    /// 
    /// </summary>
    void RayDetection()
    {
        if (BlockRay) return;

        RaycastHit hit;

        Vector3 fwr = m_Transform.forward;
        Debug.DrawRay(m_Transform.position, fwr, Color.green);

        if (Physics.Raycast(m_Transform.position, fwr, out hit, DistanceCheck))
        {
            if (hit.transform.GetComponent<bl_SimpleDoor>() != null)
            {
                CacheSimpleDoor = hit.transform.GetComponent<bl_SimpleDoor>();
                Detected = DetectType.Door;
                bl_UIManager.Instance.ShowInputText(true, "PRESS [F]");
            }
            else if (hit.transform.GetComponent<bl_DraggableObject>() != null)
            {
                Draggable = hit.transform.GetComponent<bl_DraggableObject>();
                if (!Draggable.isDrag)
                {
                    Draggable.CameraPoint = hit.point;
                    bl_UIManager.Instance.SetHandIcon(1);
                    Detected = DetectType.Draggable;
                }
            }
        }
        else
        {
            if (Detected != DetectType.None)
            {
                bl_UIManager.Instance.ShowInputText(false);
                bl_UIManager.Instance.SetHandIcon(0);
                Detected = DetectType.None;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void InputControl()
    {
        if (Detected == DetectType.Door && CacheSimpleDoor != null)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                CacheSimpleDoor.Intercalate();
            }
        }
        else if (Detected == DetectType.Draggable && Draggable != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!Draggable.isDrag)
                {
                    Draggable.Take();
                    BlockRay = true;
                    bl_UIManager.Instance.SetHandIcon(2);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (Draggable.OwnerID == PhotonNetwork.LocalPlayer.UserId)
                {
                    if (Draggable.isDrag)
                    {
                        Draggable.Drop();
                    }
                    BlockRay = false;
                    Draggable = null;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateObject()
    {
        if (Detected == DetectType.Draggable && Draggable != null)
        {
            if (Draggable.isDrag)
            {
                Draggable.Drag(m_Transform.position + (m_Transform.forward * 1.5f));
            }
        }
        else if (Detected == DetectType.Draggable && (Draggable == null || !Draggable.isDrag))
        {
            Detected = DetectType.None;
            BlockRay = false;
        }
    }
  

    public enum DetectType
    {
        None = 0,
        Door = 1,
        Draggable = 2,
    }
}