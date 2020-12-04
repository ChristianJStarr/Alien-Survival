using MLAPI;
using TMPro;
using UnityEngine;

public class ServerUI_Controller : MonoBehaviour
{

    public TextMeshProUGUI statusText, snapshotText, commandText;

    private bool isRunning = false;


    private float snapshotMax;



    // Update is called once per frame
    void Update()
    {
        if(NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsServer) 
        {
            SetServerStatus(true);
            UpdateCommandStats();
            UpdateSnapshotStats();
        }
        else 
        {
            SetServerStatus(false);
        }
    }



    private void SetServerStatus(bool online) 
    {
        if(isRunning != online) 
        {
            isRunning = online;
            
            if (isRunning)
            {
                statusText.text = "RUNNING";
                statusText.color = new Color32(93,255,137,255);
            }
            else
            {
                statusText.text = "OFFLINE";
                statusText.color = new Color32(255, 112, 94, 255);
            }
        }
    }

    public void StopServer() 
    {
        if (GameServer.singleton != null) GameServer.singleton.StopGameServer();
    }

    public void UpdateSnapshotStats() 
    {
        int snapshot_Id = GameServer.singleton.DebugSnapshotId;
        float snapshot_Size = GameServer.singleton.DebugSnapshotSize;
        if(snapshot_Size > snapshotMax) 
        {
            snapshotMax = snapshot_Size;    
        }
        snapshotText.text = "Snapshot Id: " + snapshot_Id + " Size: " + snapshot_Size / 1000 + "kb  Max:" + snapshotMax / 1000 + "kb";
    }

    public void UpdateCommandStats() 
    {
        int commandsPer = GameServer.singleton.DebugCommandPerSecond;
        commandText.text = "Commands: " + commandsPer + "/s";
    }
}
