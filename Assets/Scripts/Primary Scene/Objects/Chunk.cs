using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField]public int chunkId;


    private MeshRenderer renderer;

    private void Start() 
    {
        renderer = GetComponent<MeshRenderer>();
        renderer.material.color = Random.ColorHSV();
    }
}
