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
    private GameServer gameServer;

    //World Object Lists
    private List<WorldObject> worldObjects = new List<WorldObject>();
    private List<WorldObject> worldObjectsHidden = new List<WorldObject>();
    private List<ulong> connectedClients = new List<ulong>();

    public WorldObjectData[] worldObjectDatas;


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
        gameServer = GameServer.singleton;
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

        foreach (WorldObject worldObject in worldObjects)
        {
            WorldObjectData data = GetObjectDataFromId(worldObject.objectDataId);
            if(data != null) 
            {
                worldObject.objectAmount = data.gatherAmount;
            }
            else 
            {
                NetworkHideForAll(worldObject.networkObject);
            }
        }
    }

    //Save Loaded Objects
    public void AutoSave() 
    {
        //Save all world objects
    }

    //ConnectCallbacks
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
            for (int i = 0; i < worldObjectsHidden.Count; i++)
            {
                WorldObject worldObject = worldObjectsHidden[i];
                WorldObjectData data = GetObjectDataFromId(worldObject.objectDataId);
                if (data != null && worldObject.objectDestroyedTime != 0) 
                {
                    if (GetNetworkTime() - worldObject.objectDestroyedTime >= data.respawnTime) 
                    {
                        NetworkShowForAll(worldObject.networkObject);
                        worldObjectsHidden[i].objectAmount = data.gatherTotal;
                    }
                }
            }
            yield return new WaitForSeconds(60); 
        }
    }

    //DepleteWorldObject
    public void DepleteWorldObject(ulong networkId, int toolId, int amount, Action<WorldObjectTransferData> callback) 
    {
        DebugMsg.Notify("Starting Deplete of WorldObject", 3);
        for (int i = 0; i < worldObjects.Count; i++)
        {
            if (worldObjects[i].NetworkId == networkId)
            {
                DebugMsg.Notify("WorldObject with Matching ID found", 4);
                WorldObject obj = worldObjects[i];
                WorldObjectData data = GetObjectDataFromId(obj.objectDataId);
                if (data.toolId == toolId)
                {
                    DebugMsg.Notify("Tool ID matches", 4);
                    if (obj.objectAmount - amount > 0)
                    {
                        DebugMsg.Notify("Depleting", 4);
                        worldObjects[i].objectAmount -= amount;
                        callback(new WorldObjectTransferData() { itemId = data.gatherItemId, amount = amount });
                    }
                    else if (obj.objectAmount > 0)
                    {
                        DebugMsg.Notify("Depleting and Destroying", 4);
                        worldObjects[i].objectAmount = 0;
                        worldObjectsHidden.Add(worldObjects[i]);
                        NetworkHideForAll(obj.networkObject);
                        callback(new WorldObjectTransferData() { itemId = data.gatherItemId, amount = obj.objectAmount });
                    }
                }
                break;
            }
        }
    }

    //-----------------------------------------------------------------//
    //                    World Object  TOOLS                          //
    //-----------------------------------------------------------------//

    //Get Network Time
    private float GetNetworkTime() 
    {
        if(networkingManager != null) 
        {
            return networkingManager.NetworkTime;
        }
        else 
        {
            networkingManager = NetworkingManager.Singleton;
            if (networkingManager != null)
            {
                return networkingManager.NetworkTime;
            }
            else 
            {
                return 0;
            }
        }
    }

    //Get World Object Data
    private WorldObjectData GetObjectDataFromId(int objectId) 
    {
        foreach (WorldObjectData data in worldObjectDatas)
        {
            if(objectId == data.objectId) 
            {
                return data;
            }
        }
        return null;
    }
    
    //Show NetworkObject for ALL
    private void NetworkShowForAll(NetworkedObject networkObject)
    {
        for (int id = 0; id < connectedClients.Count; id++)
        {
            networkObject.NetworkShow(connectedClients[id]);
        }
    }

    //Hide NetworkObject for ALL
    private void NetworkHideForAll(NetworkedObject networkObject)
    {
        for (int id = 0; id < connectedClients.Count; id++)
        {
            networkObject.NetworkHide(connectedClients[id]);
        }
    }


}

public class WorldObjectTransferData 
{
    public int itemId;
    public int amount;
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