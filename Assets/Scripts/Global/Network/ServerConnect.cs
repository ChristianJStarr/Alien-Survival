using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ServerConnect : MonoBehaviour
{

    #region Singleton

    public static ServerConnect singleton;
    void Awake()
    {
        //Singleton Init
        singleton = this;
    }

    #endregion

    private NetworkingManager networkManager; //Networking Manager
    private GameServer gameServer; //Game Server
    private WebServer webServer; //Web Server
    public MainMenuScript mainMenu; //Main Menu

    public bool devServer = false; //If development Server
    private ServerProperties storedProperties; //Stored Server Properties
    [SerializeField]
    private int logLevel = 3; //LogLevel
    private string storedIp = "";
    private void Start()
    {
        networkManager = NetworkingManager.Singleton;
        DontDestroyOnLoad(this.gameObject);
#if UNITY_SERVER
        if(networkManager != null)
        {
            StartServer();    
        }
#endif
#if UNITY_EDITOR
        if (devServer && networkManager != null)
        {
            StartServer();
        }
#endif
    }

    //-----------------------------------------------------------------//
    //                       Client Side Connect                       //
    //-----------------------------------------------------------------//
    
    //Request Ping
    public void RequestPing(Action<int> callback) 
    {
        StartCoroutine(StartPing(returnValue => { callback(returnValue); }));
    }
    //Request Ping Wait
    private IEnumerator StartPing(Action<int> callback)
    {
        int count = 0;
        int pingTime = 1;
        WaitForSeconds f = new WaitForSeconds(0.05F);
        Ping ping = new Ping(storedIp);
        while (!ping.isDone)
        {
            if (count >= 10)
            {
                pingTime = 0;
                break;
            }
            count++;
            yield return f;
        }
        if(pingTime != 0) 
        {
            pingTime = ping.time;
        }
        callback(pingTime);
    }



    //Client: Connect to Server
    public void ConnectToServer(string ip, ushort port)
    {
        storedIp = ip;
        if(mainMenu == null) 
        {
            mainMenu = FindObjectOfType<MainMenuScript>();
        }
        mainMenu.LoadingScreen(true);
        DebugMessage("Connecting to Server.", 1);
        StartCoroutine(ConnectionWait(ip, port));
    }

    //Connection Wait
    private IEnumerator ConnectionWait(string ip, ushort port) 
    {
        yield return new WaitForSeconds(5f);
        networkManager.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(PlayerPrefs.GetInt("userId") + "," + PlayerPrefs.GetString("authKey") + "," + PlayerPrefs.GetString("username"));
        networkManager.GetComponent<UnetTransport>().ConnectAddress = ip;
        networkManager.GetComponent<UnetTransport>().ConnectPort = port;
        networkManager.OnClientConnectedCallback += PlayerConnected_Player;
        networkManager.OnClientDisconnectCallback += PlayerDisconnected_Player;
        networkManager.StartClient();
    }
    
    //Callback: Connected
    private void PlayerConnected_Player(ulong id) 
    {
        GameServer.singleton.PlayerConnected_Player(id);
    }
    
    //Callback: Disconnected
    private void PlayerDisconnected_Player(ulong id)
    {
        DebugMessage("Disconnected from Server.", 1);
        
        if(SceneManager.GetActiveScene().name != "MainMenu") 
        {
            if(GameServer.singleton != null) 
            {
                GameServer.singleton.PlayerDisconnected_Player(id);
            }
            SceneManager.LoadScene(1);
        }
        else 
        {
            if (mainMenu == null)
            {
                mainMenu = FindObjectOfType<MainMenuScript>();
            }
            mainMenu.LoadingScreen(false);
        }
        
    }

    //-----------------------------------------------------------------//
    //                       Server Side Connect                       //
    //-----------------------------------------------------------------//
    
    //Server: Start Server
    public void StartServer()
    {
        storedProperties = new ServerProperties();
        webServer = GetComponent<WebServer>();
        if (GetServerSettings()) 
        {
            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += ServerStarted;
            networkManager.OnClientConnectedCallback += PlayerConnected_Server;
            networkManager.OnClientDisconnectCallback += PlayerDisconnected_Server;
            networkManager.GetComponent<UnetTransport>().MaxConnections = storedProperties.serverMaxPlayer;
            networkManager.GetComponent<UnetTransport>().ConnectAddress = storedProperties.serverIP;
            networkManager.GetComponent<UnetTransport>().ConnectPort = storedProperties.serverPort;
            networkManager.GetComponent<UnetTransport>().ServerListenPort = storedProperties.serverPort;
            networkManager.StartServer();
        }
    }

    //Get Server Properties
    public ServerProperties GetServerProperties() 
    {
        return storedProperties;
    }
    
    //Server: Stop Server
    public void StopServer()
    {
        UpdateServerList(false);
        GameServer.singleton.StopGameServer();
    }
    
    //Approval Check
    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkingManager.ConnectionApprovedDelegate callback)
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
        if(gameServer == null) 
        {
            gameServer = GameServer.singleton;
        }
        
        //Check if player has active PlayerInfo
        if (gameServer.activePlayers != null)
        {
            for (int i = 0; i < gameServer.activePlayers.Count; i++)
            {
                if (gameServer.activePlayers[i].id == id)
                {
                    DebugMessage("Player '" + username + "' Tried to Connect but has Active Player Info.", 1);
                    approve = false;
                    break;
                }
            }
        }


        if (approve)
        {
            //Check if player has stored PlayerInfo
            if (gameServer.inactivePlayers != null)
            {
                for (int i = 0; i < gameServer.inactivePlayers.Count; i++)
                {
                    PlayerInfo player = gameServer.inactivePlayers[i];
                    if (player.id == id)
                    {
                        if (player.authKey == authKey)
                        {
                            gameServer.ActiveManage(player, true);
                            gameServer.InactiveManage(player, false);
                            spawnPoint = player.location;
                            noData = false;
                            break;
                        }
                        else
                        {
                            DebugMessage("Player '" + username + "' Tried to Connect with Invalid AuthKey.", 1);
                            approve = false;
                            noData = false;
                            break;
                        }
                    }
                }
            }
            //Else make new PlayerInfo for this player
            if (noData)
            {
                PlayerInfo newPlayer = new PlayerInfo
                {
                    name = username,
                    authKey = authKey,
                    id = id,
                    health = 100,
                    food = 100,
                    water = 100,
                    location = spawnPoint,
                    clientId = clientId
                };
                gameServer.ActiveManage(newPlayer, true);
                DebugMessage("Created New Player Data for '" + username + "'.", 1);
                approve = true;
            }
        }
        
        bool createPlayerObject = true;
        ulong? prefabHash = SpawnManager.GetPrefabHashFromGenerator("Alien");
        callback(createPlayerObject, prefabHash, approve, spawnPoint, Quaternion.identity);
    }
    
    //Callback: Server Started
    private void ServerStarted() 
    {
        UpdateServerList(true);
        NetworkSceneManager.SwitchScene("Primary");
    }
    
    //Update the Server List
    private void UpdateServerList(bool value)
    {

        Server server = new Server
        {
            name = storedProperties.serverName,
            description = storedProperties.serverDescription,
            map = storedProperties.serverMap,
            mode = storedProperties.serverMode,
            player = 0,
            maxPlayer = storedProperties.serverMaxPlayer,
            serverIP = storedProperties.serverIP,
            serverPort = storedProperties.serverPort
        };
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
                    DebugMessage("Updating Server List.", 1);
                }
            });
        }
    }

    //Callback: Player Conencted
    private void PlayerConnected_Server(ulong id)
    {
        UpdatePlayerCount();
    }

    //Callback: Player Disconnected
    private void PlayerDisconnected_Server(ulong id) 
    {
        GameServer.singleton.MovePlayerToInactive(id);
        UpdatePlayerCount();
    }

    //Update the Player Count on the Server List
    private void UpdatePlayerCount()
    {

        int count = networkManager.ConnectedClients.Count;
        if (webServer != null)
        {
            webServer.ServerListPlayerCount(storedProperties.serverName, count, returnValue =>
            {
                if (returnValue)
                {
                    //Player Count updated successfully
                    DebugMessage("Updating Server List, Player Count.", 1);
                }
            });
        }
    }
    
    //Get the Stored Server Settings
    private bool GetServerSettings()
    {
        string path = @"C:\Settings\server-properties.txt".Replace('\\', Path.DirectorySeparatorChar);
        if (!File.Exists(path)) 
        {
            DebugMessage("No Server Properties Found at 'C:/Settings/server-properties.txt'.", 1);
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
                    storedProperties.serverName = serverProperties.serverName;
                    storedProperties.serverDescription = serverProperties.serverDescription;
                    storedProperties.serverMode = serverProperties.serverMode;
                    storedProperties.serverMap = serverProperties.serverMap;
                    storedProperties.serverIP = serverProperties.serverIP;
                    storedProperties.serverPort = serverProperties.serverPort;
                    storedProperties.serverMaxPlayer = serverProperties.serverMaxPlayer;
                    storedProperties.maxEnemies = serverProperties.maxEnemies;
                    storedProperties.maxFriendly = serverProperties.maxFriendly;
                    storedProperties.autoSaveInterval = serverProperties.autoSaveInterval;
                    return true;
                }
                else 
                {
                    DebugMessage("Please Fix 'server-properties.txt'.", 1);
                    return false;
                }
            }
            else 
            {
                DebugMessage("Please Fix 'server-properties.txt'.", 1);
                return false;
            }
        }

    }
    
    //Check the Settings file 
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
    
    //Debug Message
    private void DebugMessage(string message, int level)
    {
        if (networkManager != null) 
        {
            if (level <= logLevel)
            {
                if (networkManager.IsServer)
                {
                    Debug.Log("[Server] ServerConnect : " + message);
                }
                else
                {
                    Debug.Log("[Client] ServerConnect : " + message);
                }
            }
        }
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
    public int autoSaveInterval;
}
