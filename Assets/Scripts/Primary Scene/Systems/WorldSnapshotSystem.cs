using MLAPI;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class WorldSnapshotSystem : MonoBehaviour
{
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
    private bool systemEnabled = false;

    private GameServer gameServer; //Game Server

    //Systems & Managers
    public PlayerObjectSystem playerObjectSystem;
    public WorldAISystem worldAISystem;
    public WorldObjectSystem worldObjectSystem;
    private NetworkingManager networkingManager;

    //Configuration
    public int snapshotRange = 200; //Meters
    public int sendFullEvery = 1; //Seconds

    //Inc. Values
    private int _snapshotId = 1; //Current SnapshotID

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

    private void FixedUpdate()
    {
#if UNITY_EDITOR
        if (networkingManager != null && networkingManager.IsServer) 
        {
            PackedSnapshot[] snapshots = GetFullChunkSnapshots();
            for (int i = 0; i < snapshots.Length; i++)
            {
                gameServer.ServerSendSnapshot(snapshots[i]);
            }
        }
#elif UNITY_SERVER
            PackedSnapshot[] snapshots = GetFullChunkSnapshots();
            for (int i = 0; i < snapshots.Length; i++)
            {
                gameServer.ServerSendSnapshot(snapshots[i]);
            }
#endif
    }
    private PackedSnapshot[] GetFullChunkSnapshots() 
    {
        _snapshotId++;
        if (_snapshotId > 100) _snapshotId = 1;

        Snapshot_Player[] s_players = playerObjectSystem.GetControlObjectSnapshot();
        Snapshot_AI[] s_ais = worldAISystem.GetAIObjectsSnapshot();
        Snapshot_WorldObject[] s_worldObjects = worldObjectSystem.GetWorldObjectsSnapshot();

        float networkTime = networkingManager.NetworkTime;

        //Get Loaded Chunks
        List<int> loadedChunks = new List<int>();
        for (int i = 0; i < s_players.Length; i++)
        {
            int[] nearbyChunks = ChunkHelper.GetNearbyChunkIds(s_players[i].chunkId);
            for (int e = 0; e < nearbyChunks.Length; e++)
            {
                if (!loadedChunks.Contains(nearbyChunks[e]))
                {
                    loadedChunks.Add(nearbyChunks[e]);
                }
            }
        }

        //Seperate Data into Chunks
        Dictionary<int, QuickChunkData> chunkDatas = new Dictionary<int, QuickChunkData>();

        //Player Object Data
        for (int i = 0; i < s_players.Length; i++)
        {
            int _chunkId = s_players[i].chunkId;
            if (loadedChunks.Contains(s_players[i].chunkId))
            {
                if (chunkDatas.ContainsKey(_chunkId))
                {
                    chunkDatas[_chunkId].q_chunkPlayer.Add(s_players[i]);
                }
                else
                {
                    chunkDatas.Add(_chunkId, new QuickChunkData());
                    chunkDatas[_chunkId].q_chunkPlayer.Add(s_players[i]);
                }
            }
        }

        //World AI Data
        for (int i = 0; i < s_ais.Length; i++)
        {
            int _chunkId = s_ais[i].chunkId;
            if (loadedChunks.Contains(s_ais[i].chunkId))
            {
                if (chunkDatas.ContainsKey(_chunkId))
                {
                    chunkDatas[_chunkId].q_chunkAI.Add(s_ais[i]);
                }
                else
                {
                    chunkDatas.Add(_chunkId, new QuickChunkData());
                    chunkDatas[_chunkId].q_chunkAI.Add(s_ais[i]);
                }
            }
        }

        //World Object Data
        for (int i = 0; i < s_worldObjects.Length; i++)
        {
            int _chunkId = s_worldObjects[i].chunkId;
            if (loadedChunks.Contains(s_worldObjects[i].chunkId))
            {
                if (chunkDatas.ContainsKey(_chunkId))
                {
                    chunkDatas[_chunkId].q_chunkWorldObject.Add(s_worldObjects[i]);
                }
                else
                {
                    chunkDatas.Add(_chunkId, new QuickChunkData());
                    chunkDatas[_chunkId].q_chunkWorldObject.Add(s_worldObjects[i]);
                }
            }
        }

        //Pack Chunk Snapshots


        //Convert Quick to Snapshot
        QuickChunkData[] instances = chunkDatas.Values.ToArray();
        int instanceLength = instances.Length;
        ChunkSnapshot[] chunkSnapshots = new ChunkSnapshot[instanceLength];
        for (int i = 0; i < instanceLength; i++)
        {
            chunkSnapshots[i] = QuickChunkData.CovertToSnapshot(instances[i]);
        }

        //   centerchunk /  snapshots
        Dictionary<int, PackedSnapshot> centerChunks = new Dictionary<int, PackedSnapshot>();

        for (int i = 0; i < s_players.Length; i++)
        {
            int centerChunkId = s_players[i].chunkId;
            if (centerChunks.ContainsKey(centerChunkId)) 
            {
                if (centerChunks[centerChunkId].clients == null) 
                {
                    centerChunks[centerChunkId].clients = new List<ulong>();
                }
                centerChunks[s_players[i].chunkId].clients.Add(s_players[i].clientId);
            }
            else 
            {
                PackedSnapshot packed = new PackedSnapshot();
                packed.snapshotId = _snapshotId;
                packed.networkTime = networkTime;
                if (packed.clients == null) packed.clients = new List<ulong>();
                packed.clients.Add(s_players[i].clientId);
                centerChunks.Add(centerChunkId, packed);
            }
        }

        //Fill Packed Snapshots
        PackedSnapshot[] packedSnapshots = centerChunks.Values.ToArray();
        for (int i = 0; i < packedSnapshots.Length; i++)
        {
            int[] nearby = ChunkHelper.GetNearbyChunkIds(packedSnapshots[i].centerChunk);
            for (int e = 0; e < instanceLength; e++)
            {
                if (nearby.Contains(chunkSnapshots[e].chunkId)) 
                {
                    packedSnapshots[i].AddChunk(chunkSnapshots[e]);
                }
            }
        }

        return packedSnapshots;
    }

#endif
    }

public class QuickChunkData 
{
    public int q_chunkId;
    public List<Snapshot_Player> q_chunkPlayer = new List<Snapshot_Player>();
    public List<Snapshot_AI> q_chunkAI = new List<Snapshot_AI>();
    public List<Snapshot_WorldObject> q_chunkWorldObject = new List<Snapshot_WorldObject>();

    public static ChunkSnapshot CovertToSnapshot(QuickChunkData data) 
    {
        return new ChunkSnapshot()
        {
            chunkId = data.q_chunkId,
            players = data.q_chunkPlayer.ToArray(),
            ai = data.q_chunkAI.ToArray(),
            worldObjects = data.q_chunkWorldObject.ToArray()
        };
    }
}

public class PackedSnapshot
{
    //Server Only
    public List<ulong> clients;
    public int centerChunk;

    //Shared Data
    public int snapshotId;
    public float networkTime;

    public List<Snapshot_Player> players = new List<Snapshot_Player>(); 
    public List<Snapshot_AI> ai = new List<Snapshot_AI>();
    public List<Snapshot_WorldObject> worldObjects = new List<Snapshot_WorldObject>();

    public void AddChunk(ChunkSnapshot chunk) 
    {
        for (int i = 0; i < chunk.players.Length; i++)
        {
            players.Add(chunk.players[i]);
        }
        for (int i = 0; i < chunk.ai.Length; i++)
        {
            ai.Add(chunk.ai[i]);
        }
        for (int i = 0; i < chunk.worldObjects.Length; i++)
        {
            worldObjects.Add(chunk.worldObjects[i]);
        }
    }
}

public struct ChunkSnapshot
{
    public int chunkId;
    public Snapshot_Player[] players;
    public Snapshot_AI[] ai;
    public Snapshot_WorldObject[] worldObjects;
}

public struct Snapshot_Player 
{
    public ulong networkId;
    public ulong clientId;
    public int chunkId;
    public Vector3 location;
    public Vector2 rotation;
    public int holdId;
    public int holdState;
    public bool isRagdoll;
}

public struct Snapshot_AI 
{
    public ulong networkId;
    public int chunkId;
    public Vector3 location;
    public Vector2 rotation;
    public int holdId;
    public int holdState;
}

public struct Snapshot_WorldObject 
{
    public int chunkId;
    public int objectId;
    public int spawnId;
}



