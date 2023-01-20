using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_CharacterRowUI : MonoBehaviour
{

    [SerializeField] private Text NameText = null;
    [SerializeField] private Image PreviewImg = null;
    public GameObject SelectedUI;

    private bl_CharacterSelection Manager;
    private int ID;

    public void Set(bl_PlayerModel model, bool selected, int id, bl_CharacterSelection manager)
    {
        ID = id;
        NameText.text = model.ModelName.ToUpper();
        PreviewImg.sprite = model.Preview;
        SelectedUI.SetActive(selected);
        Manager = manager;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnSelec()
    {
        Manager.OnSelectCharacter(ID);
    }
}