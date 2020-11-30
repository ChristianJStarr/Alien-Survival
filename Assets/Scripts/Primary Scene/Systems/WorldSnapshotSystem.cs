using MLAPI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldSnapshotSystem : MonoBehaviour
{
    private bool systemEnabled = false;
 
    public PlayerCommandSystem commandSystem;
    public WorldAISystem worldAISystem;

    private GameServer gameServer;
    private NetworkingManager networkingManager;
    private int _snapshotId = 1;


    private QuickAccess_Snapshot lastSnapshot;



    //Start System
    public bool StartSystem() 
    {
        if(NetworkingManager.Singleton != null)
        {
            networkingManager = NetworkingManager.Singleton;
            gameServer = GameServer.singleton;
            systemEnabled = true;

            return true;
        }
        else 
        {
            return false;
        }
    }

    //Stop System
    public bool StopSystem() 
    {
        systemEnabled = false;
        return true;
    }


    //Send Out Snapshot 20/second
    private void Update() 
    {
        if (systemEnabled) 
        {
            QuickAccess_Snapshot quickSnapshot = new QuickAccess_Snapshot();
            quickSnapshot.players = new Dictionary<ulong, Snapshot_Player>();
            quickSnapshot.ai = new Dictionary<ulong, Snapshot_AI>();

            //Convert Players to Snapshot
            foreach (PlayerControlObject controlObject in commandSystem.players.Values)
            {
                if (controlObject != null)
                {
                    Snapshot_Player snapshotPlayer = controlObject.ConvertToSnapshot();
                    if(PlayerHasChanged(snapshotPlayer)) 
                    {
                        quickSnapshot.players.Add(snapshotPlayer.networkId, snapshotPlayer);
                    }
                }
            }

            for (int i = 0; i < worldAISystem.ai.Count; i++)
            {
                if(worldAISystem.ai[i] != null) 
                {
                    Snapshot_AI snapshotAI = worldAISystem.ai[i].ConvertToSnapshot();
                    if (AIHasChanged(snapshotAI))
                    {
                        quickSnapshot.ai.Add(snapshotAI.networkId, snapshotAI);
                    }
                }
            }

            quickSnapshot.networkTime = networkingManager.NetworkTime;
            quickSnapshot.snapshotId = _snapshotId;
            gameServer.ServerSendSnapshot(new Snapshot()
            {
                snapshotId = quickSnapshot.snapshotId,
                players = quickSnapshot.players.Values.ToArray(),
                ai = quickSnapshot.ai.Values.ToArray(),
                networkTime = quickSnapshot.networkTime
            });
            _snapshotId++;
            lastSnapshot = quickSnapshot;
        }



    }


    private bool PlayerHasChanged(Snapshot_Player player) 
    {
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

}

public class QuickAccess_Snapshot
{
    public int snapshotId;
    public float networkTime;
    public Dictionary<ulong, Snapshot_Player> players = new Dictionary<ulong, Snapshot_Player>();
    public Dictionary<ulong, Snapshot_AI> ai = new Dictionary<ulong, Snapshot_AI>();
    
}


public class Snapshot 
{
    public int snapshotId;
    public float networkTime;
    public Snapshot_Player[] players = new Snapshot_Player[0];
    public Snapshot_AI[] ai = new Snapshot_AI[0];



    public static QuickAccess_Snapshot ConvertQuick(Snapshot snapshot)
    {
        QuickAccess_Snapshot quickAccess = new QuickAccess_Snapshot()
        {
            snapshotId = snapshot.snapshotId,
            networkTime = snapshot.networkTime
        };


        if (snapshot.players != null)
        {
            Dictionary<ulong, Snapshot_Player> _players = new Dictionary<ulong, Snapshot_Player>();
            for (int i = 0; i < snapshot.players.Length; i++)
            {
                _players.Add(snapshot.players[i].networkId, snapshot.players[i]);
            }
            if (_players.Count > 0)
            {
                quickAccess.players = _players;
            }
        }

        if (snapshot.ai != null)
        {
            Dictionary<ulong, Snapshot_AI> _ai = new Dictionary<ulong, Snapshot_AI>();
            for (int i = 0; i < snapshot.ai.Length; i++)
            {
                _ai.Add(snapshot.ai[i].networkId, snapshot.ai[i]);
            }
            if (_ai.Count > 0)
            {
                quickAccess.ai = _ai;
            }
        }

        return quickAccess;
    }

    public static Snapshot TrimForDistance(Vector3 playerLocation, Snapshot snapshot) 
    {
        List<Snapshot_Player> players = new List<Snapshot_Player>(); 
        if(snapshot.players != null) 
        {
            for (int i = 0; i < snapshot.players.Length; i++)
            {
                if(Vector3.Distance(snapshot.players[i].location, playerLocation) < 400) 
                {
                    players.Add(snapshot.players[i]);
                }
            }
            snapshot.players = players.ToArray();
        }

        List<Snapshot_AI> ai = new List<Snapshot_AI>();
        if (snapshot.ai != null) 
        {
            for (int i = 0; i < snapshot.ai.Length; i++)
            {
                if (Vector3.Distance(snapshot.ai[i].location, playerLocation) < 400)
                {
                    ai.Add(snapshot.ai[i]);
                }
            }
            if(ai.Count > 0) 
            {
                snapshot.ai = ai.ToArray();
            }
            else
            {
                snapshot.ai = null;
            }
        }

        return snapshot;
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



