using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WorldObjectSystem : MonoBehaviour
{
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
    private bool systemEnabled = false;
    
    //NetworkingManager
    private NetworkingManager networkingManager;

    //World Object Lists    Spawn_Id  / WorldObject
    private Dictionary<int, WorldObject> worldObjects = new Dictionary<int, WorldObject>();
    //World Object Pool     Object_Id / GameObject
    private Dictionary<int, Queue<WorldObject>> objectPool = new Dictionary<int, Queue<WorldObject>>();

    //-----Configuration-----
    private int c_TreeMaxAmount = 0;
    private int c_RockMaxAmount = 0;
    private int c_LootMaxAmount = 0;
    private int c_TreeRespawnTime = 0;
    private int c_RockRespawnTime = 0;
    private int c_LootRespawnTime = 0;
    private int c_SingleSyncPerShot = 1;

    //-----Spawnpoint Objects------
    private List<int> changedSpawnpoints = new List<int>();
    private List<int> occupiedSpawnpoints = new List<int>();
    private SpawnpointObject[] spawnpoints;
    private Vector3[] spawnpointLocations;
    private int spawnpointsLength = 0;
    private int singleSyncInterval = 0;

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
        //Load Properties
        LoadServerProperties();
        
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
        spawnpoints = WorldObjectSpawnpointManager.GetSpawnpoints();
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

    //Get Array of Changed World Objects
    public Snapshot_WorldObject[] GetWorldObjectsSnapshot() 
    {
        for (int i = 0; i < c_SingleSyncPerShot; i++)
        {
            AddSingleSyncSnapshot();
        }
        List<Snapshot_WorldObject> temp = new List<Snapshot_WorldObject>();
        for (int i = changedSpawnpoints.Count - 1; i >= 0; i--)
        {
            temp.Add(new Snapshot_WorldObject()
            {
                chunkId = ChunkHelper.GetChunkIdFromPosition(spawnpoints[i].transform.position),
                objectId = spawnpoints[i].spawn_objectId,
                spawnId = spawnpoints[i].spawn_id
            });
            changedSpawnpoints.RemoveAt(i);
        }
        return temp.ToArray();
    }

    //Get Array of All World Objects
    public Snapshot_WorldObject[] GetAllWorldObjectsSnapshot() 
    {
        Snapshot_WorldObject[] instance = new Snapshot_WorldObject[spawnpointsLength];
        for (int i = 0; i < spawnpointsLength; i++)
        {
            instance[i] = new Snapshot_WorldObject()
            {
                objectId = spawnpoints[i].spawn_objectId,
                spawnId = spawnpoints[i].spawn_id
            };
        }
        return instance;
    }

    //Get Array of Some World Objects (Slow Sync)
    public void AddSingleSyncSnapshot() 
    {
        if(singleSyncInterval >= spawnpointsLength) 
        {
            singleSyncInterval = 0;
        }
        if (!changedSpawnpoints.Contains(singleSyncInterval)) 
        {
            changedSpawnpoints.Add(singleSyncInterval);
        }
        singleSyncInterval++;
    }
    
    //Load Configuration from Server Properties
    private void LoadServerProperties() 
    {
        ServerProperties sp = ServerConnect.singleton.GetServerProperties();
        c_TreeMaxAmount = sp.wo_maxTree;
        c_RockMaxAmount = sp.wo_maxRock;
        c_LootMaxAmount = sp.wo_maxLoot;
        c_TreeRespawnTime = sp.wo_respawnTrees; 
        c_RockRespawnTime = sp.wo_respawnRocks;
        c_LootRespawnTime = sp.wo_respawnLoot;
    }



    //-----------------------------------------------------------------//
    //                    World Object Spawning                        //
    //-----------------------------------------------------------------//

    //Loop for Checking Spawnpoints
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
            if ((spawn.lastSpawntime == 0 || networkTime - spawn.lastSpawntime > GetRespawnTime(spawn.spawn_type)) && !occupiedSpawnpoints.Contains(spawn.spawn_id) && GetCurrentAmount(spawn.spawn_type) < GetMaxAmount(spawn.spawn_type))
            {
                WorldObject worldObject = SpawnObject(spawn.spawn_type, spawn.spawn_level, spawn.spawn_id, spawn.transform.position);
                if (worldObject != null)
                {
                    spawnpoints[i].spawn_objectId = worldObject.objectDataId;
                }
            }
        }
    }

    //Spawn Object
    private WorldObject SpawnObject(int spawn_type, int spawn_level, int spawn_id, Vector3 position)
    {
        WorldObjectData data = WorldObjectDataManager.GetRandomWorldData(spawn_type, spawn_level);
        WorldObject instance = null;

        if (objectPool.ContainsKey(data.objectId) && objectPool[data.objectId].Count > 0) 
        {
            instance = objectPool[data.objectId].Dequeue();
            instance.gameObject.SetActive(true);
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
        }
        else 
        {
            instance = Instantiate(data.objectPrefab, position, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0)).GetComponent<WorldObject>();
        }

        instance.objectAmount = data.gatherAmount;
        instance.spawn_Id = spawn_id;
        worldObjects.Add(spawn_id, instance);
        occupiedSpawnpoints.Add(spawn_id);
        AddCurrentAmount(spawn_type);
        return instance;
    }

    //Destroy Object
    private int DepleteObject(int spawnpoint_Id, int deplete_amount)
    {
        WorldObject worldObject = spawnpoints[spawnpoint_Id].worldObject;
        if (worldObject == null) return 0;
        if(deplete_amount > 0) 
        {
            if(worldObject.objectAmount > deplete_amount) 
            {
                worldObject.objectAmount -= deplete_amount;
                return deplete_amount;
            }
            else if(worldObject.objectAmount == deplete_amount) 
            {
                worldObject.gameObject.SetActive(false);
                int objectId = worldObject.objectDataId;
                SubtractCurrentAmount(spawnpoints[spawnpoint_Id].spawn_type);
                if (occupiedSpawnpoints.Contains(spawnpoint_Id))
                {
                    occupiedSpawnpoints.Remove(spawnpoint_Id);
                }
                if (objectPool.ContainsKey(objectId))
                {
                    objectPool[objectId].Enqueue(worldObject);
                }
                else
                {
                    objectPool.Add(objectId, new Queue<WorldObject>());
                    objectPool[objectId].Enqueue(worldObject);
                }
                if (worldObjects.ContainsKey(spawnpoint_Id))
                {
                    spawnpoints[spawnpoint_Id].lastSpawntime = networkingManager.NetworkTime;
                    worldObjects.Remove(spawnpoint_Id);
                }
                return deplete_amount;
            }
            else 
            {
                worldObject.gameObject.SetActive(false);
                int objectId = worldObject.objectDataId;
                SubtractCurrentAmount(spawnpoints[spawnpoint_Id].spawn_type);
                if (occupiedSpawnpoints.Contains(spawnpoint_Id))
                {
                    occupiedSpawnpoints.Remove(spawnpoint_Id);
                }
                if (objectPool.ContainsKey(objectId))
                {
                    objectPool[objectId].Enqueue(worldObject);
                }
                else
                {
                    objectPool.Add(objectId, new Queue<WorldObject>());
                    objectPool[objectId].Enqueue(worldObject);
                }
                if (worldObjects.ContainsKey(spawnpoint_Id))
                {
                    spawnpoints[spawnpoint_Id].lastSpawntime = networkingManager.NetworkTime;
                    worldObjects.Remove(spawnpoint_Id);
                }
                return worldObject.objectAmount;
            }
        }
        else 
        {
            return 0;
        }
    }




    //-----------------------------------------------------------------//
    //                    World Object Info                            //
    //-----------------------------------------------------------------//


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
    //Subtract from Current Amount
    private void SubtractCurrentAmount(int spawnType)
    {
        if (spawnType == 1)
        {
            currentTrees--;
            if (currentTrees < 0) { currentTrees = 0; }
        }
        else if (spawnType == 2)
        {
            currentRocks--;
            if (currentRocks < 0) { currentRocks = 0; }
        }
        else if (spawnType == 3)
        {
            currentLoot--;
            if (currentLoot < 0) { currentLoot = 0; }
        }
    }
#endif
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