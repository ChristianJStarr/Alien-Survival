using MLAPI;
using MLAPI.Connection;
using MLAPI.Serialization.Pooled;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class WorldSnapshotSystem : MonoBehaviour
{
    public PlayerCommandSystem commandSystem;

    private bool systemEnabled = false;


    private Snapshot[] snapshots;


    public bool StartSystem() 
    {
        systemEnabled = true;
        snapshots = new Snapshot[3];
        return true;
    }
    public bool StopSystem() 
    {
        systemEnabled = false;
        return true;
    }

    private void Update() 
    {
        if (systemEnabled) 
        {
            //15 Sends per Second
            SendSnapshot();
        }
    }

    //Send Snapshot to Clients
    private void SendSnapshot() 
    {
        int _snapshotId = 0;
        if (snapshots[2] == null)
        {
            _snapshotId = 1;
        }
        else 
        {
            _snapshotId = snapshots[2].snapshotId + 1;
        }


        Snapshot snapshot = new Snapshot()
        {
            snapshotId = _snapshotId,
            players = Generate_SnapshotPlayer(),
            objects = Generate_SnapshotObject()
        };
        GameServer.singleton.ServerSendSnapshot(snapshot);
        StoreSnapshot(snapshot);
    }

    //Store Snapshot in Prediction Array
    private void StoreSnapshot(Snapshot snapshot) 
    {
        if(snapshot != null) 
        {
            snapshots[0] = snapshots[1];
            snapshots[1] = snapshots[2];
            snapshots[2] = snapshot;
        }
    }

    //Generate Snapshot_Player Array
    private Snapshot_Player[] Generate_SnapshotPlayer() 
    {
        PlayerControlObject[] clients = commandSystem.players.Values.ToArray();
        if(clients != null) 
        {
            List<Snapshot_Player> players = new List<Snapshot_Player>();
            for (int i = 0; i < clients.Length; i++)
            {
                players.Add(new Snapshot_Player()
                {
                    networkId = clients[i].NetworkId,
                    location = clients[i].transform.position,
                    rotation = clients[i].transform.rotation.eulerAngles,
                    animation = 0
                });
            }
            return players.ToArray();
        }
        else 
        {
            return null;
        }
        
    }

    //Generate Snapshot_Object Array
    private Snapshot_Object[] Generate_SnapshotObject() 
    {
        int objectAmount = 0;
        if(objectAmount > 0) 
        {
            Snapshot_Object[] objects = new Snapshot_Object[objectAmount];
            for (int i = 0; i < objectAmount; i++)
            {
                objects[i].networkId = 0;
                objects[i].location = Vector3.zero;
                objects[i].rotation = Vector3.zero;
                objects[i].state = 0;
            }
            return objects;
        }
        else 
        {
            return null;
        }
    }
}

public class Snapshot 
{
    public int snapshotId;
    public Snapshot_Player[] players;
    public Snapshot_Object[] objects;
}

public class Snapshot_Player 
{
    public ulong networkId;
    public Vector3 location;
    public Vector3 velocity;
    public Vector3 rotation; 
    public int animation;
}

public class Snapshot_Object 
{
    public ulong networkId;
    public Vector3 location;
    public Vector3 rotation;
    public int state;
}

