using MLAPI;
using System.Collections.Generic;
using UnityEngine;

public class WorldSnapshotManager : NetworkedBehaviour
{
    public static WorldSnapshotManager Singleton;

    public Dictionary<ulong, PlayerControlObject> players = new Dictionary<ulong, PlayerControlObject>();

    private Snapshot[] snapshots;

    private int currentSnapshotId;

    private void Awake() 
    {
        Singleton = this;
    }

    void Start()
    {
        snapshots = new Snapshot[3];
        if (IsServer) { Destroy(this); }
    }

    public void ProcessSnapshot(Snapshot snapshot)
    {
        snapshots[0] = snapshots[1];
        snapshots[1] = snapshots[2];
        if(snapshots[3] != snapshot)
            snapshots[3] = snapshot;
        snapshots[2] = snapshots[3];
        currentSnapshotId = snapshot.snapshotId;
        ApplySnapshot();
    }


    private void ApplySnapshot() 
    {
        DebugMsg.Begin(303, "Applying World Snapshot...", 3);
        for (int i = 0; i < snapshots[3].players.Length; i++)
        {
            ulong snap_networkId = snapshots[3].players[i].networkId;
            if (players.ContainsKey(snapshots[3].players[i].networkId)) 
            {
                if(players[snap_networkId].transform.position == snapshots[3].players[i].location) 
                {
                    players[snap_networkId].transform.position = snapshots[3].players[i].location;
                }
                if (players[snap_networkId].transform.rotation == Quaternion.Euler(snapshots[3].players[i].rotation)) 
                {
                    players[snap_networkId].transform.rotation = Quaternion.Euler(snapshots[3].players[i].rotation);
                }
            }
            else 
            {
                PlayerControlObject playerControlObject = GetNetworkedObject(snap_networkId).GetComponent<PlayerControlObject>();
                if(playerControlObject != null) 
                {
                    players.Add(snap_networkId, playerControlObject);
                    
                    if (playerControlObject.transform.position == snapshots[3].players[i].location)
                    {
                        playerControlObject.transform.position = snapshots[3].players[i].location;
                    }
                    if (playerControlObject.transform.rotation == Quaternion.Euler(snapshots[3].players[i].rotation))
                    {
                        playerControlObject.transform.rotation = Quaternion.Euler(snapshots[3].players[i].rotation);
                    }
                }
            }
        }
        DebugMsg.End(303, "Finished Applying World Snapshot.", 3);
    }

}
