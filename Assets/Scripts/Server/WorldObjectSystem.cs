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
    private List<WorldObject> worldObjects = new List<WorldObject>();
    private List<WorldObject> worldObjectsHidden = new List<WorldObject>();
    private List<ulong> connectedClients = new List<ulong>();

    //Max Amounts
    private int max_trees = 9999;
    private int max_rocks = 9999;
    private int max_ore = 9999;

    //Amount Counters
    private int tree_amount = 0;
    private int rock_amount = 0;
    private int ore_amount = 0;

    //-----------------------------------------------------------------//
    //              World Object System Primary Functions              //
    //-----------------------------------------------------------------//

    //Start System
    public bool StartSystem()
    {
        systemEnabled = true;
        //System Started

        //Load world Objects
        LoadSavedObjects();

        networkingManager = NetworkingManager.Singleton;
        if(networkingManager != null) 
        {
            networkingManager.OnClientConnectedCallback += PlayerConnected;
            networkingManager.OnClientDisconnectCallback += PlayerDisconnected;
        }
        else 
        {
            return false;
        }

        //Start Respawner Loop
        StartCoroutine(WorldObjectRespawner());
        return true;
    }

    //Stop System
    public bool StopSystem() 
    {
        systemEnabled = false;
        return true;
    }

    //Load Saved Objects
    private void LoadSavedObjects() 
    {
        worldObjects = FindObjectsOfType<WorldObject>().ToList();
        worldObjects.Shuffle();
        for (int i = 0; i < worldObjects.Count; i++)
        {
            int objectType = worldObjects[i].objectType;
            worldObjects[i].objectId = i;
            worldObjects[i].objectAmount = GetObjectResourceAmount(objectType);
            if (objectType == 1) 
            {
                tree_amount++;
                if(tree_amount >= max_trees) 
                {
                    worldObjectsHidden.Add(worldObjects[i]);
                }
                else 
                {
                    worldObjectsHidden.Remove(worldObjects[i]);
                }
            }
            if (objectType == 2) 
            {
                rock_amount++;
                if(rock_amount <= max_rocks) 
                {
                    worldObjectsHidden.Add(worldObjects[i]);
                }
                else
                {
                    worldObjectsHidden.Remove(worldObjects[i]);
                }
            }
            if (objectType == 3) 
            {
                ore_amount++;
                if(ore_amount <= max_ore) 
                {
                    worldObjectsHidden.Add(worldObjects[i]);
                }
                else
                {
                    worldObjectsHidden.Remove(worldObjects[i]);
                }
            }
        }

        for (int i = 0; i < worldObjectsHidden.Count; i++)
        {
            for (int id = 0; id < connectedClients.Count; id++)
            {
                worldObjectsHidden[i].networkObject.NetworkHide(connectedClients[id]);
            }
        }
    }

    //Save Loaded Objects
    public void AutoSave() 
    {
        //Save all world objects
    }


    public void PlayerConnected(ulong clientId) 
    {
        for (int i = 0; i < worldObjectsHidden.Count; i++)
        {
            worldObjectsHidden[i].networkObject.NetworkHide(clientId);
        }
        connectedClients.Add(clientId);
    }
    public void PlayerDisconnected(ulong clientId) 
    {
        connectedClients.Remove(clientId);
    }


    //-----------------------------------------------------------------//
    //                    World Object Respawning                      //
    //-----------------------------------------------------------------//
    
    //Loop for Respawning
    private IEnumerator WorldObjectRespawner() 
    {
        while (systemEnabled) 
        {
            float netTime = NetworkingManager.Singleton.NetworkTime;
            for (int i = 0; i < worldObjects.Count; i++)
            {
                float time = worldObjects[i].objectDestroyedTime;
                if (time != 0) 
                {
                    int objectType = worldObjects[i].objectType;
                    if (netTime - time > GetObjectRespawnTime(objectType)) 
                    {
                        for (int id = 0; id < connectedClients.Count; id++)
                        {
                            worldObjects[i].networkObject.NetworkShow(connectedClients[id]);
                        }
                        worldObjects[i].objectAmount = GetObjectResourceAmount(objectType);
                    }
                }
            }
            yield return new WaitForSeconds(60); 
        }
    }

    //Get Respawn Time
    private int GetObjectRespawnTime(int objectType) 
    {
        if(objectType == 1) //Tree
        {
            return 240; //seconds
        }
        else if (objectType == 1) //Rock
        {
            return 420; //seconds
        } 
        else if (objectType == 1) //Ore
        {
            return 800; //seconds;
        }
        return 0;
    }
    
    //Get Resource Amount
    private int GetObjectResourceAmount(int objectType) 
    {
        if(objectType == 1) //Wood
        {
            return UnityEngine.Random.Range(100, 150);
        }
        else if (objectType == 2) //Stone
        {
            return UnityEngine.Random.Range(70, 80);
        }
        else if (objectType == 3) //Ore
        {
            return UnityEngine.Random.Range(30, 70);
        }
        return 0;
    }
}




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