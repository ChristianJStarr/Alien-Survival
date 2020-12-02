using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class WorldObjectSystem : MonoBehaviour
{
    private bool systemEnabled = false;
    
    //NetworkingManager
    private NetworkingManager networkingManager;

    //World Object Lists
    private Dictionary<int, WorldObject> worldObjects = new Dictionary<int, WorldObject>();

    //-----Configuration-----
    public int c_TreeMaxAmount = 1500;
    public int c_RockMaxAmount = 0;
    public int c_LootMaxAmount = 0;
    public int c_TreeRespawnTime = 60; //Seconds
    public int c_RockRespawnTime = 60; //Seconds
    public int c_LootRespawnTime = 60; //Seconds

    //-----Spawnpoint Objects------
    private List<int> occupiedSpawnpoints = new List<int>();
    [HideInInspector] public SpawnpointObject[] spawnpoints;
    private Vector3[] spawnpointLocations;
    private int spawnpointsLength = 0;


    //-----Counters-----
    private int currentTrees = 0;
    private int currentRocks = 0;
    private int currentLoot = 0;


    //-----------------------------------------------------------------//
    //              World Object System Primary Functions              //
    //-----------------------------------------------------------------//

    //Start System
    public bool StartSystem()
    {
        //Load world Objects
        networkingManager = NetworkingManager.Singleton;
        //Load Spawnpoints
        systemEnabled = LoadSpawnpoints();
        //Start Respawner Loop
        StartCoroutine(SpawnObjectLoop());
        return systemEnabled;
    }

    //Stop System
    public bool StopSystem()
    {
        systemEnabled = false;
        return true;
    }

    //Load Saved Objects
    private bool LoadSpawnpoints()
    {
        spawnpoints = FindObjectsOfType<SpawnpointObject>();
        spawnpointsLength = spawnpoints.Length;
        spawnpointLocations = new Vector3[spawnpointsLength];
        for (int i = 0; i < spawnpointsLength; i++)
        {
            spawnpointLocations[i] = spawnpoints[i].transform.position;
        }
        if(spawnpointsLength > 0) 
        {
            return true;
        }
        return false;
    }

    //Save Loaded Objects
    public void AutoSave()
    {
        //Save all world objects
    }

    //Get Array of All World Objects
    public Snapshot_WorldObject[] GetWorldObjectsSnapshot() 
    {
        Snapshot_WorldObject[] snap = new Snapshot_WorldObject[spawnpointsLength];
        for (int i = 0; i < spawnpointsLength; i++)
        {
            snap[i] = new Snapshot_WorldObject()
            {
                objectId = spawnpoints[i].spawn_objectId,
                spawnId = spawnpoints[i].spawn_id,
                location = spawnpointLocations[i]
            };
        }
        return snap;
    }
    


    //-----------------------------------------------------------------//
    //                    World Object Spawning                        //
    //-----------------------------------------------------------------//

    private IEnumerator SpawnObjectLoop() 
    {
        WaitForSeconds wait = new WaitForSeconds(120);
        while (systemEnabled) 
        {
            CheckSpawnpoints();
            yield return wait;
        }
    }

    //Look for Available Spawnpoints
    private void CheckSpawnpoints()
    {
        float networkTime = networkingManager.NetworkTime;
        for (int i = 0; i < spawnpoints.Length; i++)
        {
            SpawnpointObject spawn = spawnpoints[i];
            if ((spawn.lastSpawntime == 0 || networkTime - spawn.lastSpawntime > GetRespawnTime(spawn.spawn_type)) && !occupiedSpawnpoints.Contains(spawn.spawn_id))
            {
                if (GetCurrentAmount(spawn.spawn_type) < GetMaxAmount(spawn.spawn_type))
                {
                    float randomY = 5;
                    GameObject worldObject = PooledManager.InstantiatePooledObject(WorldObjectDataManager.GetPrefabFromSpawnpointData(spawn.spawn_type, spawn.spawn_level), spawn.transform.position, Quaternion.Euler(0,randomY,0));
                    if (worldObject != null)
                    {
                        WorldObject world = worldObject.GetComponent<WorldObject>();
                        if (world != null) 
                        {
                            WorldObjectData objectData = WorldObjectDataManager.GetWorldObjectDataById(world.objectDataId);
                            if(objectData != null)
                            {
                                occupiedSpawnpoints.Add(spawn.spawn_id);
                                AddCurrentAmount(spawn.spawn_type);
                                spawnpoints[i].worldObject = worldObject;
                                world.objectAmount = objectData.gatherAmount;
                                world.spawn_Id = spawn.spawn_id;
                                spawnpoints[i].spawn_objectId = world.objectDataId;
                                worldObjects.Add(world.spawn_Id, world);
                            }
                            else { worldObject.SetActive(false); }
                        }
                        else { worldObject.SetActive(false); }
                    }
                    else { worldObject.SetActive(false); }
                }
            }
        }
    }

    //Remove World Object
    private void RemoveWorldObject(int spawnpoint_Id) 
    {
        if (worldObjects.ContainsKey(spawnpoint_Id)) 
        {
            if (occupiedSpawnpoints.Contains(spawnpoint_Id)) 
            {
                occupiedSpawnpoints.Remove(spawnpoint_Id);
            }
            spawnpoints[spawnpoint_Id].lastSpawntime = networkingManager.NetworkTime;
            worldObjects[spawnpoint_Id].gameObject.SetActive(false);
            worldObjects.Remove(spawnpoint_Id);
        }
    }

    //Get Respawn Time from Spawn Type
    private int GetRespawnTime(int spawnType)
    {
        if (spawnType == 1)
        {
            return c_TreeRespawnTime;
        }
        else if (spawnType == 2)
        {
            return c_RockRespawnTime;
        }
        else if (spawnType == 3)
        {
            return c_LootRespawnTime;
        }
        return 999999;
    }
    //Get Max Amount from Spawn Type
    private int GetMaxAmount(int spawnType)
    {
        if (spawnType == 1)
        {
            return c_TreeMaxAmount;
        }
        else if (spawnType == 2)
        {
            return c_RockMaxAmount;
        }
        else if (spawnType == 3)
        {
            return c_LootMaxAmount;
        }
        return 0;
    }
    //Get Max Amount from Spawn Type
    private int GetCurrentAmount(int spawnType)
    {
        if (spawnType == 1)
        {
            return currentTrees;
        }
        else if (spawnType == 2)
        {
            return currentRocks;
        }
        else if (spawnType == 3)
        {
            return currentLoot;
        }
        return 999999;
    }
    //Add to Current Amount
    private void AddCurrentAmount(int spawnType) 
    {
        if (spawnType == 1)
        {
            currentTrees++;
        }
        else if (spawnType == 2)
        {
            currentRocks++;
        }
        else if (spawnType == 3)
        {
            currentLoot++;
        }
    }
}




//Tools Below for List Scrambling
public static class ThreadSafeRandom
{
    [ThreadStatic] private static System.Random Local;

    public static System.Random ThisThreadsRandom
    {
        get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
    }
}
static class MyExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }


    
}