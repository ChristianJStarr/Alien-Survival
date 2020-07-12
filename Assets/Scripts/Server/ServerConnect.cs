using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;
using System;
using System.Collections;
using System.IO;
using TMPro;
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
    public MainMenuScript mainMenu; //Main Menu
    public TMP_InputField serverConsole;
    public TMP_InputField serverCommand;
    public GameObject serverUI;
    public GameObject serverCamera;


    public bool devServer = false; //If development Server
    private ServerProperties storedProperties; //Stored Server Properties
    [SerializeField]
    private int logLevel = 3; //LogLevel
    private string storedIp = "";

    private string command = "";
    int am = 1;

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




    //Server Commands
    public void CommandUpdated()
    {
        command = serverCommand.text;
    }
    public void SendCommand()
    {
        if (command.StartsWith("/"))
        {
            serverCommand.text = "";
            string[] coms = command.Split(null);
            if (coms.Length > 1)
            {
                string newCommand = coms[0].Trim('/');
                string var1 = coms[1];
                string var2 = coms[2];
                if (newCommand == "teleport")
                {
                    if (var1.Length > 0 && var2.Length > 0)
                    {
                        gameServer.ServerTeleport(var1, var2);
                    }
                    else
                    {
                        DebugMessage("Please Specify, '/teleport <player> <target>", 1);
                    }
                }
                else { DebugMessage("Command Not Recognized. Try /Help for a list of commands.", 1); }
            }
            else
            {
                string newCommand = coms[0].Trim('/');

                if (newCommand == "Stop") { StopServer(); }
                else { DebugMessage("Command Not Recognized. Try /Help for a list of commands.", 1); }
            }
        }
        else
        {
            DebugMessage("Command Not Recognized. Try /Help for a list of commands.", 1);
        }
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
        if (pingTime != 0)
        {
            pingTime = ping.time;
        }
        callback(pingTime);
    }

    //Client: Connect to Server
    public void ConnectToServer(string ip, ushort port)
    {
        storedIp = ip;
        if (mainMenu == null)
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
        yield return new WaitForSeconds(2f);
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
        DebugMessage("Connected to Server.", 1);
        if (gameServer == null)
        {
            gameServer = GameServer.singleton;
        }
        gameServer.PlayerConnected_Player(id);
    }

    //Callback: Disconnected
    private void PlayerDisconnected_Player(ulong id)
    {
        DebugMessage("Disconnected from Server.", 1);

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            StartCoroutine(LoadMainMenu());
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

    private IEnumerator LoadMainMenu()
    {
        yield return null;
        bool singleCall = true;
        DebugMessage("Starting to Load Main Menu.", 4);
        AsyncOperation op = SceneManager.LoadSceneAsync(1);
        op.allowSceneActivation = false;
        while (!op.isDone)
        {
            if (op.progress >= 0.9f && singleCall)
            {
                singleCall = false;
                DebugMessage("Requesting Disconnect Stats.", 3);
                LoadMainMenuStage(op);
            }
            yield return null;
        }
    }

    private void LoadMainMenuStage(AsyncOperation op) 
    {
        webServer.StatRequest(PlayerPrefs.GetInt("userId"), PlayerPrefs.GetString("authKey"), onRequestFinished =>
        {
            if (onRequestFinished)
            {
                DebugMessage("Successfully Requested Stats.", 2);
                op.allowSceneActivation = true;
            }
            else
            {
                DebugMessage("Failed Requesting Stats.", 1);
                op.allowSceneActivation = true;
            }
        });
    }
   

    //-----------------------------------------------------------------//
    //                       Server Side Connect                       //
    //-----------------------------------------------------------------//

    private void ConsoleLogMessage(string message, string stack, LogType type) 
    {
        serverConsole.text += message + "\n";
    }

    //Server: Start Server
    public void StartServer()
    {
        //serverCamera.SetActive(true);
        //serverUI.SetActive(true);
        //Application.logMessageReceived += ConsoleLogMessage;

        gameServer = GameServer.singleton;
        storedProperties = new ServerProperties();
        if (GetServerSettings()) 
        {
            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += ServerStarted;
            networkManager.OnClientConnectedCallback += PlayerConnected_Server;
            networkManager.OnClientDisconnectCallback += PlayerDisconnected_Server;
            networkManager.GetComponent<UnetTransport>().MaxConnections = storedProperties.serverMaxPlayer;
            networkManager.GetComponent<UnetTransport>().ConnectAddress = storedProperties.publicIP;
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
        DebugMessage("Stopping the Server...", 1);
        UpdateServerList(false);
        GameServer.singleton.StopGameServer();
    }
    
    //Approval Check
    private void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkingManager.ConnectionApprovedDelegate callback)
    {
        if(gameServer == null) { gameServer = GameServer.singleton; }
        bool approve = true;
        bool noData = true;
        string connectData = System.Text.Encoding.ASCII.GetString(connectionData);
        string[] connectDataSplit = connectData.Split(',');
        int id = Convert.ToInt32(connectDataSplit[0]);
        string authKey = connectDataSplit[1].ToString();
        string username = connectDataSplit[2].ToString();
        GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
        Vector3 spawnPoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length)].transform.position;

        //Check if player has active PlayerInfo
        if (gameServer.activePlayers != null)
        {
            for (int i = 0; i < gameServer.activePlayers.Count; i++)
            {
                if (gameServer.activePlayers[i].id == id)
                {
                    Debug.Log(id);
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
                            player.time = DateTime.Now;
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
                    clientId = clientId,
                    coinsAdd = 0,
                    expAdd = 0,
                    hoursAdd = 0,
                    time = DateTime.Now
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
            serverIP = storedProperties.publicIP,
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
                    DebugMessage("Updated Server List.", 2);
                }
            });
        }
        StartCoroutine(ServerListLoop());
    }

    private IEnumerator ServerListLoop() 
    {
        yield return new WaitForSeconds(480);
        if(webServer != null) 
        {
            webServer.ServerListUpdateRecent(storedProperties.serverName, storedProperties.publicIP, onRequestFinished => 
            {
                if (onRequestFinished) 
                {
                    DebugMessage("Updated Server List.", 2);
                }
            });
        }
        StartCoroutine(ServerListLoop());
    }


    //Callback: Player Conencted
    private void PlayerConnected_Server(ulong id)
    {
        UpdatePlayerCount();
    }

    //Callback: Player Disconnected
    private void PlayerDisconnected_Server(ulong id)
    {
        PlayerInfo savedInfo = gameServer.MovePlayerToInactive(id);
        if (savedInfo != null)
        {
            SavePlayerStats(savedInfo);
        }
        UpdatePlayerCount();
    }

    //Save Player Statistics
    private void SavePlayerStats(PlayerInfo savedInfo) 
    {
        string serverAuthToken = "2alien2survival2";
        string notifyData = storedProperties.serverName + "," + savedInfo.hoursAdd + "," + savedInfo.expAdd + "," + savedInfo.coinsAdd;
        if(webServer != null) 
        {
            webServer.StatSend(savedInfo.id, savedInfo.authKey, serverAuthToken, savedInfo.expAdd, savedInfo.coinsAdd, savedInfo.hoursAdd, notifyData, "", returnValue => 
            {
                if (returnValue) 
                {
                    DebugMessage("Saving Player Statistics Successful.", 2);
                }
                else 
                {
                    DebugMessage("Failed Saving Player Statistics.", 1);
                }
            });
        }
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
                else 
                {
                    DebugMessage("Updating Server List Failed", 1);
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
                DebugMessage("Please Fix 'server-properties.txt'.", 1);
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
    public string serverName = "Default Server Name";
    public string serverDescription = "Default Description";
    public string serverMode = "Default Gamemode";
    public string serverMap = "Default Map";
    public string publicIP = "0.0.0.0";
    public ushort serverPort = 7708;
    public int serverMaxPlayer = 30;
    public int maxEnemies = 0;
    public int maxFriendly = 0;
    public int autoSaveInterval = 5;
}
