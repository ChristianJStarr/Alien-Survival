using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Spawning;
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
    public WebServer webServer; //Web Server
    private MainMenuScript mainMenu; //Main Menu
    private ServerProperties storedProperties; //Stored Server Properties

    private string last_ipAddress;
    private ushort last_port;

    #region Debug Auto-Connect
#if UNITY_EDITOR
    [Space]
    [Space]
    [Tooltip("Auto-Connect On Scene Load")]
    public bool autoConnect = true;
    public string autoConnectIp = "10.0.0.211";
    public ushort autoConnectPort = 5055;
#endif
    #endregion

    private void Start()
    {
        if(NetworkingManager.Singleton != null) 
        {
            DontDestroyOnLoad(gameObject);
            networkManager = NetworkingManager.Singleton;
            #region Server Start
#if UNITY_SERVER
            StartServer();
            return;
#elif UNITY_EDITOR
            string[] data = Application.dataPath.Split('/');
            if (data[data.Length - 2].Contains("clone"))
            {
                StartServer();
            }
            else if (autoConnect)
            {
                networkManager.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(PlayerPrefs.GetInt("userId") + "," + PlayerPrefs.GetString("authKey") + "," + PlayerPrefs.GetString("username"));
                ((RufflesTransport.RufflesTransport)NetworkingManager.Singleton.NetworkConfig.NetworkTransport).ConnectAddress = autoConnectIp;
                ((RufflesTransport.RufflesTransport)NetworkingManager.Singleton.NetworkConfig.NetworkTransport).Port = (ushort)autoConnectPort;
                networkManager.OnClientConnectedCallback += PlayerConnected_Player;
                networkManager.OnClientDisconnectCallback += PlayerDisconnected_Player;
                networkManager.StartClient();
            }
#endif
            #endregion
        }
    }

    #region Client-Side

    //------Connecting

    //Client: Connect to Server
    public void ConnectToServer(string ip, ushort port)
    {
        if (mainMenu == null)
        {
            mainMenu = FindObjectOfType<MainMenuScript>();
        }
        if (mainMenu != null)
        {
            mainMenu.LoadGame();
            DebugMsg.Notify("Connecting to Server.", 1);
            last_ipAddress = ip;
            last_port = port;
            networkManager.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(PlayerPrefs.GetInt("userId") + "," + PlayerPrefs.GetString("authKey") + "," + PlayerPrefs.GetString("username"));
            ((RufflesTransport.RufflesTransport)NetworkingManager.Singleton.NetworkConfig.NetworkTransport).ConnectAddress = ip;
            ((RufflesTransport.RufflesTransport)NetworkingManager.Singleton.NetworkConfig.NetworkTransport).Port = (ushort)port;
            networkManager.OnClientConnectedCallback += PlayerConnected_Player;
            networkManager.OnClientDisconnectCallback += PlayerDisconnected_Player;
        }
    }
    
    //-----Callbacks
    
    //Callback: Connected
    private void PlayerConnected_Player(ulong clientId)
    {
        DebugMsg.Notify("Connected to Server.", 1);
    }
    //Callback: Disconnected
    private void PlayerDisconnected_Player(ulong id)
    {
        DebugMsg.Notify("Disconnected from Server.", 1);
        if (SceneManager.GetActiveScene().buildIndex != 1)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            if (mainMenu == null)
            {
                mainMenu = FindObjectOfType<MainMenuScript>();   
            }
            if (mainMenu != null)
            {
                mainMenu.ConnectingFailed();
            }
        }
    }
    public void RetryConnection() 
    {
        ConnectToServer(last_ipAddress, last_port);
    }
    #endregion

    #region Sever-Side

#if (UNITY_SERVER || UNITY_EDITOR)
//------Start/Stop

    //Server: Start Server
    public void StartServer()
    {
        Application.targetFrameRate = 20;
        gameServer = GameServer.singleton;
        storedProperties = new ServerProperties();
        if (GetServerSettings()) 
        {
            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += ServerStarted;
            networkManager.OnClientConnectedCallback += PlayerConnected_Server;
            networkManager.OnClientDisconnectCallback += PlayerDisconnected_Server;
            ((RufflesTransport.RufflesTransport)NetworkingManager.Singleton.NetworkConfig.NetworkTransport).ConnectAddress = storedProperties.publicIP;
            ((RufflesTransport.RufflesTransport)NetworkingManager.Singleton.NetworkConfig.NetworkTransport).Port = (ushort)storedProperties.serverPort;
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
        DebugMsg.Notify("Stopping the Server...", 1);
        UpdateServerList(false);
        gameServer.StopGameServer();
    }
    //Callback: Server Started
    private void ServerStarted()
    {
        UpdateServerList(true);
        NetworkSceneManager.SwitchScene("Primary");
    }


    //-----Callbacks

    //Callback: Player Conencted
    private void PlayerConnected_Server(ulong clientId)
    {
        gameServer.PlayerConnected(clientId);
        UpdatePlayerCount();
    }
    //Callback: Player Disconnected
    private void PlayerDisconnected_Server(ulong clientId)
    {
        PlayerInfo savedInfo = gameServer.Server_MovePlayerToInactive(clientId);
        if (savedInfo != null)
        {
            SavePlayerStats(savedInfo);
        }
        UpdatePlayerCount();
        gameServer.PlayerDisconnected(clientId);
    }
    //Approval Check
    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkingManager.ConnectionApprovedDelegate callback)
    {
        if(gameServer == null) { gameServer = GameServer.singleton; }
        bool approve = false;
        string connectData = System.Text.Encoding.ASCII.GetString(connectionData);
        string[] connectDataSplit = connectData.Split(',');
        int userId = Convert.ToInt32(connectDataSplit[0]);
        string authKey = connectDataSplit[1].ToString();
        string username = connectDataSplit[2].ToString();
        GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
        Vector3 spawnPoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length - 1)].transform.position;
        //Check if player has stored PlayerInfo

        if (gameServer.MovePlayerToActive(clientId, userId, authKey))
        {
            spawnPoint = gameServer.GetPlayerLocation(clientId);
            approve = true;
        }
        else
        {
            PlayerInfo newPlayer = new PlayerInfo
            {
                username = username,
                authKey = authKey,
                id = userId,
                health = 100,
                food = 100,
                water = 100,
                location = spawnPoint,
                clientId = clientId,
                coinsAdd = 0,
                expAdd = 0,
                hoursAdd = 0,
                time = DateTime.Now,
                isNew = true,
                inventory = new Inventory()
                {
                    blueprints = new int[5] {1,2,3,4,5}
                }
            };
            if (gameServer.CreatePlayer(newPlayer)) 
            {
                approve = true;       
            }
        } 
        callback(true, SpawnManager.GetPrefabHashFromGenerator("Alien"), approve, spawnPoint, Quaternion.identity);
    }
    
    
    //-----Server List

    //Update Server List
    private void UpdateServerList(bool value)
    {
        Server server = new Server
        {
            server_name = storedProperties.serverName,
            server_description = storedProperties.serverDescription,
            server_map = storedProperties.serverMap,
            server_mode = storedProperties.serverMode,
            server_players = 0,
            server_maxPlayers = storedProperties.serverMaxPlayer,
            server_Ip = storedProperties.publicIP,
            server_Port = storedProperties.serverPort
        };
        if (!value)
        {
            server.server_Ip = "REMOVE";
        }
        if (webServer != null)
        {
            webServer.ServerListSet(server, returnValue =>
            {
                if (returnValue)
                {
                    DebugMsg.Notify("Updated Server List.", 4);
                }
            });
        }
        StartCoroutine(ServerListLoop());
    }
    //Server List Loop
    private IEnumerator ServerListLoop() 
    {
        yield return new WaitForSeconds(480);
        if(webServer != null) 
        {
            webServer.ServerListUpdateRecent(storedProperties.publicIP, storedProperties.serverPort, onRequestFinished => 
            {
                if (onRequestFinished) 
                {
                    DebugMsg.Notify("Updated Server List.", 4);
                }
            });
        }
        StartCoroutine(ServerListLoop());
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
                    DebugMsg.Notify("Updating Server List, Player Count.", 4);
                }
                else
                {
                    DebugMsg.Notify("Updating Server List Failed", 4);
                }
            });
        }
    }


    //-----Settings Data

    //Save Player Statistics
    private void SavePlayerStats(PlayerInfo savedInfo) 
    {
        string notifyData = storedProperties.serverName + "," + savedInfo.hoursAdd + "," + savedInfo.expAdd + "," + savedInfo.coinsAdd;
        if(webServer != null) 
        {
            webServer.SetClientStats(savedInfo.id, savedInfo.authKey, savedInfo.expAdd, savedInfo.coinsAdd, savedInfo.hoursAdd, notifyData, "", savedInfo.kills, savedInfo.deaths, returnValue => 
            {
                if (returnValue) 
                {
                    DebugMsg.Notify("Saving Player Statistics Successful.", 4);
                }
                else 
                {
                    DebugMsg.Notify("Failed Saving Player Statistics.", 4);
                }
            });
        }
    }    
    //Get the Stored Server Settings
    private bool GetServerSettings()
    {
        string path = Application.dataPath + "/server-properties.txt";
        if (File.Exists(path))
        {
            ServerProperties serverProperties = JsonUtility.FromJson<ServerProperties>(File.ReadAllText(path));
            if (!CheckSettingsFile(serverProperties))
            {
                storedProperties = serverProperties;
                return true;
            }
            else
            {
                DebugMsg.Notify("Please Fix 'server-properties.txt'.", 1);
                return false;
            }
        }
        else
        {
            ServerProperties sp = new ServerProperties();
            string defaultSettings = JsonUtility.ToJson(sp);
            File.WriteAllText(path, defaultSettings);
            storedProperties = sp;
            return true;
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
        if (sp.publicIP.Length == 0) { empty = true; }
        if (sp.serverPort == 0) { empty = true; }
        if (sp.serverMaxPlayer == 0) { empty = true; }
        return empty;
    }

#endif

    #endregion
}

public class ServerProperties 
{
    //Primary Properties
    public string serverName = "Default Server Name";
    public string serverDescription = "Default Description";
    public string serverMode = "Default Gamemode";
    public string serverMap = "Default Map";
    public string publicIP = "0.0.0.0";
    public ushort serverPort = 7708;
    public int serverMaxPlayer = 30;
    public int autoSaveInterval = 5;

    //Player Info System
    public int[] defaultPlayerBlueprints = new int[] { 1, 2 };

    //World AI System
    public int ai_MaxEnemies = 0;
    public int ai_MaxFriendly = 0;
    public int ai_EnemyAttackRadius = 80;
    public int ai_EnemyWanderRadius = 800;
    public int ai_RespawnTime = 600; //10 Minutes
    public int ai_StateUpdateTime = 1; // 1 Second
    public int ai_WalkSpeed = 1;
    public int ai_RunSpeed = 5;

    //World Object System
    public int wo_maxTree = 2000;
    public int wo_maxRock = 0;
    public int wo_maxLoot = 0;
    public int wo_respawnTrees = 60;
    public int wo_respawnRocks = 60;
    public int wo_respawnLoot = 60;
}
