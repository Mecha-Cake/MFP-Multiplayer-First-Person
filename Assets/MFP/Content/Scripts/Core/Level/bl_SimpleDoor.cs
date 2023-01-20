using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class bl_SimpleDoor : MonoBehaviour
{
    [Range(-360, 360), SerializeField] private float CloseAngle = 0;
    [Range(-360, 360), SerializeField] private float OpenAngle = 0;

    [Separator("References")]
    [SerializeField] private AudioClip OpenSound = null;

    private int isOpen { get; set; }

    private bl_DoorManagers Manager;
    private AudioSource ASource;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        Manager = FindObjectOfType<bl_DoorManagers>();
        ASource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Open()
    {
        isOpen = 1;
        StopCoroutine("RotateDoor");
        StartCoroutine("RotateDoor");
        Manager.ChangeDoorState(this, isOpen);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Close()
    {
        isOpen = 0;
        StopCoroutine("RotateDoor");
        StartCoroutine("RotateDoor");
        Manager.ChangeDoorState(this, isOpen);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Intercalate()
    {
        if (isOpen == 1 || isOpen == -1)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ApplyState(int state)
    {
        isOpen = state;
        StopCoroutine("RotateDoor");
        StartCoroutine("RotateDoor");
    }

    /// <summary>
    /// 
    /// </summary>
    public void ApplyStateInsta(int state)
    {
        isOpen = state;
        Vector3 v = transform.eulerAngles;
        if (isOpen == 0)
        {
            v.y = CloseAngle;
        }
        else if (isOpen == 1)
        {
            v.y = OpenAngle;
        }
        transform.eulerAngles = v;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator RotateDoor()
    {
        float d = 0;
        ASource.clip = OpenSound;
        ASource.Play();

        Vector3 v = transform.eulerAngles;
        while (d < 1)
        {
            d += Time.deltaTime / 2;
            if (isOpen == 0)
            {
                v.y = CloseAngle;
            }
            else if (isOpen == 1)
            {
                v.y = OpenAngle;
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(v), d);
            yield return new WaitForEndOfFrame();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Vector3 v = transform.eulerAngles;
        v.y = CloseAngle;
        transform.eulerAngles = v;
        if(Manager == null)
        {
            Manager = FindObjectOfType<bl_DoorManagers>();
        }
        if (!string.IsNullOrEmpty(gameObject.scene.name))
        {
            Manager.RegisteDoor(this);
        }
    }
#endif
}