using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapePodSystem : MonoBehaviour
{
    private bool systemEnabled = false;

    public PlayerCommandSystem playerCommandSystem;

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
        EscapePodObject escapePod = SpawnPod();
        playerCommandSystem.Teleport_ToVector(clientId, escapePod.spawn_Position.position, escapePod.spawn_Position.rotation);
    }

    //Check for Inactive Pods Loop
    private IEnumerator CheckInactivePodsLoop() 
    {
        WaitForSeconds wait = new WaitForSeconds(10);
        while (systemEnabled) 
        {
            if (!systemEnabled) break;
            CheckInactivePods();
            yield return wait;
        }
    }

    //Check for Inactive Pods
    private void CheckInactivePods() 
    {
        Debug.Log("Checking Inactive Pods");
        Vector3[] players = playerCommandSystem.GetPlayerPositionsArray();
        int podCount = spawnedPods.Count;
        float[] distance = new float[podCount];
        for (int i = 0; i < podCount; i++)
        {
            float minDistance = 2000;
            for (int e = 0; e < players.Length; e++)
            {
                float cur = Vector3.Distance(players[e], spawnpoints[i]);
                if(minDistance > cur) { minDistance = cur; }
            }
        }
        for (int i = podCount - 1; i >= 0; i--)
        {
            if(distance[i] > 200) 
            {
                Destroy(spawnedPods[i]);
                Debug.Log("Destoryed Inactive Pod. Distance from Nearest Player: " + distance[i]);
            }
        }
    }

    //Spawn an Escape Pod
    private EscapePodObject SpawnPod()
    {
        GameObject pod = Instantiate(escapePodPrefab, GetSpawnpoint(), Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), transform);
        spawnedPods.Add(pod);
        return pod.GetComponent<EscapePodObject>();
    }

    //Get Available Spawnpoint
    private Vector3 GetSpawnpoint() 
    {
        Debug.Log("Getting Spawnpoint for Pod");
        //Hyper Inefficient Spawnpoint Finder
        Vector3[] players = playerCommandSystem.GetPlayerPositionsArray();
        float[] distance = new float[players.Length];
        //Get Distance from Players
        for (int i = 0; i < spawnpoints.Length; i++)
        {
            float minDistance = 2000;
            for (int e = 0; e < players.Length; e++)
            {
                float cur = Vector3.Distance(players[e], spawnpoints[i]);
                if(minDistance > cur) { minDistance = cur; }
            }
            distance[i] = minDistance;
        }
        for (int i = 0; i < spawnpoints.Length; i++)
        {
            if (!occupiedSpawnpoints.Contains(spawnpoints[i])) 
            { 
                if(distance[i] > 500) 
                {
                    occupiedSpawnpoints.Add(spawnpoints[i]);
                    return spawnpoints[i];
                }
            }
        }
        for (int i = 0; i < spawnpoints.Length; i++)
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
        for (int i = 0; i < spawnpoints.Length; i++)
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
