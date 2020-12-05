using MLAPI;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class WorldSnapshotSystem : MonoBehaviour
{
    private bool systemEnabled = false;

    private GameServer gameServer; //Game Server

    //Systems & Managers
    public PlayerCommandSystem commandSystem;
    public WorldAISystem worldAISystem;
    public WorldObjectSystem worldObjectSystem;
    private NetworkingManager networkingManager;
    
    //Trim Snapshot Job
    TrimSnapshotByDistance trimSnapshotJob;
    JobHandle trimHandle;
    NativeArray<Vector3> trimPositionsNative, trimPlayersNative;
    NativeArray<bool> trimIncludeNative;

    //Configuration
    public int snapshotRange = 200; //Meters
    public int sendFullEvery = 1; //Seconds

    //Inc. Values
    private int _snapshotId = 1; //Current SnapshotID
    private int currentFrame = 0;
    private QuickAccess_Snapshot lastSnapshot;
    private Dictionary<ulong, ulong> clientIdDictionary = new Dictionary<ulong, ulong>();




    //Start System
    public bool StartSystem() 
    {
        systemEnabled = true;

        if (NetworkingManager.Singleton != null)
        {
            networkingManager = NetworkingManager.Singleton;
        } else { return false; }

        if(GameServer.singleton != null)
        {
            gameServer = GameServer.singleton;
        } else { return false; }

        return systemEnabled;
    }
    //Stop System
    public bool StopSystem() 
    {
        systemEnabled = false;
        return !systemEnabled;
    }

    private void Update() 
    {
        if (systemEnabled) 
        {
            currentFrame++;
            if (currentFrame == sendFullEvery * 20)
            {
                currentFrame = 0;
                SendSnapshotToEveryone(true);
            }
            else 
            {
                SendSnapshotToEveryone(false);
            }
        }
    }

    
    //Get ClientID from NetworkId
    private ulong GetClientId(ulong networkId)
    {
        if (clientIdDictionary.ContainsKey(networkId))
        {
            return clientIdDictionary[networkId];
        }
        else 
        {
            ulong clientId = gameServer.Server_GetClientIdFromNetworkId(networkId);
            clientIdDictionary.Add(networkId, clientId);
            return clientId;
        }
    }

    //Send Snapshot To Every Client
    private void SendSnapshotToEveryone(bool full) 
    {
        Snapshot snapshot = CreateWorldSnapshot();
        GameServer.singleton.DebugSnapshotId = snapshot.snapshotId;
        //Create Native Arrays for Job
        int playerCount = snapshot.players.Length;
        if (playerCount < 1) return; //No players? No snapshot.
        int aiCount = snapshot.ai.Length;
        int worldObjectCount = snapshot.worldObjects.Length;
        int positionsLength = playerCount + aiCount + worldObjectCount;

        //Allocate Arrays
        Vector3[] positions = new Vector3[positionsLength];
        Vector3[] players = new Vector3[playerCount];
        ulong[] clientIds = new ulong[playerCount];

        int count = 0;
        //Write Positions of Snapshot
        for (int e = 0; e < playerCount; e++)
        {
            positions[count] = snapshot.players[e].location;
            players[count] = positions[count];
            clientIds[count] = GetClientId(snapshot.players[e].networkId);
            count++;
        }
        for (int e = 0; e < snapshot.ai.Length; e++)
        {
            positions[count] = snapshot.ai[e].location;
            count++;
        }
        for (int e = 0; e < snapshot.worldObjects.Length; e++)
        {
            positions[count] = snapshot.worldObjects[e].location;
            count++;
        }
        
        //Allocate into Natives
        trimPositionsNative = new NativeArray<Vector3>(positions, Allocator.TempJob);
        trimPlayersNative = new NativeArray<Vector3>(players, Allocator.TempJob);
        trimIncludeNative = new NativeArray<bool>(playerCount * positionsLength, Allocator.TempJob);

        //Create Job
        trimSnapshotJob = new TrimSnapshotByDistance()
        {
            position = trimPositionsNative,
            player = trimPlayersNative,
            result = trimIncludeNative,
            positionLength = positionsLength,
            distance = snapshotRange
        };
        //Schedule Job
        trimHandle = trimSnapshotJob.Schedule(playerCount * positionsLength, trimHandle);
        //Wait for Job Complete
        trimHandle.Complete();
        //Send Individual Snapshots to ClientIds[]
        for (int i = 0; i < playerCount; i++)
        {
            List<Snapshot_Player> tempA = new List<Snapshot_Player>();
            List<Snapshot_AI> tempB = new List<Snapshot_AI>();
            List<Snapshot_WorldObject> tempC = new List<Snapshot_WorldObject>();
            for (int e = 0; e < positionsLength; e++)
            {
                if (trimIncludeNative[(i * positionsLength) + e])
                {
                    if (e < playerCount)
                    {
                        tempA.Add(snapshot.players[e]);
                    }
                    else if (e < aiCount)
                    {
                        if (full) 
                        {
                            tempB.Add(snapshot.ai[e - playerCount]);
                        }
                        else if(AIHasChanged(snapshot.ai[e - playerCount]))
                        {
                            tempB.Add(snapshot.ai[e - playerCount]);
                        }
                    }
                    else if (e < worldObjectCount)
                    {
                        if (full) 
                        {
                            tempC.Add(snapshot.worldObjects[e - playerCount - aiCount]);
                        }
                        else if(WorldObjectHasChanged(snapshot.worldObjects[e - playerCount - aiCount])) 
                        {
                            tempC.Add(snapshot.worldObjects[e - playerCount - aiCount]);
                        }
                    }
                }
            }
            Snapshot newSnapshot = new Snapshot()
            {
                networkTime = snapshot.networkTime,
                snapshotId = snapshot.snapshotId,
                players = tempA.ToArray()
            };
            if (tempB.Count > 0)
            {
                newSnapshot.ai = tempB.ToArray();
            }
            if (tempC.Count > 0)
            {
                newSnapshot.worldObjects = tempC.ToArray();
            }
            gameServer.ServerSendSnapshot(clientIds[i], newSnapshot);
        }
        trimPositionsNative.Dispose();
        trimPlayersNative.Dispose();
        trimIncludeNative.Dispose();
    }

    //Read World State & Create Snapshot
    private Snapshot CreateWorldSnapshot() 
    {
        Snapshot fullSnapshot = new Snapshot();
        //Convert Players To Snapshot
        PlayerControlObject[] players = commandSystem.players.Values.ToArray();
        fullSnapshot.players = new Snapshot_Player[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            fullSnapshot.players[i] = players[i].ConvertToSnapshot();
        }
        //Convert AI To Snapshot
        AIControlObject[] ai = worldAISystem.ai.ToArray();
        fullSnapshot.ai = new Snapshot_AI[ai.Length];
        for (int i = 0; i < ai.Length; i++)
        {
            fullSnapshot.ai[i] = ai[i].ConvertToSnapshot();
        }
        //Convert WorldObjects To Snapshot
        fullSnapshot.worldObjects = worldObjectSystem.GetWorldObjectsSnapshot();

        //Set Time & ID
        fullSnapshot.networkTime = networkingManager.NetworkTime;
        fullSnapshot.snapshotId = _snapshotId;

        _snapshotId++;
        if (_snapshotId > 100) _snapshotId = 1;
        lastSnapshot = Snapshot.ConvertQuick(fullSnapshot);
        //Send Snapshot
        return fullSnapshot;
    }

    //Has Player Changed
    private bool PlayerHasChanged(Snapshot_Player player) 
    {
        return true;
        if (lastSnapshot != null && lastSnapshot.players != null && lastSnapshot.players.ContainsKey(player.networkId)) 
        {
            if(lastSnapshot.players[player.networkId] != player) 
            {
                return true;
            }
            return false;
        }
        return true;
    }

    //Has AI Changed
    private bool AIHasChanged(Snapshot_AI ai)
    {
        if (lastSnapshot != null && lastSnapshot.ai != null && lastSnapshot.ai.ContainsKey(ai.networkId))
        {
            if (lastSnapshot.ai[ai.networkId] != ai)
            {
                return true;
            }
            return false;
        }
        return true;
    }

    //Has World Object Changed
    private bool WorldObjectHasChanged(Snapshot_WorldObject worldObject) 
    {
        if (lastSnapshot != null && lastSnapshot.worldObjects != null && lastSnapshot.worldObjects.ContainsKey(worldObject.spawnId))
        {
            if (lastSnapshot.worldObjects[worldObject.spawnId] != worldObject)
            {
                return true;
            }
            return false;
        }
        return true;
    }

}

public class QuickAccess_Snapshot
{
    public int snapshotId;
    public float networkTime;
    public Dictionary<ulong, Snapshot_Player> players = new Dictionary<ulong, Snapshot_Player>();
    public Dictionary<ulong, Snapshot_AI> ai = new Dictionary<ulong, Snapshot_AI>();
    public Dictionary<int, Snapshot_WorldObject> worldObjects = new Dictionary<int, Snapshot_WorldObject>();
}

public class Snapshot 
{
    public int snapshotId;
    public float networkTime;
    public Snapshot_Player[] players = new Snapshot_Player[0];
    public Snapshot_AI[] ai = new Snapshot_AI[0];
    public Snapshot_WorldObject[] worldObjects = new Snapshot_WorldObject[0];

    public static QuickAccess_Snapshot ConvertQuick(Snapshot snapshot)
    {
        QuickAccess_Snapshot quickAccess = new QuickAccess_Snapshot()
        {
            snapshotId = snapshot.snapshotId,
            networkTime = snapshot.networkTime,
            players = new Dictionary<ulong, Snapshot_Player>(),
            ai = new Dictionary<ulong, Snapshot_AI>(),
            worldObjects = new Dictionary<int, Snapshot_WorldObject>()
        };
        if (snapshot.players != null)
        {
            for (int i = 0; i < snapshot.players.Length; i++)
            {
                quickAccess.players.Add(snapshot.players[i].networkId, snapshot.players[i]);
            }
        }
        if (snapshot.ai != null)
        {
            for (int i = 0; i < snapshot.ai.Length; i++)
            {
                if (!quickAccess.ai.ContainsKey(snapshot.ai[i].networkId))
                {
                    quickAccess.ai.Add(snapshot.ai[i].networkId, snapshot.ai[i]);
                }
            }
        }
        if (snapshot.worldObjects != null)
        {
            for (int i = 0; i < snapshot.worldObjects.Length; i++)
            {
                if (!quickAccess.worldObjects.ContainsKey(snapshot.worldObjects[i].spawnId)) 
                {
                    quickAccess.worldObjects.Add(snapshot.worldObjects[i].spawnId, snapshot.worldObjects[i]);
                }
            }
        }
        return quickAccess;
    }
}

public class Snapshot_Player 
{
    public ulong networkId;
    public Vector3 location;
    public Vector2 rotation;
    public int holdId;
    public int holdState;
}

public class Snapshot_AI 
{
    public ulong networkId;
    public Vector3 location;
    public Vector2 rotation;
    public int holdId;
    public int holdState;
}

public class Snapshot_WorldObject 
{
    public int objectId;
    public int spawnId;
    public Vector3 location;
}

//Jobs
public struct TrimSnapshotByDistance : IJobFor
{
    [ReadOnly]
    public NativeArray<Vector3> position;
    [ReadOnly]
    public NativeArray<Vector3> player;
    [ReadOnly]
    public int positionLength;
    [ReadOnly]
    public int distance;

    public NativeArray<bool> result; // Index I

    public void Execute(int i) 
    {
        int playerIndex = i / positionLength;
        int positionIndex = i - (playerIndex * positionLength);
        if (Vector3.Distance(position[positionIndex], player[playerIndex]) < distance)
        {
            result[i] = true;
        }
    }
}


