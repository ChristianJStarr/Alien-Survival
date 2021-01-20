using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class GameServer : NetworkedBehaviour
{
    #region Singleton

    public static GameServer singleton;
    void Awake()
    {
        //Singleton Init
        singleton = this;
    }

    #endregion


#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
    private ServerProperties storedProperties;//Properties
    [Header("Systems")]
    public PlayerInfoSystem playerInfoSystem;
    public WorldAISystem worldAISystem;
    public WorldObjectSystem worldObjectSystem;
    public ChatSystem chatSystem;
    public PlayerCommandSystem playerCommandSystem;
    public PlayerObjectSystem playerObjectSystem;
    public WorldSnapshotSystem worldSnapshotSystem;
    public EscapePodSystem escapePodSystem;
#endif
    [Header("Managers")]
    public PlayerInfoManager playerInfoManager;
    public PlayerUIManager playerUIManager;
    public ChatManager chatManager;
    public WorldSnapshotManager snapshotManager;
    public PlayerConnectManager playerConnectManager;
    public WorldParticleManager worldParticleManager;
    public LocalSoundManager localSoundManager;

    private NetworkingManager networkingManager;
    private int networkPing; //Client Side Ping

    #region Debug Statistics
    //----ServerDebug
    public int DebugSnapshotId = 0;
    public float DebugSnapshotSize = 0;
    public int DebugCommandPerSecond = 0;
    public int DebugCommandSize = 0;
    private int DebugCommandSize_AvgCount = 0;
    private int DebugCommandSize_AvgTotal = 0;
    private bool debugClientId = true;
    #endregion

    private void Start()
    {
        networkingManager = NetworkingManager.Singleton;
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
        if (IsServer && networkingManager != null)
        {
            StartGameServer();
        }
#endif
    }

    #region Start Functions
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)

    //Start the Game Server
    private void StartGameServer()
    {
        DebugMsg.Begin(1, "Starting Game Server.", 1);

        //Store properties
        storedProperties = ServerConnect.singleton.GetServerProperties();
        if (storedProperties == null)
        {
            DebugMsg.End(1, "Failed to Retrieve Server Properties", 1);
            return;
        }
        else
        {
            DebugMsg.Notify("Loaded Server Properties.", 1);
        }

        //Start Player Info System
        if (playerInfoSystem == null || !playerInfoSystem.StartSystem())
        {
            DebugMsg.End(1, "Failed to Start Player Info System", 1);
            return;
        }
        else
        {
            DebugMsg.Notify("Started Player Info System.", 1);
        }

        //Start World AI System
        if (worldAISystem == null || !worldAISystem.StartSystem())
        {
            DebugMsg.End(1, "Failed to Start World AI System", 1);
            return;
        }
        else
        {
            DebugMsg.Notify("Started World AI System.", 1);
        }

        //Start World Object System
        if (worldObjectSystem == null || !worldObjectSystem.StartSystem())
        {
            DebugMsg.End(1, "Failed to Start World Object System", 1);
            return;
        }
        else
        {
            DebugMsg.Notify("Started World Object System.", 1);
        }

        //Start Player Command System
        if (playerCommandSystem == null || !playerCommandSystem.StartSystem())
        {
            DebugMsg.End(1, "Failed to Start Player Command System", 1);
            return;
        }
        else
        {
            DebugMsg.Notify("Started Player Command System.", 1);
        }

        //Start World Snapshot System
        if (worldSnapshotSystem == null || !worldSnapshotSystem.StartSystem())
        {
            DebugMsg.End(1, "Failed to Start World Snapshot System", 1);
            return;
        }
        else
        {
            DebugMsg.Notify("Started World Snapshot System.", 1);
        }

        //Start Escape Pod System
        if (escapePodSystem == null || !escapePodSystem.StartSystem())
        {
            DebugMsg.End(1, "Failed to Start World Snapshot System", 1);
            return;
        }
        else
        {
            DebugMsg.Notify("Started World Snapshot System.", 1);
        }

        //Player Object System
        if (playerObjectSystem == null || !playerObjectSystem.StartSystem())
        {
            DebugMsg.End(1, "Failed to Player Object System", 1);
            return;
        }
        else
        {
            DebugMsg.Notify("Started Player Object System.", 1);
        }

        //Start Loops
        StartCoroutine(AutoSaveLoop());
        DebugMsg.End(1, "Game Server has Started.", 1);
    }

    //Stop the Game Server
    public void StopGameServer()
    {
        DebugMsg.Begin(2, "Stopping Game Server.", 1);

        NetworkingManager.Singleton.StopServer();

        playerInfoSystem.StopSystem();
        worldAISystem.StopSystem();
        worldObjectSystem.StopSystem();
        playerCommandSystem.StopSystem();
        worldSnapshotSystem.StopSystem();
        escapePodSystem.StopSystem();
        playerObjectSystem.StopSystem();

        DebugMsg.End(2, "Finsihed Stopping Game Server.", 1);
        Application.Quit();
    }

    //Create New Player
    public bool CreatePlayer(PlayerInfo playerInfo)
    {
        return playerInfoSystem.CreatePlayer(playerInfo);
    }

    //Get Player Location
    public Vector3 GetPlayerLocation(ulong clientId)
    {
        return playerInfoSystem.GetPlayerLocation(clientId);
    }
#endif
    #endregion

#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)

    #region ServerSide Callbacks

    //CALLBACK: Player Connected
    public void PlayerConnected(ulong clientId)
    {
        string playerName = playerInfoSystem.GetPlayerName(clientId);
        NetworkedObject playerObject = networkingManager.ConnectedClients[clientId].PlayerObject;
        ulong networkId = playerObject.NetworkId;

        Debug.Log("ControlObject Registered: " + clientId);
        playerObjectSystem.RegisterControlObject(clientId, playerObject);
        playerInfoSystem.SetPlayerNetworkId(clientId, networkId);
        chatSystem.PlayerConnected_AllMessage(playerName);
        chatSystem.PlayerWelcome_Specific(playerName, storedProperties.serverName, clientId);

        if (playerInfoSystem.GetPlayerDead(clientId))
        {
            Server_RespawnPlayer(clientId);
        }
        if (playerInfoSystem.GetPlayerNew(clientId))
        {
            Server_PlayerConnectSet(clientId, true);
            escapePodSystem.SpawnPlayerInsideEscapePod(clientId);
        }
        else
        {
            Server_PlayerConnectSet(clientId);
        }

        playerInfoSystem.Inventory_AddNew(clientId, 1, 1);

        playerInfoSystem.Inventory_AddNew(clientId, 50, 200);
        playerInfoSystem.ForceRequestInfoById(clientId);
    }

    //CALLBACK: Player Disconnected
    public void PlayerDisconnected(ulong clientId)
    {
        string playerName = playerInfoSystem.GetPlayerName(clientId);
        chatSystem.PlayerDisconnected_AllMessage(name);
        playerObjectSystem.UnRegisterControlObject(clientId);
    }
    #endregion

    #region ServerSide Tasks

    //Move Player To Active List
    public bool MovePlayerToActive(ulong clientId, int userId, string authKey) => playerInfoSystem.MovePlayerToActive(clientId, userId, authKey);

    //Move Player To Inactive List
    public WebStatsData Server_MovePlayerToInactive(ulong clientId) => playerInfoSystem.MovePlayerToInactive(clientId);

    //Handle Player Death (called from player info system when health reaches 0)
    public void Server_PlayerDeath(ulong clientId, Inventory inventory, string username)
    {
        Server_UIClosePlayerInventory(clientId);
        Server_UIShowDeathScreen(clientId, playerInfoSystem.ResetTimeSurvived(clientId)); //Show Death Screen
        playerObjectSystem.ToggleRagdoll(clientId, true);
        Server_PlayerDeathDrop(inventory, playerObjectSystem.GetCurrentPosition(clientId), username);
        //Wait for Respawn Response
    }

    //Spawn DeathDrop
    private void Server_PlayerDeathDrop(Inventory inventory, Vector3 location, string username)
    {
        //Spawn Loot
        inventory.Clear();
    }

    //Respawn Player
    public void Server_RespawnPlayer(ulong clientId)
    {
        GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
        Vector3 spawnpoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length - 1)].transform.position;
        playerObjectSystem.ToggleRagdoll(clientId, false);
        playerObjectSystem.Teleport_ToVector(clientId, spawnpoint);
        playerInfoSystem.ResetPlayerInfo(clientId, spawnpoint);
        Server_UIHideDeathScreen(clientId);
    }

    //Get ClientId from NetworkId
    public ulong Server_GetClientIdFromNetworkId(ulong networkId)
    {
        return GetNetworkedObject(networkId).OwnerClientId;
    }

    //Connect Callback for Client
    private void Server_PlayerConnectSet(ulong clientId, bool playCutscene = false)
    {
        InvokeClientRpcOnClient(Server_PlayerConnectSetRpc, clientId, playCutscene);
    }
    [ClientRPC]
    private void Server_PlayerConnectSetRpc(bool playCutscene)
    {
        playerConnectManager.ConnectCallback(playCutscene);
    }

    //Spawn Particle for Player
    public void Server_SpawnParticle(int particleId, Vector3 position, int visibility)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(particleId);
                writer.WriteVector3Packed(position);
                InvokeClientRpcPerformance(Server_SpawnParticleRpc, playerObjectSystem.GetNearbyClients(position, visibility), writeStream);
            }
        }
    }

    [ClientRPC]
    private void Server_SpawnParticleRpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            worldParticleManager.SpawnParticle(reader.ReadInt32Packed(), reader.ReadVector3Packed());
        }
    }


    //Play Sound Effect for Player
    public void Server_PlaySoundEffect(int soundEffectId, Vector3 position, int visibility)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(soundEffectId);
                writer.WriteVector3Packed(position);
                InvokeClientRpcPerformance(Server_PlaySoundEffectRpc, playerObjectSystem.GetNearbyClients(position, visibility), writeStream);
            }
        }
    }
    public void Server_PlaySoundEffect(int soundEffectId, Vector3 position, int visibility, ulong clientToIgnore)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(soundEffectId);
                writer.WriteVector3Packed(position);
                InvokeClientRpcPerformance(Server_PlaySoundEffectRpc, playerObjectSystem.GetNearbyClients(position, visibility, clientToIgnore), writeStream);
            }
        }
    }

    [ClientRPC]
    private void Server_PlaySoundEffectRpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            localSoundManager.PlaySoundEffect(reader.ReadInt32Packed(), reader.ReadVector3Packed());
        }
    }


    public void PlayerKilledPlayer(ulong killer, ulong killed)
    {
        playerInfoSystem.IncPlayerKills(killer);
        playerInfoSystem.AddPlayerExp(killer, 200);
        playerInfoSystem.AddPlayerCoins(killer, 50);
        playerInfoSystem.AddPlayerExp(killed, 20);
        string killer_name = playerInfoSystem.GetPlayerName(killer);
        string killed_name = playerInfoSystem.GetPlayerName(killed);
        chatSystem.PlayerKilled_AllMessage(killed_name, killer_name);
    }


    #endregion

    #region ServerSide Tasks: UI
    //-----------------------------------------------------------------//
    // (S)  SERVER         Tasks : User Interface                      //
    //-----------------------------------------------------------------//


    //-----Player Inventory
    //Show
    private void Server_UIShowPlayerInventory(ulong clientId, int uiType, UIData uiData)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(uiType);
                //Write UI Data


                InvokeClientRpcOnClientPerformance(Server_UIShowPlayerInventoryRpc, clientId, writeStream);
            }
        }
    }
    [ClientRPC]
    private void Server_UIShowPlayerInventoryRpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            int uiType = reader.ReadInt32Packed();
            UIData data = null;
            playerInfoManager.ShowInventoryScreen(uiType, data);
        }
    }
    //Hide
    public void Server_UIClosePlayerInventory(ulong clientId)
    {
        InvokeClientRpcOnClient(Server_UIClosePlayerInventoryRpc, clientId);
    }
    [ClientRPC]
    private void Server_UIClosePlayerInventoryRpc()
    {
        playerInfoManager.HideInventoryScreen();
    }


    //-----Player Death Screen
    //Show
    public void Server_UIShowDeathScreen(ulong clientId, TimeSpan span)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {

                writer.WriteDoublePacked(span.TotalHours);
                InvokeClientRpcOnClientPerformance(Server_UIShowDeathScreenRpc, clientId, writeStream);
            }
        }
    }
    [ClientRPC]
    private void Server_UIShowDeathScreenRpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerUIManager.ShowDeathScreen(reader.ReadDoublePacked());
        }
    }
    //Hide
    public void Server_UIHideDeathScreen(ulong clientId)
    {
        InvokeClientRpcOnClient(Server_UIHideDeathScreenRpc, clientId);
    }
    [ClientRPC]
    private void Server_UIHideDeathScreenRpc()
    {
        playerUIManager.HideDeathScreen();
    }
    #endregion

#endif

    #region Snapshotting Lockstepping

    //Send Player Command to Server
    public void ClientSendPlayerCommand(Stream stream)
    {
        InvokeServerRpcPerformance(Server_AuthenticatePlayerCommand, stream, "PlayerCommand");
    }

    //Authenticate Player Command
    [ServerRPC(RequireOwnership = false)]
    private void Server_AuthenticatePlayerCommand(ulong _clientId, Stream stream)
    {
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
        Debug.Log(string.Format("Command Incoming from Client: ", _clientId));
        playerCommandSystem.StoreClientCommand(BitHelper.ConvertClientCommand(_clientId, stream));
#endif
    }

    //Send World Snapshot to Clients
    public void ServerSendSnapshot(PackedSnapshot snapshot)
    {
        InvokeClientRpcPerformance(Client_RecieveWorldSnapshot, snapshot.clients, BitHelper.ConvertPackedSnapshot(snapshot));
    }

    //Handle Incoming World Snapshot
    [ClientRPC]
    private void Client_RecieveWorldSnapshot(ulong clientId, Stream stream)
    {
        PackedSnapshot snapshot = BitHelper.ConvertPackedSnapshot(stream);
        if (debugClientId)
        {
            debugClientId = false;
            DebugMenu.UpdateConnect(clientId, PlayerPrefs.GetString("username"));
        }
        if (networkingManager != null)
        {
            networkPing = Mathf.Clamp((int)(networkingManager.NetworkTime - snapshot.networkTime) * 1000, 1, 999);
        }
        snapshotManager.ProcessSnapshot(snapshot);
    }
    #endregion

    #region ClientSide Requests
    //--Request Player Name 
    public void GetNameByClientId(ulong clientId, Action<string> callback)
    {
        DebugMsg.Notify("Requesting Name of Client Id", 2);
        StartCoroutine(GetNameByClientId_Wait(clientId, returnValue => { callback(returnValue); }));
    }
    private IEnumerator GetNameByClientId_Wait(ulong clientId, Action<string> callback)
    {
        RpcResponse<string> response = InvokeServerRpc(GetNameByClientId_Rpc, clientId);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private string GetNameByClientId_Rpc(ulong clientId)
    {
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
        return playerInfoSystem.GetPlayerName(clientId);
#else
        return null;
#endif
    }


    //--Request to Move Item
    public void MovePlayerItemBySlot(string authKey, int oldSlot, int newSlot)
    {
        DebugMsg.Notify("Requesting to Modify Inventory.", 2);
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                writer.WriteInt32Packed(oldSlot);
                writer.WriteInt32Packed(newSlot);
                InvokeServerRpcPerformance(MovePlayerItemBySlot_Rpc, writeStream);
            }
        }

    }
    [ServerRPC(RequireOwnership = false)]
    private void MovePlayerItemBySlot_Rpc(ulong clientId, Stream stream)
    {
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerInfoSystem.Inventory_MoveItem(clientId, reader.ReadStringPacked().ToString(), reader.ReadInt32Packed(), reader.ReadInt32Packed());
        }
#endif
    }


    //--Request to Remove Item
    public void ClientRemoveItemBySlot(string authKey, int slot)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                writer.WriteInt32Packed(slot);
                InvokeServerRpcPerformance(RemovePlayerItemBySlot_Rpc, writeStream);
            }
        }
    }
    [ServerRPC(RequireOwnership = false)]
    private void RemovePlayerItemBySlot_Rpc(ulong clientId, Stream stream)
    {
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            string authKey = reader.ReadStringPacked().ToString();
            int slot = reader.ReadInt32Packed();
            Item item = playerInfoSystem.Inventory_GetDropItem(clientId, slot);
            if (item != null && playerInfoSystem.Inventory_RemoveItem(clientId, authKey, slot))
            {
                Debug.Log("Spawning Dropped Item: " + item.itemId);
                //Spawn (item)
            }
        }
#endif
    }


    //--Request to Split Item
    public void ClientSplitItemBySlot(string authKey, int slot, int amount)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                writer.WriteInt32Packed(slot);
                writer.WriteInt32Packed(amount);
                InvokeServerRpcPerformance(SplitItemBySlot_Rpc, writeStream);
            }
        }
    }
    [ServerRPC(RequireOwnership = false)]
    private void SplitItemBySlot_Rpc(ulong clientId, Stream stream)
    {
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerInfoSystem.Inventory_SplitItem(clientId, reader.ReadStringPacked().ToString(), reader.ReadInt32Packed(), reader.ReadInt32Packed());
        }
#endif
    }


    //--Request to Craft Item
    public void CraftItemById(string authKey, int itemId, int amount)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                writer.WriteInt32Packed(itemId);
                writer.WriteInt32Packed(amount);
                InvokeServerRpcPerformance(CraftItemById_Rpc, writeStream);
            }
        }

    }
    [ServerRPC(RequireOwnership = false)]
    private void CraftItemById_Rpc(ulong clientId, Stream stream)
    {
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerInfoSystem.Inventory_CraftItem(clientId, reader.ReadStringPacked().ToString(), reader.ReadInt32Packed(), reader.ReadInt32Packed());
        }
#endif
    }


    //--Request to Disconnect
    public void RequestToDisconnect(string authKey)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                InvokeServerRpcPerformance(RequestToDisconnect_Rpc, writeStream);
            }
        }
    }
    [ServerRPC(RequireOwnership = false)]
    private void RequestToDisconnect_Rpc(ulong clientId, Stream stream)
    {
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            string authKey = reader.ReadStringPacked().ToString();
            if (playerInfoSystem.Confirm(clientId, authKey))
            {
                NetworkingManager.Singleton.DisconnectClient(clientId);
            }
        }
#endif
    }


    //--Request to Respawn
    public void RequestToRespawn(string authKey)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                InvokeServerRpcPerformance(RequestToRespawn_Rpc, writeStream);
            }
        }
    }
    [ServerRPC(RequireOwnership = false)]
    private void RequestToRespawn_Rpc(ulong clientId, Stream stream)
    {
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            string authKey = reader.ReadStringPacked().ToString();
            if (playerInfoSystem.Confirm(clientId, authKey))
            {
                Server_RespawnPlayer(clientId);
            }
        }
#endif
    }


    //--Request Ping
    public int GetPlayerPing()
    {
        return networkPing;
    }
    #endregion

    #region Game Chat
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
    //Chat - Send ALL
    public void Chat_SendToAll(string message)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(message);
                InvokeClientRpcOnEveryonePerformance(ChatSendTo_Rpc, writeStream);
            }
        }
    }

    //Chat - Send Specific
    public void Chat_SendToSpecific(string message, ulong clientId)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(message);
                InvokeClientRpcOnClientPerformance(ChatSendTo_Rpc, clientId, writeStream);
            }
        }
    }
#endif
    [ClientRPC]
    private void ChatSendTo_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            ChatManager.singleton.Incoming(reader.ReadStringPacked().ToString());
        }
    }
    #endregion

    #region PlayerInfo Pipeline
    public void ServerSendPlayerInfo(ulong clientId, Stream stream)
    {
        InvokeClientRpcOnClientPerformance(ServerSendPlayerInfo_Rpc, clientId, stream, "PlayerInfo");
    }
    [ClientRPC]
    private void ServerSendPlayerInfo_Rpc(ulong clientId, Stream stream)
    {
        playerInfoManager.IntakeStream(stream);
    }
    #endregion

#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)

    #region ServerLoops
    //-----------------------------------------------------------------//
    //                          LOOPS                                  //
    //-----------------------------------------------------------------//

    //Auto Save Loop
    private IEnumerator AutoSaveLoop()
    {
        int interval = storedProperties.autoSaveInterval;
        if (interval < 5)
        {
            interval = 5;
        }
        while (true)
        {
            yield return new WaitForSeconds(interval * 60f);
            playerInfoSystem.AutoSave();
        }
    }
    #endregion

#endif
}



//1156 06/20/20
//1692 06/25/20
//2106 07/11/20 
//2212 08/15/20 
//1451 08/25/20 
//1123 09/22/20 
//1374 11/23/20 
//1229 11/30/20
//893  12/12/20
//917  12/13/20
//927 12/30/20
//912 1/20/21