using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkIndentifier : MonoBehaviour
{
    public int chunkId = 0;
    private Vector3 last_position;

    private void Update() 
    {
        if(transform.position != last_position) 
        {
            last_position = transform.position;
            chunkId = GetChunkId(last_position);
            Debug.Log(chunkId);
        }
    }

    private int GetChunkId(Vector3 position) 
    {
        position.x = ((int)(position.x + 50) / 100);
        position.z = ((int)(position.z + 50) / 100);
        return (int)((position.x * 20) + position.z) + 1;
    }
}
