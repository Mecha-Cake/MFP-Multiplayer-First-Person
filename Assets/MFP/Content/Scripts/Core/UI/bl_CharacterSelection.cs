using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_CharacterSelection : MonoBehaviour
{

    [SerializeField] private GameObject RowPrefab = null;
    [SerializeField] private Transform Panel = null;

    private List<bl_CharacterRowUI> cacheList = new List<bl_CharacterRowUI>();
    private bl_PlayerModel[] PlayerModels;

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        PlayerModels = bl_GameData.Instance.PlayerPrefab.GetComponentInChildren<bl_PlayerAnimator>().PlayerModels;
        InstanceRows();
    }

    /// <summary>
    /// 
    /// </summary>
    void InstanceRows()
    {
        for (int i = 0; i < PlayerModels.Length; i++)
        {
            GameObject g = Instantiate(RowPrefab) as GameObject;
            g.transform.SetParent(Panel, false);
            bl_CharacterRowUI ui = g.GetComponent<bl_CharacterRowUI>();
            ui.Set(PlayerModels[i], (i == bl_GameData.Instance.LocalPlayerModel), i, this);
            cacheList.Add(ui);
        }
    }

    public void OnSelectCharacter(int ID)
    {
        bl_GameData.Instance.LocalPlayerModel = ID;
        PlayerPrefs.SetInt(PropertiesKeys.LocalCharacter, ID);
        for (int i = 0; i < cacheList.Count; i++)
        {
            cacheList[i].SelectedUI.SetActive(false);
        }
        cacheList[ID].SelectedUI.SetActive(true);
    }
}