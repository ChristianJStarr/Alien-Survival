using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



public class WorldObjectDataManager : MonoBehaviour
{
    #region Singleton
    public static WorldObjectDataManager Singleton;
    private void Awake()
    {
        Singleton = this;
    }
    #endregion
    
    [SerializeField] private WorldObjectData[] worldObjectData;
    private bool forceSerialize = true;

    #region Auto-Serialize Data
#if UNITY_EDITOR
    private void OnValidate()
    {
        string[] guids = AssetDatabase.FindAssets("t:WorldObjectData", new[] { "Assets/Content/WorldObjectData" });
        int count = guids.Length;
        if (worldObjectData != null && worldObjectData.Length == count && !forceSerialize) return;
        worldObjectData = new WorldObjectData[count];
        for (int n = 0; n < count; n++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[n]);
            worldObjectData[n] = AssetDatabase.LoadAssetAtPath<WorldObjectData>(path);
        }
        Debug.Log("Successfully Updated WorldObjectData. Count: " + count);
    }
#endif
    #endregion


    //Get World Object Data by ID
    public static WorldObjectData GetWorldObjectDataById(int id) 
    {
        if(Singleton != null) 
        {
            return Singleton.GetWorldObjectDataByIdTask(id);
        }
        return null;
    }
    private WorldObjectData GetWorldObjectDataByIdTask(int id) 
    {
        for (int i = 0; i < worldObjectData.Length; i++)
        {
            if(worldObjectData[i].objectId == id) 
            {
                return worldObjectData[i];
            }
        }
        return null;
    }

    //Get Prefab From Spawnpoint Data
    public static GameObject GetPrefabFromSpawnpointData(int object_type, int spawn_level) 
    {
        if(Singleton != null) 
        {
            return Singleton.GetPrefabFromSpawnpointDataTask(object_type, spawn_level);
        }
        return null;
    }
    private GameObject GetPrefabFromSpawnpointDataTask(int object_type, int spawn_level)
    {
        List<WorldObjectData> tempA = new List<WorldObjectData>(); // WorldObjects with Matching Type
        for (int i = 0; i < worldObjectData.Length; i++)
        {
            if (worldObjectData[i].objectType == object_type)
            {
                tempA.Add(worldObjectData[i]);
            }
        }
        if (spawn_level != 0)
        {
            for (int i = 0; i < tempA.Count; i++)
            {
                if (tempA[i].objectLevel != spawn_level)
                {
                    tempA.RemoveAt(i);
                }
            }
        }
        if (tempA.Count > 0)
        {
            return tempA[Random.Range(0, tempA.Count - 1)].objectPrefab;
        }
        else
        {
            return null;
        }
    }

    //Get Prefab From Spawnpoint Data
    public static GameObject GetPrefabFromObjectId(int objectId)
    {
        if (Singleton != null)
        {
            return Singleton.GetPrefabFromObjectIdTask(objectId);
        }
        return null;
    }
    private GameObject GetPrefabFromObjectIdTask(int objectId) 
    {
        return GetWorldObjectDataByIdTask(objectId).objectPrefab;
    }
}
