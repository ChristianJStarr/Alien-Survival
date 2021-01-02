using MLAPI;
using TMPro;
using UnityEngine;

public class ServerUI_Controller : MonoBehaviour
{
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
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
        string size = snapshot_Size + "b";
        if(snapshot_Size > 1000) { size = snapshot_Size / 1000 + "kb"; }
        string max = snapshotMax + "b";
        if (snapshotMax > 1000) { max = snapshotMax / 1000 + "kb"; }
        snapshotText.text = "Snapshot Id: " + snapshot_Id + " Size: " + size + " Max: " + max;
    }

    public void UpdateCommandStats() 
    {
        int commandsPer = GameServer.singleton.DebugCommandPerSecond;
        int commandSize = GameServer.singleton.DebugCommandSize;
        string size = commandSize + "b";
        if(commandSize > 1000) { size = commandSize / 1000 + "kb"; }
        commandText.text = "Commands: " + commandsPer + "/s Size: " + size;
    }
#endif
}
