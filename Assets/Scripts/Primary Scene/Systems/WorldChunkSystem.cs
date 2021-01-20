using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldChunkSystem : MonoBehaviour
{
    private bool systemEnabled = false;
    private bool validateChunks = false;
    [SerializeField] private Chunk[] chunks;

    public void StartSystem() { systemEnabled = true; }
    public void StopSystem() { systemEnabled = false; }



    #region Editor Chunk Validation
    private void OnValidate()
    {
        if (validateChunks) 
        {
            int inc = 0;
            for (int i = 0; i < 20; i++)
            {
                for (int e = 0; e < 20; e++)
                {
                    chunks[inc].transform.position = new Vector3(i * 100, 0, e * 100);
                    inc++;
                }
            }
        }
    }
    #endregion

}