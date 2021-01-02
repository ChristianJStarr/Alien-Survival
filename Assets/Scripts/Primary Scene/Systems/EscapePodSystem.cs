using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapePodSystem : MonoBehaviour
{
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
    private bool systemEnabled = false;

    public PlayerObjectSystem playerObjectSystem;

    //Prefab & Pool
    public GameObject escapePodPrefab;
    private List<GameObject> spawnedPods = new List<GameObject>();

    //Pod Spawnpoints
    private Vector3[] spawnpoints = new Vector3[0];
    private List<Vector3> occupiedSpawnpoints = new List<Vector3>();

    //Start this System
    public bool StartSystem() 
    {
        systemEnabled = true;

        GameObject[] temp = GameObject.FindGameObjectsWithTag("PodSpawnpoint");
        spawnpoints = new Vector3[temp.Length];
        for (int i = 0; i < temp.Length; i++)
        {
            spawnpoints[i] = temp[i].transform.position;
        }

        StartCoroutine(CheckInactivePodsLoop());

        return systemEnabled;
    }

    //Stop this System
    public bool StopSystem()
    {
        systemEnabled = false;

        return systemEnabled;
    }

    //Spawn Player Inside Escape Pod
    public void SpawnPlayerInsideEscapePod(ulong clientId)
    {
        Debug.Log("Attemping to Spawn Player in Escape Pod.");
        EscapePodObject escapePod = SpawnPod(clientId);
        playerObjectSystem.Teleport_ToVector(clientId, escapePod.spawn_Position.position, escapePod.spawn_Position.rotation);
    }

    //Check for Inactive Pods Loop
    private IEnumerator CheckInactivePodsLoop() 
    {
        WaitForSeconds wait = new WaitForSeconds(10);
        while (systemEnabled) 
        {
            if (!systemEnabled) break;
            if (spawnedPods.Count > 0) 
            {
                CheckInactivePods();
            }
            yield return wait;
        }
    }

    //Check for Inactive Pods
    private void CheckInactivePods() 
    {
        Debug.Log("Checking Inactive Pods");
        Vector3[] players = playerObjectSystem.GetPlayerPositionsArray();
        int podCount = spawnedPods.Count;
        if(players.Length > 0) 
        {
            float[] distance = new float[podCount];
            for (int i = 0; i < podCount; i++)
            {
                float minDistance = 2000;
                for (int e = 0; e < players.Length; e++)
                {
                    if (spawnedPods[i] == null) break;
                    float cur = Vector3.Distance(players[e], spawnedPods[i].transform.position);
                    if (minDistance > cur) { minDistance = cur; }
                }
            }
            for (int i = podCount - 1; i >= 0; i--)
            {
                if (distance[i] > 200)
                {
                    Destroy(spawnedPods[i]);
                    Debug.Log("Destoryed Inactive Pod. Distance from Nearest Player: " + distance[i]);
                }
            }
        }
        else 
        {
            for (int i = podCount - 1; i >= 0; i--)
            {
                Destroy(spawnedPods[i]);
                Debug.Log("Destoryed Inactive Pod.");
            }
        }
    }

    //Spawn an Escape Pod
    private EscapePodObject SpawnPod(ulong clientId)
    {
        GameObject pod = Instantiate(escapePodPrefab, GetSpawnpoint(clientId), Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), transform);
        NetworkedObject networkObject = pod.GetComponent<NetworkedObject>();
        networkObject.Spawn();
        spawnedPods.Add(pod);
        return pod.GetComponent<EscapePodObject>();
    }

    //Get Available Spawnpoint
    private Vector3 GetSpawnpoint(ulong clientId) 
    {
        Debug.Log("Getting Spawnpoint for Pod");
        Vector3[] players = playerObjectSystem.GetPlayerPositionsArray(clientId);
        float[] distance = new float[players.Length];
        int spawnpoints_Length = spawnpoints.Length;
        int players_Length = players.Length;
        if(spawnpoints_Length != 0) 
        {
            if (players.Length == 0)
            {
                return spawnpoints[UnityEngine.Random.Range(0, spawnpoints_Length - 1)];
            }
            else
            {
                for (int i = 0; i < spawnpoints_Length; i++)
                {
                    float minDistance = 2000;
                    for (int e = 0; e < players_Length; e++)
                    {
                        float cur = Vector3.Distance(players[e], spawnpoints[i]);
                        if (minDistance > cur) { minDistance = cur; }
                    }
                    distance[i] = minDistance;
                }
                for (int i = 0; i < spawnpoints_Length; i++)
                {
                    if (!occupiedSpawnpoints.Contains(spawnpoints[i]))
                    {
                        if (distance[i] > 500)
                        {
                            occupiedSpawnpoints.Add(spawnpoints[i]);
                            return spawnpoints[i];
                        }
                    }
                }
                for (int i = 0; i < spawnpoints_Length; i++)
                {
                    if (!occupiedSpawnpoints.Contains(spawnpoints[i]))
                    {
                        if (distance[i] > 400)
                        {
                            occupiedSpawnpoints.Add(spawnpoints[i]);
                            return spawnpoints[i];
                        }
                    }
                }
                for (int i = 0; i < spawnpoints_Length; i++)
                {
                    if (!occupiedSpawnpoints.Contains(spawnpoints[i]))
                    {
                        if (distance[i] > 300)
                        {
                            occupiedSpawnpoints.Add(spawnpoints[i]);
                            return spawnpoints[i];
                        }
                    }
                }
                return Vector3.zero;
            }
        }
        else 
        {
            return Vector3.zero;
        }
    }
#endif
}
