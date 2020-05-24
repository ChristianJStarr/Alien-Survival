using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;
using UnityEngine;


public class GameServer : MonoBehaviour
{
    public WebServer webServer;
    public string serverName = "Development Server #1";
    public string serverDescription = "This is a development server.";
    public string serverMode = "Default Mode";
    public string serverMap = "Default Map";
    public string serverIP = "127.0.0.1";
    public ushort serverPort = 7777;
    public int serverMaxPlayer = 200;
    public bool devServer = false;

    private void Start()
    {
        webServer = GetComponent<WebServer>();
#if UNITY_SERVER
        StartServer();
#endif
#if UNITY_EDITOR
        if (devServer) 
        {
            StartServer();
        }
#endif
    }
    /// <summary>
    /// Start Client & Connect to Server.
    /// </summary>
    /// <param name="ip">Server IP Address</param>
    /// <param name="port">Server Port</param>
    public void ConnectToServer(string ip, ushort port)
    {
        NetworkingManager.Singleton.GetComponent<UnetTransport>().ConnectAddress = ip;


        NetworkingManager.Singleton.GetComponent<UnetTransport>().ConnectPort = port;
        NetworkingManager.Singleton.StartClient();
    }
    /// <summary>
    /// Start Server as Server
    /// </summary>
    public void StartServer()
    {
        NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkingManager.Singleton.OnServerStarted += ServerStarted;
        NetworkingManager.Singleton.OnClientConnectedCallback += PlayerConnected;
        NetworkingManager.Singleton.OnClientDisconnectCallback += PlayerDisconnected; 
        NetworkingManager.Singleton.StartServer();
    }
    public void StopServer() 
    {
        NetworkingManager.Singleton.StopServer();
        UpdateServerList(false);
    }
    private void PlayerConnected(ulong obj)
    {
        Debug.Log("Network - Game - Player Connected.");
        UpdatePlayerCount();
    }
    private void PlayerDisconnected(ulong obj)
    {
        Debug.Log("Network - Game - Player Disconnected.");
        UpdatePlayerCount();
    }
    private void ServerStarted() 
    {
        Debug.Log("Network - Game - Server Started.");
        UpdateServerList(true);
        NetworkSceneManager.SwitchScene("Primary");
    }
    /// <summary>
    /// Approval Check Callback.
    /// </summary>
    /// <param name="connectionData"></param>
    /// <param name="clientId"></param>
    /// <param name="callback"></param>
    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkingManager.ConnectionApprovedDelegate callback) 
    {
        //Your logic here
        bool approve = true;
        bool createPlayerObject = true;
        Debug.Log("Network - Game - Approval Check.");
        ulong? prefabHash = SpawnManager.GetPrefabHashFromGenerator(null); // The prefab hash. Use null to use the default player prefab
        //Find array of available spawn points.
        GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
        //Pick a random point in array of available.
        Vector3 spawnPoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length)].transform.position; 
        //If approve is true, the connection gets added. If it's false. The client gets disconnected
        callback(createPlayerObject, prefabHash, approve, spawnPoint, Quaternion.identity);
    }
    /// <summary>
    /// Update the Server list with stored settings.
    /// </summary>
    private void UpdateServerList(bool value) 
    {
        
        Server server = new Server();
        server.name = serverName;
        server.description = serverDescription;
        server.map = serverMap;
        server.mode = serverMode;
        server.player = 0;
        server.maxPlayer = serverMaxPlayer;
        server.serverIP = serverIP;
        server.serverPort = serverPort;
        if (!value)
        {
            server.serverIP = "REMOVE";
        }
        if (webServer != null)
        {
            webServer.ServerListSend(server, returnValue =>
            {
                if (returnValue)
                {
                    //Server List updated successfully
                    Debug.Log("Network - Game - Updated Server List.");
                }
            });
        }
        else 
        {
            Debug.Log("Network - Game - Web Server is null");
        }
    }
    /// <summary>
    /// Update the current player count on the server list.
    /// </summary>
    private void UpdatePlayerCount() 
    {

        int count = NetworkingManager.Singleton.ConnectedClients.Count;
        if (webServer != null)
        {
            webServer.ServerListPlayerCount(serverName, count, returnValue =>
            {
                if (returnValue)
                {
                    //Player Count updated successfully
                    Debug.Log("Network - Game - Updated Player Count.");
                }
            });
        }
    }
}
