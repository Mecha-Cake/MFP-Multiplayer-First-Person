//////////////////////////////////////////////////////////////////////////////
// bl_MouseLook.cs
//
//
//
//                Lovatto Studio
/////////////////////////////////////////////////////////////////////////////
using UnityEngine;

public class bl_MouseLook : MonoBehaviour {

    public bool isPlayer = true;
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    public float offsetY = 0F;

    float rotationX = 0F;
    private GameObject m_Camera = null;

    public float rotationY = 0F;
    //private
    Quaternion originalRotation;
    private bool isDeath = false;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>() != null)
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }

        if (Camera.main != null)
        {
            m_Camera = Camera.main.gameObject;
        }
        originalRotation = transform.localRotation;

        if (PlayerPrefs.HasKey("sensitive"))
        {
            sensitivityX = PlayerPrefs.GetFloat("sensitive");
            sensitivityY = PlayerPrefs.GetFloat("sensitive");
        }
        else
        {
            sensitivityX = 6;
            sensitivityY = 6;
        }

    }
    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_EventHandler.LocalPlayerDeathEvent += OnLocalDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.LocalPlayerDeathEvent -= OnLocalDeath;
    }

    void OnLocalDeath()
    {
        isDeath = true;
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (isDeath)
            return;
        if (m_Camera == null)
        {
            GetCurrentCamera();
            return;
        }
        if (!bl_CoopUtils.GetCursorState)
            return;

            if (axes == RotationAxes.MouseXAndY)
            {
                // Read the mouse input axis
                rotationX += Input.GetAxis("Mouse X") * sensitivityX / 60 * m_Camera.GetComponent<Camera>().fieldOfView;
                rotationY += (Input.GetAxis("Mouse Y") * sensitivityY / 60 * m_Camera.GetComponent<Camera>().fieldOfView + offsetY);

                rotationX = bl_CoopUtils.ClampAngle(rotationX, minimumX, maximumX);
                rotationY = bl_CoopUtils.ClampAngle(rotationY, minimumY, maximumY);

                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);

                transform.localRotation = originalRotation * xQuaternion * yQuaternion;
            }
            else if (axes == RotationAxes.MouseX)
            {
                rotationX += Input.GetAxis("Mouse X") * sensitivityX;
                rotationX = bl_CoopUtils.ClampAngle(rotationX, minimumX, maximumX);

                Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
                transform.localRotation = originalRotation * xQuaternion;
            }
            else
            {
                rotationY += Input.GetAxis("Mouse Y") * sensitivityY + offsetY;
                rotationY = bl_CoopUtils.ClampAngle(rotationY, minimumY, maximumY);

                Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
                transform.localRotation = originalRotation * yQuaternion;
            }
            offsetY = 0F;
        

    }

    /// <summary>
    /// if we have the camera, then seek one.
    /// </summary>
    void GetCurrentCamera()
    {
        if (Camera.main != null)
        {
            m_Camera = Camera.main.gameObject;
        }
        else if (Camera.current != null)
        {
            m_Camera = Camera.current.gameObject;
        }
        else
        {
            m_Camera = this.gameObject;
        }
    }
}