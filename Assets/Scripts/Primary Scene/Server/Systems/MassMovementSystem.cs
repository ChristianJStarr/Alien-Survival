using MLAPI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MassMovementSystem : MonoBehaviour
{
    private bool systemEnabled = false;

    public Dictionary<ulong, List<MovementCall>> playerCommandQueue = new Dictionary<ulong, List<MovementCall>>();

    public WorldSnapshot[] snapshots;

    public bool StartSystem()
    {
        systemEnabled = true;
        return true;
    }

    public bool StopSystem()
    {
        systemEnabled = false;
        return true;
    }


    public void SendCommand(ulong clientId, MovementCall command)
    {
        if (playerCommandQueue.ContainsKey(clientId))
        {
            playerCommandQueue[clientId].Add(command);
        }
        else
        {
            List<MovementCall> temp = new List<MovementCall>();
            temp.Add(command);
            playerCommandQueue.Add(clientId, temp);
        }
    }



    private void FixedUpdate()
    {
        List<EntityData> data = snapshots[0].entityData.ToList();
        foreach (ulong clientId in playerCommandQueue.Keys.ToArray())
        {
            List<MovementCall> commands;
            if (playerCommandQueue.TryGetValue(clientId, out commands))
            {
                for (int i = 0; i < commands.Count; i++)
                {
                    bool contains = false;
                    float horizontal = commands[i].horizontalAxis;
                    float vertical = commands[i].verticalAxis;
                    float time = commands[i].networkTime;
                    for (int e = 0; i < data.Count; e++)
                    {
                        if (data[e].clientId == clientId) 
                        {
                            contains = true;
                            data[e] = (ExecuteCommand(clientId, time, horizontal, vertical));
                            break;
                        }
                    }
                    if (!contains) 
                    {
                        data.Add(ExecuteCommand(clientId, time, horizontal, vertical));
                    }
                }
            }
        }


        WorldSnapshot newSnap = new WorldSnapshot(NetworkingManager.Singleton.NetworkTime, data.ToArray());
        snapshots = new WorldSnapshot[] { newSnap, snapshots[1], snapshots[2], snapshots[3] };
    }

    public EntityData ExecuteCommand(ulong _clientId, float networkTime, float horizontal, float vertical) 
    {
        EntityData data = new EntityData() { clientId = _clientId };

        NetworkedObject player = NetworkingManager.Singleton.ConnectedClients[_clientId].PlayerObject;


        Vector3 currentPosition = player.transform.position;
        Quaternion currentRotation = player.transform.rotation;

        data.location = currentPosition;
        data.rotation = currentRotation;

        
        return data;
    }




}


public struct WorldSnapshot 
{
    public float networkTime;
    public EntityData[] entityData;

    public WorldSnapshot (float time, EntityData[] datas) 
    {
        this.networkTime = time;
        this.entityData = datas;
    }
}

public class EntityData 
{
    public ulong clientId;
    public Vector3 location;
    public Quaternion rotation;
    
}
public struct MovementCall 
{
    public float horizontalAxis;
    public float verticalAxis;
    public float networkTime;
    public Vector3 lookDir;

    public MovementCall (float horiz, float vert, float time, Vector3 look) 
    {
        this.horizontalAxis = horiz;
        this.verticalAxis = vert;
        this.networkTime = time;
        this.lookDir = look;
    }
}


