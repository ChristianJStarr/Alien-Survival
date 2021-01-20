using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldChunkManager : MonoBehaviour
{
    [SerializeField] private Chunk[] chunks;
    private bool validateChunks = false;


    #region Validate Chunks
    private void OnValidate() 
    {
        if (validateChunks) 
        {
            chunks = FindObjectsOfType<Chunk>().ToList().OrderBy(chunk => chunk.chunkId).ToArray();
        }
    }
    #endregion



    private Vector3 last_position;
    private Transform player_object;


    private void Update() 
    {
        if (player_object)
        {
            if (player_object.position != last_position) 
            {
                PlayerMoved();
                last_position = player_object.position;
            }
        }
        else 
        {
            if (LocalPlayerControlObject.GetLocalPlayer())
            {
                player_object = LocalPlayerControlObject.GetLocalPlayer().transform;
            }
        }
    }

    private void PlayerMoved() 
    {
        int chunk = ChunkHelper.GetChunkIdFromPosition(player_object.position);
        int[] near = ChunkHelper.GetNearbyChunkIds(chunk);

        for (int i = 0; i < chunks.Length; i++)
        {
            bool showChunk = false;
            for (int e = 0; e < near.Length; e++)
            {
                if(chunks[i].chunkId == near[e]) 
                {
                    showChunk = true;
                    break;
                }
            }
            chunks[i].gameObject.SetActive(showChunk);
        }
    }

}
