using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerConnect : MonoBehaviour
{
    private NetworkingManager networkManager;
    private GameServer gameServer;
    private WebServer webServer;

    public bool devServer = false;
    private string serverName, serverDescription, serverMode, serverMap, serverIP;
    private int serverMaxPlayer, maxEnemies, maxFriendly;
    private ushort serverPort;
    

    private void Start()
    {
        networkManager = NetworkingManager.Singleton;
        DontDestroyOnLoad(this.gameObject);
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

    //-----------------------------------------------------------------//
    //                       Client Side Connect                       //
    //-----------------------------------------------------------------//
    public void ConnectToServer(string ip, ushort port)
    {
        networkManager.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(PlayerPrefs.GetInt("id") + "," + PlayerPrefs.GetString("authKey") + "," + PlayerPrefs.GetString("username"));
        networkManager.GetComponent<UnetTransport>().ConnectAddress = ip;
        networkManager.GetComponent<UnetTransport>().ConnectPort = port;
        networkManager.OnClientConnectedCallback += PlayerConnected_Player;
        networkManager.OnClientDisconnectCallback += PlayerDisconnected_Player;
        networkManager.StartClient();
    }
    private void PlayerConnected_Player(ulong id) 
    {
        GameServer.singleton.PlayerDisconnected_Player(id);
    }
    private void PlayerDisconnected_Player(ulong id)
    {
        Debug.Log("Network - Game - Disconnected from Server.");
        SceneManager.LoadScene(1);
    }

    //-----------------------------------------------------------------//
    //                       Server Side Connect                       //
    //-----------------------------------------------------------------//
    public void StartServer()
    {
        webServer = GetComponent<WebServer>();
        if (GetServerSettings()) 
        {
            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += ServerStarted;
            networkManager.OnClientConnectedCallback += PlayerConnected_Server;
            networkManager.OnClientDisconnectCallback += PlayerDisconnected_Server;
            networkManager.GetComponent<UnetTransport>().MaxConnections = serverMaxPlayer;
            networkManager.GetComponent<UnetTransport>().ConnectAddress = serverIP;
            networkManager.GetComponent<UnetTransport>().ConnectPort = serverPort;
            networkManager.GetComponent<UnetTransport>().ServerListenPort = serverPort;
            networkManager.StartServer();
        }
    }
    public void StopServer()
    {
        UpdateServerList(false);
        GameServer.singleton.StopGameServer();
    }
    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkingManager.ConnectionApprovedDelegate callback)
    {
        bool approve = true;
        bool noData = true;
        string connectData = System.Text.Encoding.ASCII.GetString(connectionData);
        string[] connectDataSplit = connectData.Split(',');
        int id = Convert.ToInt32(connectDataSplit[0]);
        string authKey = connectDataSplit[1].ToString();
        string username = connectDataSplit[2].ToString();
        GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
        Vector3 spawnPoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length)].transform.position;
        GameServer gs = GameServer.singleton;
        
        foreach (PlayerInfo player in gs.activePlayers)
        {
            //Check if player has active PlayerInfo
            if (player.id == id)
            {
                Debug.Log("Network - Server - Player: " + username + " attempted to connect but has active player info.");
                approve = false;
                break;
            }
        }
        if (approve)
        {
            if (gs.inactivePlayers != null)
            {
                foreach (PlayerInfo player in gs.inactivePlayers)
                {
                    if (player.id == id)
                    {
                        if (player.authKey == authKey)
                        {
                            gs.ActiveManage(player, true);
                            gs.InactiveManage(player, false);
                            spawnPoint = player.location;
                            noData = false;
                            //Player is approved.
                            break;
                        }
                        else
                        {
                            Debug.Log("Network - Server - Player: " + username + " has stored player info but diffrent auth key.");
                            approve = false;
                            noData = false;
                            break;
                        }
                    }
                }
            }
            //Check if player has stored PlayerInfo

            //Else make new PlayerInfo for this player
            if (noData)
            {
                PlayerInfo newPlayer = new PlayerInfo();
                newPlayer.name = username;
                newPlayer.authKey = authKey;
                newPlayer.id = id;
                newPlayer.health = 100;
                newPlayer.food = 100;
                newPlayer.water = 100;
                newPlayer.location = spawnPoint;
                newPlayer.clientId = clientId;
                gs.ActiveManage(newPlayer, true);
                Debug.Log("Network - Server - New Player data created for player: " + username);
                approve = true;
            }
        }
        bool createPlayerObject = true;
        ulong? prefabHash = SpawnManager.GetPrefabHashFromGenerator("Alien");
        callback(createPlayerObject, prefabHash, approve, spawnPoint, Quaternion.identity);
    }
    private void ServerStarted() 
    {
        UpdateServerList(true);
        NetworkSceneManager.SwitchScene("Primary");
    }
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
                    Debug.Log("ServerUpdate Server list Successful");
                }
            });
        }
        else
        {
        }
    }
    private void PlayerConnected_Server(ulong id)
    {
        UpdatePlayerCount();
    }
    private void PlayerDisconnected_Server(ulong id) 
    {
        GameServer.singleton.MovePlayerToInactive(id);
        UpdatePlayerCount();
    }
    private void UpdatePlayerCount()
    {

        int count = networkManager.ConnectedClients.Count;
        if (webServer != null)
        {
            webServer.ServerListPlayerCount(serverName, count, returnValue =>
            {
                if (returnValue)
                {
                    //Player Count updated successfully
                    Debug.Log("Network - Server - Updated Player Count.");
                }
            });
        }
    }
    private bool GetServerSettings()
    {
        string path = @"C:\Settings\server-properties.txt".Replace('\\', Path.DirectorySeparatorChar);
        if (!File.Exists(path)) 
        {

            Debug.LogError("No Server Properties Found at C:/Settings/server-properties.txt");
            return false;
        }
        else 
        {
            string storedSettings = File.ReadAllText(path);
            if (storedSettings.Length > 0)
            {
                ServerProperties serverProperties = JsonUtility.FromJson<ServerProperties>(storedSettings);
                if (!CheckSettingsFile(serverProperties)) 
                {
                    serverName = serverProperties.serverName;
                    serverDescription = serverProperties.serverDescription;
                    serverMode = serverProperties.serverMode;
                    serverMap = serverProperties.serverMap;
                    serverIP = serverProperties.serverIP;
                    serverPort = serverProperties.serverPort;
                    serverMaxPlayer = serverProperties.serverMaxPlayer;
                    maxEnemies = serverProperties.maxEnemies;
                    maxFriendly = serverProperties.maxFriendly;
                    return true;
                }
                else 
                {
                    Debug.LogError("Please Fix server-properties.txt");
                    return false;
                }
            }
            else 
            {
                Debug.LogError("Please Fix server-properties.txt");
                return false;
            }
        }

    }
    private bool CheckSettingsFile(ServerProperties sp) 
    {
        bool empty = false;
        if(sp.serverName.Length == 0) { empty = true; }
        if (sp.serverDescription.Length == 0) { empty = true; }
        if (sp.serverMode.Length == 0) { empty = true; }
        if (sp.serverMap.Length == 0) { empty = true; }
        if (sp.serverIP.Length == 0) { empty = true; }
        if (sp.serverPort == 0) { empty = true; }
        if (sp.serverMaxPlayer == 0) { empty = true; }
        return empty;
    }
}

public class ServerProperties 
{
    public string serverName;
    public string serverDescription;
    public string serverMode;
    public string serverMap;
    public string serverIP;
    public ushort serverPort;
    public int serverMaxPlayer;
    public int maxEnemies;
    public int maxFriendly;
}
