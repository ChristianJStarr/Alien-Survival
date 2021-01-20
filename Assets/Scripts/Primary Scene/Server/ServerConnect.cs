using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Serialization.Pooled;
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

    private Vector3[] spawnpointPositions;
    private ulong playerPrefabHash;


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
#elif (UNITY_EDITOR && !UNITY_CLOUD_BUILD)
            string[] data = Application.dataPath.Split('/');
            if (data[data.Length - 2].Contains("clone"))
            {
                StartServer();
            }
#endif
#endregion
        }
    }

    #region Client-Side

    //Client: Connect to Server
    public void ConnectToServer(string ip, ushort port)
    {
        DebugMsg.Notify("Connecting to Server.", 1);
        last_ipAddress = ip;
        last_port = port;
        if (mainMenu == null)
        {
            mainMenu = FindObjectOfType<MainMenuScript>();
        }
        if (mainMenu != null)
        {
            mainMenu.LoadGame();
            using (PooledBitStream writeStream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
                {
                    writer.WriteStringPacked(PlayerPrefs.GetString("username"));
                    writer.WriteStringPacked(PlayerPrefs.GetString("authKey"));
                    writer.WriteInt32Packed(PlayerPrefs.GetInt("userId"));
                    NetworkingManager.Singleton.NetworkConfig.ConnectionData = writeStream.ToArray();
                    ((RufflesTransport.RufflesTransport)NetworkingManager.Singleton.NetworkConfig.NetworkTransport).ConnectAddress = ip;
                    ((RufflesTransport.RufflesTransport)NetworkingManager.Singleton.NetworkConfig.NetworkTransport).Port = port;
                    NetworkingManager.Singleton.OnClientConnectedCallback += PlayerConnected_Player;
                    NetworkingManager.Singleton.OnClientDisconnectCallback += PlayerDisconnected_Player;
                    NetworkingManager.Singleton.StartClient();
                }
            }
        }
    }
    
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

#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)

    //Server: Start Server
    public void StartServer()
    {
        Application.targetFrameRate = 20;
        gameServer = GameServer.singleton;
        storedProperties = new ServerProperties();
        if (GetServerSettings()) 
        {
            GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
            int spawnLength = availableSpawns.Length;
            spawnpointPositions = new Vector3[spawnLength];
            for (int i = 0; i < spawnLength; i++)
            {
                spawnpointPositions[i] = availableSpawns[i].transform.position;
            }
            playerPrefabHash = SpawnManager.GetPrefabHashFromGenerator("Alien");
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


    
    //Callback: Player Conencted
    private void PlayerConnected_Server(ulong clientId)
    {
        gameServer.PlayerConnected(clientId);
        UpdatePlayerCount();
    }
    
    //Callback: Player Disconnected
    private void PlayerDisconnected_Server(ulong clientId)
    {
        WebStatsData stats = gameServer.Server_MovePlayerToInactive(clientId);
        if (stats.id != 0)
        {
            SavePlayerStats(stats);
        }
        UpdatePlayerCount();
        gameServer.PlayerDisconnected(clientId);
    }
    
    //Approval Check
    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkingManager.ConnectionApprovedDelegate callback)
    {
        using (PooledBitReader reader = PooledBitReader.Get(new MemoryStream(connectionData)))
        {
            bool approveConnection = false;
            Vector3 spawnPoint = GetRandomSpawnpoint();
            string username = reader.ReadStringPacked().ToString();
            string authKey = reader.ReadStringPacked().ToString();
            int userId = reader.ReadInt32Packed();
            if (username.Length > 0 && authKey.Length > 0 && userId != 0)
            {
                if (gameServer == null) gameServer = GameServer.singleton;
                if (gameServer.MovePlayerToActive(clientId, userId, authKey))
                {
                    spawnPoint = gameServer.GetPlayerLocation(clientId);
                    approveConnection = true;
                }
                else
                {
                    PlayerInfo player = GeneratePlayerInfo();
                    player.username = username;
                    player.authKey = authKey;
                    player.id = userId;
                    player.location = spawnPoint;
                    player.clientId = clientId;
                    approveConnection = gameServer.CreatePlayer(player);
                }
            }
            callback(true, playerPrefabHash, approveConnection, spawnPoint, Quaternion.identity);
        }
    }

    //Get Random Spawnpoint
    private Vector3 GetRandomSpawnpoint() 
    {
        if(spawnpointPositions != null && spawnpointPositions.Length > 0) 
        {
            return spawnpointPositions[UnityEngine.Random.Range(0, spawnpointPositions.Length - 1)];
        }
        else 
        {
            return new Vector3(0,5,0);
        }
    }

    //Generate Fresh PlayerInfo
    private PlayerInfo GeneratePlayerInfo() 
    {
        return new PlayerInfo
        {
            health = 100,
            food = 100,
            water = 100,
            coinsAdd = 0,
            expAdd = 0,
            hoursAdd = 0,
            time = DateTime.Now,
            isNew = true,
            inventory = new Inventory()
            {
                blueprints = new int[5] { 1, 2, 3, 4, 5 }
            }
        };
    }

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



    //Save Player Statistics
    private void SavePlayerStats(WebStatsData stats) 
    {
        if(webServer != null) 
        {
            string notifyData = storedProperties.serverName + "," + stats.hours + "," + stats.exp + "," + stats.coins;
            stats.notify = notifyData;
            webServer.SetClientStats(stats, returnValue => 
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
