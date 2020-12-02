
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
    public SpawnpointObject[] spawnpoints;
    private Vector3[] spawnpoint_positions = new Vector3[0];
    private int spawnpoints_count = 0;

    private int c_ObjectLoadDistance = 1000;
    CalculateNearbyJob calculateNearbyJob;
    JobHandle calculateNearbyHandle;
    NativeArray<Vector3> positionsNative;
    NativeArray<bool> isNearbyNative;

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
        spawnpoints = FindObjectsOfType<SpawnpointObject>();
        spawnpoints_count = spawnpoints.Length;
        spawnpoint_positions = new Vector3[spawnpoints_count];
        for (int i = 0; i < spawnpoints_count; i++)
        {
            spawnpoint_positions[i] = spawnpoints[i].transform.position;
        }
    }

    //Client Side Update World Objects
    public void UpdateWorldObjects(Snapshot_WorldObject[] snapshot, Vector3 player_position) 
    {
        for (int i = 0; i < spawnpoints.Length; i++)
        {
            for (int x = 0; x < snapshot.Length; x++)
            {
                if(spawnpoints[i].spawn_id == snapshot[x].spawnId) 
                {
                    int targetObject = snapshot[x].objectId;
                    if(spawnpoints[i].spawn_objectId != targetObject) 
                    {
                        if(targetObject == 0) // Turn OFF
                        {
                            spawnpoints[i].worldObject.SetActive(false);
                        }
                        else //Turn ON 
                        {
                            if(spawnpoints[i].spawn_objectId != 0 && spawnpoints[i].worldObject != null) 
                            {
                                spawnpoints[i].worldObject.SetActive(false);
                            }
                            //Turn On TargetObject
                            spawnpoints[i].worldObject = SpawnWorldObject(targetObject, spawnpoint_positions[i]);
                        }
                        spawnpoints[i].spawn_objectId = targetObject;
                    }
                    break;
                }
            }
        }
        TurnOffFarAwayWorldObjects(player_position);
    }

    //Turn Off Far Away World Object Spawnpoints
    private void TurnOffFarAwayWorldObjects(Vector3 player_position) 
    {
        positionsNative = new NativeArray<Vector3>(spawnpoint_positions, Allocator.TempJob);
        isNearbyNative = new NativeArray<bool>(spawnpoints_count, Allocator.TempJob);
        calculateNearbyJob = new CalculateNearbyJob()
        {
            positions = positionsNative,
            isNearby = isNearbyNative,
            distance = c_ObjectLoadDistance,
            current = player_position
        };
        calculateNearbyHandle = calculateNearbyJob.Schedule(spawnpoints_count, calculateNearbyHandle);
        calculateNearbyHandle.Complete();
        for (int i = 0; i < spawnpoints_count; i++)
        {
            if (!isNearbyNative[i] && spawnpoints[i].spawn_objectId != 0)
            {
                spawnpoints[i].spawn_objectId = 0;
                if(spawnpoints[i].worldObject != null) 
                {
                    spawnpoints[i].worldObject.SetActive(false);
                }
            }
        }
        positionsNative.Dispose();
        isNearbyNative.Dispose();
    }

    //Spawn a World Object
    private GameObject SpawnWorldObject(int objectId, Vector3 position) 
    {
        float randomY = Random.Range(0, 100);
        return PooledManager.InstantiatePooledObject(WorldObjectDataManager.GetPrefabFromObjectId(objectId), position, Quaternion.Euler(0,randomY,0));
    }
}



public struct CalculateNearbyJob : IJobFor 
{
    public NativeArray<Vector3> positions;
    public NativeArray<bool> isNearby;
    public int distance;
    public Vector3 current;

    public void Execute(int i) 
    {
        if(Vector3.Distance(positions[i], current) < distance) 
        {
            isNearby[i] = true;
        }
    }
}





// ID  TREES
// 1  bush      type 1
// 2  sapling   type 1
// 3  tree01    type 1 
// 4  tree02    type 1
// 5  tree03    type 1