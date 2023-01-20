///This script is just a example of how use the pickup event delegate for called others function
/// you can use for any other script.
/// 
using UnityEngine;
using System.Collections;

public class bl_PickUpInstantiator : MonoBehaviour
{
    [SerializeField]
    private GameObject Effect = null;

    public void OnPickUpListener()
    {
        Instantiate(Effect, transform.position, Quaternion.Euler(new Vector3(-90,0,0)));
    }
}