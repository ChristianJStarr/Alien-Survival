using MLAPI;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class PlayerObjectSystem : MonoBehaviour
{
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
    #region Singleton
    public static PlayerObjectSystem Singleton;
    private void Awake() { Singleton = this; }
    #endregion

    private bool systemEnabled = false;
    //Player Object Dictionary   ClientID Key
    private Dictionary<ulong, PlayerControlObject> players = new Dictionary<ulong, PlayerControlObject>();


    //--------JOBS-----------------------------------
    private bool useJobs = false;
    private NativeArray<Vector3> positions_native;
    private NativeArray<bool> nearby_native;
    private GetNearbyClientsJob getNearbyClientsJob;
    private JobHandle getNearbyClientsHandle;








    private bool StartSystem()
    {
        systemEnabled = true;

        return systemEnabled;
    }
    private bool StopSystem()
    {
        systemEnabled = false;

        return !systemEnabled;
    }



    //---------------Register / UnRegister---------------

    public void RegisterControlObject(ulong clientId, NetworkedObject networkedObject)
    {
        PlayerControlObject controlObject = networkedObject.GetComponent<PlayerControlObject>();
        if (controlObject != null)
        {
            controlObject.owner_clientId = clientId;
            controlObject.owner_networkId = networkedObject.NetworkId;
            if (!players.ContainsKey(clientId))
            {
                players.Add(clientId, controlObject);
            }
        }
    }

    public void UnRegisterControlObject(ulong clientId)
    {
        if (players.ContainsKey(clientId))
        {
            players.Remove(clientId);
        }
    }



    //---------------Control Object Data-----------------


    //Get If Exists
    public bool ControlObjectExists(ulong clientId)
    {
        return players.ContainsKey(clientId);
    }
    //Get Object from ClientId
    public PlayerControlObject GetControlObjectByClientId(ulong clientId)
    {
        if (players.ContainsKey(clientId))
        {
            return players[clientId];
        }
        return null;
    }
    //Get All ControlObjects
    public PlayerControlObject[] GetAllPlayerControlObjects()
    {
        return players.Values.ToArray();
    }
    //Get Snapshot of Players
    public Snapshot_Player[] GetControlObjectSnapshot()
    {
        PlayerControlObject[] instances = players.Values.ToArray();
        int count = instances.Length;
        Snapshot_Player[] instance = new Snapshot_Player[count];
        for (int i = 0; i < count; i++)
        {
            instance[i] = instances[i].ConvertToSnapshot();
        }
        return instance;
    }
   
    //-------------Modify Control Object-----------------

    public void ModifyHoldable(ulong clientId, int holdableId, int holdableState) 
    {
        if (players.ContainsKey(clientId)) 
        {
            PlayerControlObject controlObject = players[clientId];
            controlObject.holdableId = holdableId;
            controlObject.holdableState = holdableState;
        }
    }

    public void ToggleRagdoll(ulong clientId, bool enable) 
    {
        if (players.ContainsKey(clientId)) 
        {
            players[clientId].ToggleRagdoll(enable);
        }
    }



    //Teleport To Player
    public void Teleport_ToPlayer(ulong clientId, ulong target_clientId)
    {
        if (players.ContainsKey(clientId) && players.ContainsKey(target_clientId))
        {
            PlayerControlObject player = players[clientId];
            player.characterController.enabled = false;
            player.transform.position = players[target_clientId].transform.position;
            player.characterController.enabled = true;
        }
    }

    //Teleport To Vector3
    public void Teleport_ToVector(ulong clientId, Vector3 target_position)
    {
        if (players.ContainsKey(clientId))
        {
            players[clientId].ApplyCorrection(target_position);
        }
    }
    public void Teleport_ToVector(ulong clientId, Vector3 target_position, Quaternion target_rotation)
    {
        if (players.ContainsKey(clientId))
        {
            players[clientId].Teleport(target_position, target_rotation);
        }
    }

    //Get Positions Array
    public Vector3[] GetPlayerPositionsArray()
    {
        PlayerControlObject[] instances = players.Values.ToArray();
        int length = instances.Length;
        Vector3[] instance = new Vector3[length];
        for (int i = 0; i < length; i++)
        {
            instance[i] = instances[i].transform.position;
        }
        return instance;
    }
    public Vector3[] GetPlayerPositionsArray(ulong exclude)
    {
        Vector3[] instance = null;
        if (players.ContainsKey(exclude))
        {
            PlayerControlObject[] temp = players.Values.ToArray();
            int length = temp.Length;
            instance = new Vector3[length - 1];
            for (int i = 0; i < length; i++)
            {
                if (temp[i].OwnerClientId != exclude)
                {
                    instance[i] = temp[i].transform.position;
                }
            }
        }
        else
        {
            instance = GetPlayerPositionsArray();
        }
        return instance;
    }

    //Get Player Current Position
    public Vector3 GetCurrentPosition(ulong clientId) 
    {
        if (players.ContainsKey(clientId)) 
        {
            return players[clientId].transform.position;
        }
        return Vector3.zero;
    }



    
    //Get List of Nearby Clients
    public List<ulong> GetNearbyClients(Vector3 position, int distance, ulong clientToIgnore = 0)
    {
        if (useJobs) 
        {
            return GetNearbyClients_Job(position, distance, clientToIgnore);   
        }
        else 
        {
            return GetNearbyClients_Task(position, distance, clientToIgnore);
        }
    }
    public List<ulong> GetNearbyClients(ulong clientId, int distance, ulong clientToIgnore = 0)
    {
        if (players.ContainsKey(clientId)) 
        {
            Vector3 position = players[clientId].transform.position;
            if (useJobs)
            {
                return GetNearbyClients_Job(position, distance, clientToIgnore);
            }
            else
            {
                return GetNearbyClients_Task(position, distance, clientToIgnore);
            }
        }
        return new List<ulong>();
    }
    private List<ulong> GetNearbyClients_Task(Vector3 position, int distance, ulong clientToIgnore)
    {
        DebugMsg.Begin(299, "Getting Nearby Clients. Task", 2);
        List<ulong> nearby = new List<ulong>();
        ulong[] clientIds = players.Keys.ToArray();
        Vector3[] positions = GetPlayerPositionsArray();
        int clients = clientIds.Length;
        for (int i = 0; i < clients; i++)
        {
            if (Vector3.Distance(positions[i], position) < distance)
            {
                if (clientIds[i] != clientToIgnore)
                {
                    nearby.Add(clientIds[i]);
                }
            }
        }
        DebugMsg.End(299, "Finsihed Getting Nearby Clients. Task", 2);
        return nearby;
    }
    private List<ulong> GetNearbyClients_Job(Vector3 position, int distance, ulong clientToIgnore)
    {
        DebugMsg.Begin(298, "Getting Nearby Clients. IJOB", 2);
        List<ulong> nearby = new List<ulong>();
        ulong[] clientIds = players.Keys.ToArray();
        int clients = clientIds.Length;
        positions_native = new NativeArray<Vector3>(GetPlayerPositionsArray(), Allocator.TempJob);
        nearby_native = new NativeArray<bool>(clients, Allocator.TempJob);
        getNearbyClientsJob = new GetNearbyClientsJob()
        {
            positions = positions_native,
            returns = nearby_native,
            point = position,
            length = distance
        };
        getNearbyClientsHandle = getNearbyClientsJob.Schedule(clients, getNearbyClientsHandle);
        getNearbyClientsHandle.Complete();
        for (int i = 0; i < clients; i++)
        {
            if (nearby_native[i] && clientIds[i] != clientToIgnore)
            {
                nearby.Add(clientIds[i]);
            }
        }
        positions_native.Dispose();
        nearby_native.Dispose();
        DebugMsg.End(298, "Finsihed Getting Nearby Clients. IJOB", 2);
        return nearby;
    }



#endif
}

public struct GetNearbyClientsJob : IJobFor 
{
    [ReadOnly]public NativeArray<Vector3> positions;
    public NativeArray<bool> returns;
    public Vector3 point;
    public float length;
    public void Execute(int index) 
    {
        returns[index] = Vector3.Distance(positions[index], point) <= length;
    }
}
