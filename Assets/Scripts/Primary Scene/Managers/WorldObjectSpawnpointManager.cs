using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldObjectSpawnpointManager : MonoBehaviour
{
    public static WorldObjectSpawnpointManager Singleton;
    [SerializeField] private SpawnpointObject[] spawnpoints;
    public int spawnpoints_Count = 0;


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
                temp[i].gameObject.name = "WorldObject_Spawnpoint_" + (i + 1);
                objectId++;
                if (objectId >= 6) { objectId = 1; }
            }
            Array.Sort(temp, delegate (SpawnpointObject x, SpawnpointObject y) { return x.spawn_id.CompareTo(y.spawn_id); });
            spawnpoints = temp;
            spawnpoints_Count = temp.Length;
            Debug.Log(temp.Length + " Spawnpoints Registered.");
        }
        if(spawnpoints != null) 
        {
            spawnpoints_Count = spawnpoints.Length;
        }
    }


    private void Start() 
    {
    
    
    }



#endif
    #endregion


    private void Awake()
    {
        Singleton = this; 
    }
    public static SpawnpointObject[] GetSpawnpoints() 
    {
        if(Singleton != null) 
        {
            return Singleton.spawnpoints;
        }
        return new SpawnpointObject[0];
    }
}
