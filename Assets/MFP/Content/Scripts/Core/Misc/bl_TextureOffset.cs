using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_TextureOffset : MonoBehaviour
{

    [Range(0.1f, 1)] public float Speed = 0.2f;
    [SerializeField] private Material Panner = null;
    private Vector2 Offset;

    private void Start()
    {
        Offset = Panner.GetTextureOffset("_MainTex");
    }

    private void Update()
    {
        if (Panner == null)
            return;

        Offset.x -= Time.deltaTime * Speed;
        Panner.SetTextureOffset("_MainTex", Offset);
    }
}