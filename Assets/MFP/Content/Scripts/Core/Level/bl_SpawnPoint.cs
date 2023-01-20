using UnityEngine;
using System.Collections;

public class bl_SpawnPoint : MonoBehaviour
{

    public float SpawnSpace = 3f;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        bl_GameController.Instance.RegisterSpawnPoint(this);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDestroy()
    {
        if(bl_GameController.Instance != null)
        bl_GameController.Instance.UnRegisterSpawnPoint(this);
    }

    //Debug Spawn Spcae
    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(base.transform.position, Quaternion.identity, new Vector3(1f, 0.1f, 1f));
        Color c = Color.green;
        Gizmos.color = c;
        Gizmos.DrawWireSphere(Vector3.zero, SpawnSpace);
        Gizmos.color = new Color(c.r, c.g, c.b, 0.4f);
        Gizmos.DrawSphere(Vector3.zero, this.SpawnSpace);
        Gizmos.matrix = Matrix4x4.identity;
        //Drag line helper to know the forward of spawnpoint.
        Gizmos.DrawLine(base.transform.position + ((base.transform.forward * this.SpawnSpace)), base.transform.position + (((base.transform.forward * 2f) * this.SpawnSpace)));
    }

    public float SpawnRadius
    {
        get
        {
            return this.SpawnSpace;
        }
    }
}