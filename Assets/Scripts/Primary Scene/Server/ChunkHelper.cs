using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHelper : MonoBehaviour
{
    public static int GetChunkIdFromPosition(Vector3 position, int chunkWidth = 100, int chunkCount = 20)
    {
        position.x = ((int)(position.x + (chunkWidth / 2)) / chunkWidth);
        position.z = ((int)(position.z + (chunkWidth / 2)) / chunkWidth);
        return (int)((position.x * chunkCount) + position.z) + 1;
    }

    public static int[] GetNearbyChunkIds(int chunk, int chunkCount = 20)
    {
        List<int> near = new List<int>();
        near.Add(chunk);
        near.Add(chunk + 1);
        near.Add(chunk - 1);
        near.Add(chunk + chunkCount);
        near.Add(chunk - chunkCount);
        near.Add(chunk + chunkCount + 1);
        near.Add(chunk + chunkCount - 1);
        near.Add(chunk - chunkCount + 1);
        near.Add(chunk - chunkCount - 1);
        for (int i = 0; i < near.Count; i++)
        {
            if (near[i] > 400 || near[i] < 1)
            {
                near.RemoveAt(i);
            }
        }
        return near.ToArray();
    }
}
