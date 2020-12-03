using System.Collections.Generic;
using UnityEngine;

public class PooledManager : MonoBehaviour
{
    public static PooledManager Singleton;
    private Dictionary<int, Queue<GameObject>> pool = new Dictionary<int, Queue<GameObject>>();

    private void Awake() 
    {
        Singleton = this;
    }

    /// <summary>
    /// Instantiate a Pooled Object. Do not destroy, only SetActive(false)
    /// </summary>

    public static GameObject InstantiatePooledObject(GameObject prefab, Vector3 position, Quaternion rotation) 
    {
        if (prefab == null) return null;
        return Singleton.InstantiateObject(prefab, false, position, rotation);
    }

    /// <summary>
    /// Instantiate a Pooled Object with Parent. Do not destroy, only SetActive(false)
    /// </summary>

    public static GameObject InstantiatePooledObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (prefab == null) return null;
        return Singleton.InstantiateObject(prefab, false, position, rotation, parent);
    }

    /// <summary>
    /// Instantiate a Pooled Object with Parent. Using Prefab Position/Rotation. Do not destroy, only SetActive(false)
    /// </summary>

    public static GameObject InstantiatePooledObject(GameObject prefab, Transform parent) 
    {
        if (prefab == null) return null;
        return Singleton.InstantiateObject(prefab, true, Vector3.zero, Quaternion.identity, parent);
    }


    //Instantiate Object Task
    private GameObject InstantiateObject(GameObject prefab, bool usePrefabPosition, Vector3 position, Quaternion rotation, Transform parent = null)  
    {
        int instanceId = prefab.GetInstanceID();

        GameObject instance = null;
        if (pool.ContainsKey(instanceId) && pool[instanceId].Count != 0)
        {
            if (pool[instanceId].Peek().activeSelf == false)
            {
                instance = pool[instanceId].Dequeue();
                pool[instanceId].Enqueue(instance);
            }
            else
            {
                AddInstanceToPool(prefab);
                instance = pool[instanceId].Dequeue();
                pool[instanceId].Enqueue(instance);
            }
        }
        else
        {
            AddInstanceToPool(prefab);
            instance = pool[instanceId].Dequeue();
            pool[instanceId].Enqueue(instance);
        }
        if (usePrefabPosition && parent != null) 
        {
            instance.transform.SetParent(parent, false);
            instance.transform.position = prefab.transform.position;
            instance.transform.rotation = prefab.transform.rotation;
        }
        else 
        {
            instance.transform.position = position;
            instance.transform.rotation = rotation;
        }
        instance.SetActive(true);

        return instance;
    }

    //Add Instance to Pool Queue
    private void AddInstanceToPool(GameObject prefab) 
    {
        int instanceId = prefab.GetInstanceID();
        prefab.SetActive(false);
        GameObject instance = Instantiate(prefab);
        prefab.SetActive(true);
        instance.name = prefab.name;
        instance.transform.parent = transform;
        if (!pool.ContainsKey(instanceId))
        {
            pool.Add(instanceId, new Queue<GameObject>());
        }
        pool[instanceId].Enqueue(instance);
    }

}
