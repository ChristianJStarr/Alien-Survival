
using MLAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class WorldObjectManager : MonoBehaviour
{
    #region Singleton
    public static WorldObjectManager Singleton;
    private void Awake() { Singleton = this; }
    #endregion

    //Settings
    public Settings settings; 

    //Spawnpoints
    private SpawnpointObject[] spawnpoints;
    private Vector3[] spawnpoint_positions = new Vector3[0];
    private int spawnpoints_count = 0;

    //Object Pooling
    private Dictionary<int, Queue<GameObject>> objectPool = new Dictionary<int, Queue<GameObject>>();

    //Jobs
    CalculateNearbyJob calculateNearbyJob;
    JobHandle calculateNearbyHandle;
    NativeArray<Vector3> positionsNative;
    NativeArray<bool> farAwayNative;
    
    //Configuration
    private int c_ObjectLoadDistance = 0;



    //Settings Menu Changed Event
    void OnEnable() 
    {
        SettingsMenu.ChangedSettings += Change;
    }
    void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;
    }
    public void Change() 
    {
        c_ObjectLoadDistance = settings.objectDistance;
    }



    #region Spawnpoint Finder
#if UNITY_EDITOR
    private bool updateSpawnpoints = false;
    private void OnValidate()
    {
        if (updateSpawnpoints)
        {
            int objectId = 1;
            SpawnpointObject[] temp = FindObjectsOfType<SpawnpointObject>();
            List<SpawnpointObject> tempList = temp.ToList();
            tempList.Shuffle();
            temp = tempList.ToArray();
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i].spawn_id = i + 1;
                temp[i].spawn_objectId = objectId;
                objectId++;
                if(objectId >= 6) { objectId = 1; }
            }
        }
    }
#endif
    #endregion


    private void Start() 
    {
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsServer) return;
        //Get Configuration
        c_ObjectLoadDistance = settings.objectDistance;

        //Find Spawnpoints
        spawnpoints = FindObjectsOfType<SpawnpointObject>();
        spawnpoints_count = spawnpoints.Length;

        //Reorder ID to Index        
        Array.Sort(spawnpoints, delegate (SpawnpointObject x, SpawnpointObject y) { return x.spawn_id.CompareTo(y.spawn_id);});

        //Get Positions
        spawnpoint_positions = new Vector3[spawnpoints_count];
        for (int i = 0; i < spawnpoints_count; i++)
        {
            spawnpoint_positions[i] = spawnpoints[i].transform.position;
        }
    }


    //Client Side Update World Objects
    public void UpdateWorldObjects(Snapshot_WorldObject[] snapshot, Vector3 player_position) 
    {
        //Calculate Nearby Spawnpoints
        positionsNative = new NativeArray<Vector3>(spawnpoint_positions, Allocator.TempJob);
        farAwayNative = new NativeArray<bool>(spawnpoints_count, Allocator.TempJob);
        calculateNearbyJob = new CalculateNearbyJob()
        {
            positions = positionsNative,
            farAway = farAwayNative,
            distanceSqr = c_ObjectLoadDistance * c_ObjectLoadDistance,
            current = player_position
        };
        calculateNearbyHandle = calculateNearbyJob.Schedule(spawnpoints_count, calculateNearbyHandle);
        calculateNearbyHandle.Complete();
        
        //Destroy Far Away Objects
        for (int i = 0; i < spawnpoints_count; i++)
        {
            spawnpoints[i].isNearPlayer = !farAwayNative[i];
            if (farAwayNative[i])//Object is far away
            {
                if(spawnpoints[i].spawn_objectId != 0) 
                {
                    spawnpoints[i].spawn_objectId = 0;
                    if (spawnpoints[i].worldObject != null)
                    {
                        DestroyObject(spawnpoints[i].spawn_objectId, spawnpoints[i].worldObject);
                        spawnpoints[i].worldObject = null;
                    }
                }
            }
        }

        //Read Snapshot Data
        for (int x = 0; x < snapshot.Length; x++)
        {
            //Apply Data to SpawnID
            SpawnpointLogic(snapshot[x].spawnId, snapshot[x].objectId);
        }

        //Dispose of Job NativeArrays
        positionsNative.Dispose();
        farAwayNative.Dispose();
    }

    //Spawnpoint Logic
    private void SpawnpointLogic(int spawn_id, int objectId)
    {
        spawn_id -= 1;
        if (spawnpoints[spawn_id] != null && spawnpoints[spawn_id].isNearPlayer && spawnpoints[spawn_id].spawn_objectId != objectId)
        {
            if (spawnpoints[spawn_id].worldObject != null)
            {
                DestroyObject(spawnpoints[spawn_id].spawn_objectId, spawnpoints[spawn_id].worldObject);
                spawnpoints[spawn_id].worldObject = null;
            }
            spawnpoints[spawn_id].spawn_objectId = objectId;
            if (objectId != 0) 
            {
                spawnpoints[spawn_id].worldObject = SpawnObject(objectId, spawnpoint_positions[spawn_id]);
            }
        }
    }

    //Spawn Object
    private GameObject SpawnObject(int objectId, Vector3 position) 
    {
        GameObject instance = null;
        if (objectPool.ContainsKey(objectId) && objectPool[objectId].Count != 0) 
        {
            instance = objectPool[objectId].Dequeue();
            instance.SetActive(true);
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
        }
        else 
        {
            instance = Instantiate(WorldObjectDataManager.GetPrefabFromObjectId(objectId), position, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0));   
        }
        return instance;
    }

    //Destroy Object
    private void DestroyObject(int objectId, GameObject obj) 
    {
        obj.SetActive(false);
        if (objectPool.ContainsKey(objectId)) 
        {
            objectPool[objectId].Enqueue(obj);
        }
        else 
        {
            objectPool.Add(objectId, new Queue<GameObject>());
            objectPool[objectId].Enqueue(obj);
        }
    }
}



public struct CalculateNearbyJob : IJobFor 
{
    [ReadOnly]public NativeArray<Vector3> positions;
    public NativeArray<bool> farAway;
    [ReadOnly] public int distanceSqr;
    [ReadOnly] public Vector3 current;

    public void Execute(int i) 
    {
        if((positions[i] - current).sqrMagnitude > distanceSqr) 
        {
            farAway[i] = true;
        }
        else 
        {
            farAway[i] = false;
        }
    }
}





// ID  TREES
// 1  bush      type 1
// 2  sapling   type 1
// 3  tree01    type 1 
// 4  tree02    type 1
// 5  tree03    type 1