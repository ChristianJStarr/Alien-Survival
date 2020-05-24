using MLAPI;
using UnityEngine;
using TMPro;

public class ServerView : MonoBehaviour
{
    public GameObject playerInterface, serverInterface, serverCamera;
    public TextMeshProUGUI onlineStatus, onlinePlayer;
    private NetworkingManager nw;
    
    void Start()
    {
        nw = NetworkingManager.Singleton;
        if (nw.IsServer)
        {
            serverInterface.SetActive(true);
            serverCamera.SetActive(true);
            playerInterface.SetActive(false);
        }
    }

    public void UpdateUI(string status, int players) 
    {
        onlineStatus.text = "Server Status: " + status;
        onlinePlayer.text = "Players Connected: " + players;
    }


    public void StopServer()
    {
        FindObjectOfType<GameServer>().StopServer();
    }
}
   
