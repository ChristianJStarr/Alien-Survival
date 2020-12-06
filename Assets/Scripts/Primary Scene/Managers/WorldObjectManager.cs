using MLAPI;
using System.Collections.Generic;
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

    //Transforms
    public Transform tree_parent;
    private Transform local_Player;
    
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


    private void Start() 
    {
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsServer) return;
        //Get Configuration
        c_ObjectLoadDistance = settings.objectDistance;

        //Find Spawnpoints
        spawnpoints = WorldObjectSpawnpointManager.GetSpawnpoints();
        spawnpoints_count = spawnpoints.Length;
        Debug.Log(spawnpoints_count);
        //Get Positions
        spawnpoint_positions = new Vector3[spawnpoints_count];
        for (int i = 0; i < spawnpoints_count; i++)
        {
            spawnpoint_positions[i] = spawnpoints[i].transform.position;
        }
    }

    //Client Side Update World Objects
    public void UpdateWorldObjects(Snapshot_WorldObject[] snapshot) 
    {
        if (spawnpoints_count == 0) return;
        for (int x = 0; x < snapshot.Length; x++)
        {
            int spawn_id = snapshot[x].spawnId - 1;
            if (spawnpoints[spawn_id] != null)
            {
                spawnpoints[spawn_id].spawn_objectId = snapshot[x].objectId;
            }
        }
        SpawnpointLogic();
    }

    //Spawnpoint Logic
    private void SpawnpointLogic()
    {
        if (local_Player == null) local_Player = WorldSnapshotManager.Singleton.GetLocalPlayerObject().transform;
        //Calculate Nearby Spawnpoints
        positionsNative = new NativeArray<Vector3>(spawnpoint_positions, Allocator.TempJob);
        farAwayNative = new NativeArray<bool>(spawnpoints_count, Allocator.TempJob);
        calculateNearbyJob = new CalculateNearbyJob()
        {
            positions = positionsNative,
            farAway = farAwayNative,
            distanceSqr = c_ObjectLoadDistance * c_ObjectLoadDistance,
            current = local_Player.position
        };
        calculateNearbyHandle = calculateNearbyJob.Schedule(spawnpoints_count, calculateNearbyHandle);
        calculateNearbyHandle.Complete();
        //Destroy Far Away Objects
        for (int i = 0; i < spawnpoints_count; i++)
        {
            if (farAwayNative[i])
            {
                //Object is far away
                if (spawnpoints[i].spawn_objectId != 0 && spawnpoints[i].worldGameObject != null) //Has world object
                {
                    DestroyObject(spawnpoints[i].spawn_objectId, spawnpoints[i].worldGameObject); //Pool
                    spawnpoints[i].worldGameObject = null;
                }
            }
            else  
            {
                //Object is Nearby
                if (spawnpoints[i].spawn_objectId != 0 && spawnpoints[i].worldGameObject == null) //Has world object
                {
                    spawnpoints[i].worldGameObject = SpawnObject(spawnpoints[i].spawn_objectId, spawnpoints[i].transform.position); //Pool
                }
            }
        }
        //Dispose of Job NativeArrays
        positionsNative.Dispose();
        farAwayNative.Dispose();
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
            instance = Instantiate(WorldObjectDataManager.GetPrefabFromObjectId(objectId), position, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0), tree_parent);   
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