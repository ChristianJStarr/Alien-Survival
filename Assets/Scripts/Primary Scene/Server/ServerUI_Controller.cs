using MLAPI;
using TMPro;
using UnityEngine;

public class ServerUI_Controller : MonoBehaviour
{

    public TextMeshProUGUI statusText;

    private bool isRunning = false;

    // Update is called once per frame
    void Update()
    {
        if(NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsServer) 
        {
            SetServerStatus(true);
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
}
