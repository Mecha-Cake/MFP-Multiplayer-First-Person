using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class bl_PickUpManager : bl_PhotonHelper
{
    [Separator("Spawneables")]
    /// <summary>
    /// Register all spawneable objects
    /// </summary>
    public List<GameObject> Spawneables = new List<GameObject>();
    [Separator("References")]
    [SerializeField]private Animator PickUpNotifier = null;
    [SerializeField]private Text PickUpText = null;

    private static PhotonView m_View;
    private Dictionary<string, GameObject> cacheItems = new Dictionary<string, GameObject>();

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        m_View = PhotonView.Get(this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    public void DestroyItem(GameObject objname)
    {
        m_View.RPC("RpcDestroyItem", RpcTarget.All,objname.name);
    }
   
    /// <summary>
    /// 
    /// </summary>
    /// <param name="SpawneableID"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public static void InstantiateItem(int SpawneableID,Vector3 position,Quaternion rotation)
    {
        string uniqueName = string.Format("Pickup [{0}]", bl_CoopUtils.GetGuid);
        m_View.RPC("RpcInstantiate", RpcTarget.All, SpawneableID, uniqueName, position, rotation);
    }

    [PunRPC]
    void RpcInstantiate(int spawneableID,string uniqueName,Vector3 position,Quaternion rotation)
    {
        GameObject item = Instantiate(Spawneables[spawneableID], position, rotation) as GameObject;
        item.name = uniqueName;
        cacheItems.Add(uniqueName, item);
    }

    [PunRPC]
    void RpcDestroyItem(string _name)
    {
        GameObject item = FindItem(_name);
        if (item != null)
        {
            Destroy(item);
        }
        else
        {
            Debug.LogWarning("Item with id: " + _name + " not exist in scene!");
        }
    }

    [PunRPC]
    void RpcInvokeListerners(string item)
    {
        if (cacheItems.ContainsKey(item))
        {
            cacheItems[item].GetComponent<bl_PickUpItem>().InvokeLocal();
        }
        else
        {
            Debug.LogWarning("The item: " + item + " not exist or has ben destroyed!");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPickUp(string itemName,string objname, bool invokeInServer)
    {
        if (PickUpText)
        {
            PickUpText.text = string.Format("PickUp <color=#FF0066>{0}</color>", itemName);
        }
        if (PickUpNotifier)
        {
            PickUpNotifier.Play("pickup", 0, 0);
        }
        if (invokeInServer)
        {
            m_View.RPC("RpcInvokeListerners", RpcTarget.Others, objname);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CacheThis(GameObject obj)
    {
        string n = obj.name;
        if (!cacheItems.ContainsKey(n))
        {
            cacheItems.Add(n, obj);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject FindItem(string name)
    {
        GameObject g = null;
        if (cacheItems.ContainsKey(name))
        {
            g = cacheItems[name];
        }
        return g;
    }
}