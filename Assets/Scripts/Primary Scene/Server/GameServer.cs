using MLAPI;
using MLAPI.LagCompensation;
using MLAPI.Messaging;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
    [Header("Prefabs")]
    public GameObject deathDropPrefab;
    public GameObject[] particlePrefabs;


    // -- SYSTEMS
    [Header("Systems")]
    public PlayerInfoSystem playerInfoSystem;
    public WorldAISystem worldAISystem;
    public WorldObjectSystem worldObjectSystem;
    public ChatSystem chatSystem;
    public PlayerCommandSystem playerCommandSystem;
    public WorldSnapshotSystem worldSnapshotSystem;

    // -- MANAGERS
    [Header("Managers")]
    public PlayerInfoManager playerInfoManager;
    public PlayerUIManager playerUIManager;
    public ChatManager chatManager;
    public WorldSnapshotManager snapshotManager;

    private NetworkingManager networkingManager;

    //Properties
    private ServerProperties storedProperties;

    private int networkPing;

    //Temp Location for Respawn
    public Vector3 tempPlayerPosition = new Vector3(0, -5000, 0);


    //----ServerDebug
    public int DebugSnapshotId = 0;
    public float DebugSnapshotSize = 0;
    public int DebugCommandPerSecond = 0;



    private void Start()
    {
        playerInfoManager = PlayerInfoManager.singleton;
        playerUIManager = PlayerUIManager.Singleton;
        networkingManager = NetworkingManager.Singleton;
        if (IsServer)
        {
            StartGameServer();
        }
    }


    //-----------------------------------------------------------------//
    // (S) SERVER           START FUNCTIONS                            //
    //-----------------------------------------------------------------//

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

        if(playerCommandSystem == null || !playerCommandSystem.StartSystem()) 
        {
            DebugMsg.End(1, "Failed to Start Player Command System", 1);
            return;
        }
        else 
        {
            DebugMsg.Notify("Started Player Command System.", 1);
        }

        if(worldSnapshotSystem == null || !worldSnapshotSystem.StartSystem()) 
        {
            DebugMsg.End(1, "Failed to Start World Snapshot System", 1);
            return;
        }
        else 
        {
            DebugMsg.Notify("Started World Snapshot System.", 1);
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

        DebugMsg.End(2, "Finsihed Stopping Game Server.", 1);

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

    

    //-----------------------------------------------------------------//
    // (S)  SERVER          SIDE CALLBACKS                             //
    //-----------------------------------------------------------------//
    
    //CALLBACK: Player Connected
    public void PlayerConnected(ulong clientId)
    {
        string playerName = playerInfoSystem.GetPlayerName(clientId);
        chatSystem.PlayerConnected_AllMessage(playerName);
        chatSystem.PlayerWelcome_Specific(playerName, storedProperties.serverName, clientId);
        playerCommandSystem.RegisterPlayer(clientId, NetworkingManager.Singleton.ConnectedClients[clientId].PlayerObject);
    }
    
    //CALLBACK: Player Disconnected
    public void PlayerDisconnected(ulong clientId) 
    {
        chatSystem.PlayerDisconnected_AllMessage(playerInfoSystem.GetPlayerName(clientId));
        playerCommandSystem.RemovePlayer(clientId);
    }


    //-----------------------------------------------------------------//
    // (C)  CLIENT          SIDE CALLBACKS                             //
    //-----------------------------------------------------------------//

    //Player has Connected callback.
    public void PlayerConnected_Player(ulong networkId)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(PlayerPrefs.GetInt("userId"));
                writer.WriteStringPacked(PlayerPrefs.GetString("authKey"));
                writer.WriteUInt64Packed(networkId);
                InvokeServerRpcPerformance(PlayerHasConnected_Rpc, writeStream);
            }
        }
    }

    //Player has Connectd handover.
    [ServerRPC(RequireOwnership = false)]
    private void PlayerHasConnected_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerInfoSystem.SetPlayerNetworkId(clientId, reader.ReadInt32Packed(), reader.ReadStringPacked().ToString(), reader.ReadUInt64Packed());
            if (playerInfoSystem.GetPlayerDead(clientId))
            {
                Server_RespawnPlayer(clientId);
            }
            ForceRequestInfoById(clientId);
        }
    }


    
    //-----------------------------------------------------------------//
    // (S) SERVER         Tasks                                        //
    //-----------------------------------------------------------------//

    //Move Player To Active List
    public bool MovePlayerToActive(ulong clientId, int userId, string authKey) => playerInfoSystem.MovePlayerToActive(clientId, userId, authKey);

    //Move Player To Inactive List
    public PlayerInfo Server_MovePlayerToInactive(ulong clientId) => playerInfoSystem.MovePlayerToInactive(clientId);

    //Handle Player Death (called from player info system when health reaches 0)
    public void Server_PlayerDeath(ulong clientId, Item[] items, Item[] armor, string username) 
    {
        Server_UIClosePlayerInventory(clientId);
        ServerUI_ShowDeathScreen(clientId, playerInfoSystem.ResetTimeSurvived(clientId)); //Show Death Screen
        Server_PlayerRagdoll(clientId); //Player Ragdoll Object
        Server_PlayerDeathDrop(items, armor, playerCommandSystem.players[clientId].transform.position, username);
        playerInfoSystem.Inventory_Clear(clientId);
        playerCommandSystem.Teleport_ToVector(clientId, tempPlayerPosition);
        //Wait for Respawn Response
    }

    //Handle Player Ragdoll
    public void Server_PlayerRagdoll(ulong clientId) 
    {
        //Spawn a ragdoll alien at players location
    }

    //Spawn DeathDrop
    private void Server_PlayerDeathDrop(Item[] items, Item[] armor, Vector3 location, string username)
    {
        //if (items != null)
        //{
        //    GameObject deathDropObj = Instantiate(deathDropPrefab, location, Quaternion.identity);
        //    deathDropObj.transform.position = location;
        //    DeathDrop deathDrop = deathDropObj.GetComponent<DeathDrop>();
        //    NetworkedObject networkedObject = deathDropObj.GetComponent<NetworkedObject>();
        //    List<Item> dropItemTemp = items.ToList();
        //    if (armor != null)
        //    {
        //        foreach (Item item in armor)
        //        {
        //            dropItemTemp.Add(item);
        //        }
        //    }
        //    deathDrop.UpdateDropItems(dropItemTemp);
        //    deathDrop.toolTip = "Death of " + username;
        //    deathDrop.unique = GenerateUnique();
        //    networkedObject.Spawn();
        //}
        //else if (armor != null)
        //{
        //    GameObject deathDropObj = Instantiate(deathDropPrefab, location, Quaternion.identity);
        //    deathDropObj.transform.position = location;
        //    DeathDrop deathDrop = deathDropObj.GetComponent<DeathDrop>();
        //    NetworkedObject networkedObject = deathDropObj.GetComponent<NetworkedObject>();
        //    deathDrop.UpdateDropItems(armor.ToList());
        //    deathDrop.unique = GenerateUnique();
        //    networkedObject.Spawn();
        //}
    }

    //Respawn Player
    public void Server_RespawnPlayer(ulong clientId)
    {
        GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
        Vector3 spawnpoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length)].transform.position;
        playerCommandSystem.Teleport_ToVector(clientId, spawnpoint);
        playerInfoSystem.ResetPlayerInfo(clientId, spawnpoint);
        Server_UIHideDeathScreen(clientId);
    }

    //Get ClientId from NetworkId
    public ulong Server_GetClientIdFromNetworkId(ulong networkId) 
    {
        return GetNetworkedObject(networkId).OwnerClientId;
    }

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
    public void ServerUI_ShowDeathScreen(ulong clientId, TimeSpan span)
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

    


    //-----------------------------------------------------------------//
    // (C)<->(S)         Snapshotting                                  //
    //-----------------------------------------------------------------//

    //Send Player Command to Server
    public void ClientSendPlayerCommand(PlayerCommand command)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                //CommandBreakdown
                writer.WriteSinglePacked(command.networkTime);
                writer.WriteVector2Packed(command.move);
                writer.WriteVector2Packed(command.look);
                writer.WriteBool(command.jump);
                writer.WriteBool(command.crouch);
                writer.WriteBool(command.use);
                writer.WriteInt16Packed((short)command.selectedSlot);
                InvokeServerRpcPerformance(Server_AuthenticatePlayerCommand, writeStream, "PlayerCommand");
            }
        }
    }
    
    //Authenticate Player Command
    [ServerRPC(RequireOwnership = false)]
    private void Server_AuthenticatePlayerCommand(ulong _clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            //PlayerCommand Breakdown
            playerCommandSystem.ExecuteCommand(new PlayerCommand()
            {
                clientId = _clientId,
                networkTime = reader.ReadSinglePacked(),
                move = reader.ReadVector2Packed(),
                look = reader.ReadVector2Packed(),
                jump = reader.ReadBool(),
                crouch = reader.ReadBool(),
                use = reader.ReadBool(),
                selectedSlot = reader.ReadInt16Packed()
            });
        }
    }

    //Send World Snapshot to Clients
    public void ServerSendSnapshot(ulong clientId, Snapshot snapshot, bool full = false)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(snapshot.snapshotId);
                writer.WriteSinglePacked(snapshot.networkTime);

                //Players
                if (snapshot.players != null)
                {
                    writer.WriteInt32Packed(snapshot.players.Length);
                    for (int e = 0; e < snapshot.players.Length; e++)
                    {
                        writer.WriteUInt64Packed(snapshot.players[e].networkId);
                        writer.WriteVector3Packed(snapshot.players[e].location);
                        writer.WriteVector2Packed(snapshot.players[e].rotation);
                        writer.WriteInt16Packed((short)snapshot.players[e].holdId);
                    }
                }
                else
                {
                    writer.WriteInt32Packed(0);
                }

                //AI
                if (snapshot.ai != null)
                {
                    writer.WriteInt32Packed(snapshot.ai.Length);
                    for (int e = 0; e < snapshot.ai.Length; e++)
                    {
                        writer.WriteUInt64Packed(snapshot.ai[e].networkId);
                        writer.WriteVector3Packed(snapshot.ai[e].location);
                        writer.WriteVector2Packed(snapshot.ai[e].rotation);
                        writer.WriteInt16Packed((short)snapshot.ai[e].holdId);
                    }
                }
                else
                {
                    writer.WriteInt32Packed(0);
                }

                //World Objects
                if (snapshot.worldObjects != null)
                {
                    writer.WriteInt32Packed(snapshot.worldObjects.Length);
                    for (int e = 0; e < snapshot.worldObjects.Length; e++)
                    {
                        writer.WriteInt32Packed(snapshot.worldObjects[e].spawnId);
                        writer.WriteInt32Packed(snapshot.worldObjects[e].objectId);
                    }
                }
                else
                {
                    writer.WriteInt32Packed(0);
                }



                DebugSnapshotSize = writeStream.Length;

                if (full)
                {
                    InvokeClientRpcOnClientPerformance(Client_RecieveWorldSnapshot, clientId, writeStream, "Snapshot_Full");
                }
                else
                {
                    InvokeClientRpcOnClientPerformance(Client_RecieveWorldSnapshot, clientId, writeStream, "Snapshot_Mini");
                }
            }
        }
    }
    
    //Handle Incoming World Snapshot
    [ClientRPC]
    private void Client_RecieveWorldSnapshot(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            Snapshot snapshot = new Snapshot()
            {
                snapshotId = reader.ReadInt32Packed(),
                networkTime = reader.ReadSinglePacked()
            };
            //Players
            int playerLength = reader.ReadInt32Packed();
            if (playerLength > 0)
            {
                Snapshot_Player[] s_players = new Snapshot_Player[playerLength];
                for (int i = 0; i < playerLength; i++)
                {
                    s_players[i] = new Snapshot_Player()
                    {
                        networkId = reader.ReadUInt64Packed(),
                        location = reader.ReadVector3Packed(),
                        rotation = reader.ReadVector2Packed(),
                        holdId = reader.ReadInt16Packed()
                    };
                }
                snapshot.players = s_players;
            }

            //AI
            int aiLength = reader.ReadInt32Packed();
            if (aiLength > 0)
            {
                Snapshot_AI[] s_ai = new Snapshot_AI[aiLength];
                for (int i = 0; i < aiLength; i++)
                {
                    s_ai[i] = new Snapshot_AI()
                    {
                        networkId = reader.ReadUInt64Packed(),
                        location = reader.ReadVector3Packed(),
                        rotation = reader.ReadVector2Packed(),
                        holdId = reader.ReadInt16Packed()
                    };
                }
                snapshot.ai = s_ai;
            }

            //World Object
            int worldObjectLength = reader.ReadInt32Packed();
            if(worldObjectLength > 0) 
            {
                Snapshot_WorldObject[] s_worldObjects = new Snapshot_WorldObject[worldObjectLength];
                for (int i = 0; i < worldObjectLength; i++)
                {
                    s_worldObjects[i] = new Snapshot_WorldObject()
                    {
                        spawnId = reader.ReadInt32Packed(),
                        objectId = reader.ReadInt32Packed()
                    };
                }
                snapshot.worldObjects = s_worldObjects;
            }

            //Process This Snapshot
            snapshotManager.ProcessSnapshot(snapshot);
            if (networkingManager != null)
            {
                networkPing = Mathf.Clamp((int)(networkingManager.NetworkTime - snapshot.networkTime) * 1000, 1, 999);
            }
        }
    }




    //-----------------------------------------------------------------//
    // (C)->(S)          Player Requests : Commands                    //
    //-----------------------------------------------------------------//

    //--Request Player Name 
    //CLIENT
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
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private string GetNameByClientId_Rpc(ulong clientId)
    {
        return playerInfoSystem.GetPlayerName(clientId);
    }

    //--Request to Move Item
    //CLIENT
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
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void MovePlayerItemBySlot_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerInfoSystem.Inventory_MoveItem(clientId, reader.ReadStringPacked().ToString(), reader.ReadInt32Packed(), reader.ReadInt32Packed());
        }
    }

    //--Request to Remove Item
    //CLIENT
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
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void RemovePlayerItemBySlot_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerInfoSystem.Inventory_RemoveItem(clientId, reader.ReadStringPacked().ToString(), reader.ReadInt32Packed(), returnedItem =>
            {
                if (returnedItem != null)
                {
                    //Implement Dropping of item, Currently we're deleting it.
                    //Drop returnedItem.itemID;
                }
            });
        }
    }

    //--Request to Split Item
    //CLIENT
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
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void SplitItemBySlot_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerInfoSystem.Inventory_SplitItem(clientId, reader.ReadStringPacked().ToString(), reader.ReadInt32Packed(), reader.ReadInt32Packed());
        }
    }

    //--Request to Craft Item
    //ClIENT
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
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void CraftItemById_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerInfoSystem.Inventory_CraftItem(clientId, reader.ReadStringPacked().ToString(), reader.ReadInt32Packed(), reader.ReadInt32Packed());
        }
    }

    //--Request to Disconnect
    //CLIENT
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
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void RequestToDisconnect_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            string authKey = reader.ReadStringPacked().ToString();
            if (playerInfoSystem.Confirm(clientId, authKey))
            {
                NetworkingManager.Singleton.DisconnectClient(clientId);
            }
        }
    }

    //--Request to Respawn
    //CLIENT
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
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void RequestToRespawn_Rpc(ulong clientId, Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            string authKey = reader.ReadStringPacked().ToString();
            if (playerInfoSystem.Confirm(clientId, authKey))
            {
                Server_RespawnPlayer(clientId);
            }
        }
    }
    
    //--Request Ping
    //CLIENT
    public int GetPlayerPing()
    {
        return networkPing;
    }

    //--Request to Cheat
    //CLIENT
    public void RequestToCheat_Item(int itemId, int amount) 
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(itemId);
                writer.WriteInt32Packed(amount);
                InvokeServerRpcPerformance(RequestToCheat_ItemRpc, writeStream);
            }
        }
    }
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void RequestToCheat_ItemRpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerInfoSystem.Inventory_AddNew(clientId, reader.ReadInt32Packed(), reader.ReadInt32Packed(), returnValue => { });
        }
    }

    

    //-----------------------------------------------------------------//
    //         Chat System                                             //
    //-----------------------------------------------------------------//

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
    [ClientRPC]
    private void ChatSendTo_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            ChatManager.singleton.Incoming(reader.ReadStringPacked().ToString());
        }
    }



    //-----------------------------------------------------------------//
    // (S)->(C)        PlayerInfo Pipeline                             //
    //-----------------------------------------------------------------//
 
    public void ForceRequestInfoById(ulong clientId, int depth = 1) 
    {
        //------DEPTH KEY------//
        //   1 - ALL           //
        //   2 - HEALTH        //
        //   3 - FOOD          //
        //   4 - WATER         //
        //   5 - ITEMS         //
        //   6 - ARMOR         //
        //   7 - BLUEPRINTS    //
        //   8 - HP/FOOD/WATER //
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(depth);
                if (depth == 1) //All
                {
                    PlayerInfo player = playerInfoSystem.GetPlayerInfo(clientId);
                    if (player != null)
                    {
                        writer.WriteInt32Packed(player.health);
                        writer.WriteInt32Packed(player.food);
                        writer.WriteInt32Packed(player.water);
                        writer.WriteVector3Packed(player.location);
                        
                        //INVENTORY ITEMS
                        if(player.items != null) 
                        {
                            int inventoryLength = player.items.Length;
                            if (inventoryLength > 0)
                            {
                                writer.WriteInt32Packed(inventoryLength);
                                for (int i = 0; i < inventoryLength; i++)
                                {
                                    if (player.items[i] != null)
                                    {
                                        Item instance = player.items[i];
                                        writer.WriteInt32Packed(instance.itemID);
                                        writer.WriteInt32Packed(instance.itemStack);
                                        writer.WriteInt32Packed(instance.currSlot);
                                        writer.WriteInt32Packed(instance.durability);
                                    }
                                }
                            }
                            else
                            {
                                writer.WriteInt32Packed(0);
                            }
                        }
                        else
                        {
                            writer.WriteInt32Packed(0);
                        }


                        //ARMOR ITEMS
                        if (player.armor != null)
                        {
                            int armorLength = player.armor.Length;
                            if (armorLength > 0)
                            {
                                writer.WriteInt32Packed(armorLength);
                                for (int i = 0; i < armorLength; i++)
                                {
                                    if (player.armor[i] != null)
                                    {
                                        Item instance = player.armor[i];
                                        writer.WriteInt32Packed(instance.itemID);
                                        writer.WriteInt32Packed(instance.itemStack);
                                        writer.WriteInt32Packed(instance.currSlot);
                                        writer.WriteInt32Packed(instance.durability);
                                    }
                                }
                            }
                            else
                            {
                                writer.WriteInt32Packed(0);
                            }
                        }
                        else
                        {
                            writer.WriteInt32Packed(0);
                        }

                        writer.WriteIntArrayPacked(player.blueprints);
                    }
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 2) //Health
                {
                    writer.WriteInt32Packed(playerInfoSystem.GetPlayerHealth(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 3) //Food
                {
                    writer.WriteInt32Packed(playerInfoSystem.GetPlayerFood(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 4) //Water
                {
                    writer.WriteInt32Packed(playerInfoSystem.GetPlayerWater(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 5) //Items
                {
                    //INVENTORY ITEMS
                    Item[] inventory = playerInfoSystem.GetPlayerItems(clientId);
                    if (inventory != null)
                    {
                        int inventoryLength = inventory.Length;
                        if (inventoryLength > 0)
                        {
                            writer.WriteInt32Packed(inventoryLength);
                            for (int i = 0; i < inventoryLength; i++)
                            {
                                if (inventory[i] != null)
                                {
                                    Item instance = inventory[i];
                                    writer.WriteInt32Packed(instance.itemID);
                                    writer.WriteInt32Packed(instance.itemStack);
                                    writer.WriteInt32Packed(instance.currSlot);
                                    writer.WriteInt32Packed(instance.durability);
                                }
                            }
                        }
                        else
                        {
                            writer.WriteInt32Packed(0);
                        }
                    }
                    else
                    {
                        writer.WriteInt32Packed(0);
                    }
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 6) //Armor
                {
                    //ARMOR ITEMS
                    Item[] armor = playerInfoSystem.GetPlayerArmor(clientId);
                    if (armor != null)
                    {
                        int armorLength = armor.Length;
                        if (armorLength > 0)
                        {
                            writer.WriteInt32Packed(armorLength);
                            for (int i = 0; i < armorLength; i++)
                            {
                                if (armor[i] != null)
                                {
                                    Item instance = armor[i];
                                    writer.WriteInt32Packed(instance.itemID);
                                    writer.WriteInt32Packed(instance.itemStack);
                                    writer.WriteInt32Packed(instance.currSlot);
                                    writer.WriteInt32Packed(instance.durability);
                                }
                            }
                        }
                        else
                        {
                            writer.WriteInt32Packed(0);
                        }
                    }
                    else
                    {
                        writer.WriteInt32Packed(0);
                    }
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");

                }
                else if (depth == 7) //Blueprints
                {
                    writer.WriteIntArrayPacked(playerInfoSystem.GetPlayerBlueprints(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 8) //Health / Food / Water
                {
                    PlayerInfo player = playerInfoSystem.GetPlayerInfo(clientId);
                    if (player != null)
                    {
                        writer.WriteInt32Packed(player.health);
                        writer.WriteInt32Packed(player.food);
                        writer.WriteInt32Packed(player.water);
                    }
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
            }
        }
    }
    [ClientRPC]
    private void SendInfoToPlayer_Rpc(ulong clientId, Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            int depth = reader.ReadInt32Packed();
            if(depth == 1)//All
            {
                PlayerInfo info = new PlayerInfo()
                {
                    health = reader.ReadInt32Packed(),
                    food = reader.ReadInt32Packed(),
                    water = reader.ReadInt32Packed(),
                    location = reader.ReadVector3Packed()
                };

                //READ INVENTORY ITEMS
                int inventoryLength = reader.ReadInt32Packed();
                if(inventoryLength > 0) 
                {
                    Item[] items = new Item[inventoryLength];
                    for (int i = 0; i < inventoryLength; i++)
                    {
                        items[i] = new Item()
                        {
                            itemID = reader.ReadInt32Packed(),
                            itemStack = reader.ReadInt32Packed(),
                            currSlot = reader.ReadInt32Packed(),
                            durability = reader.ReadInt32Packed()
                        };
                    }
                    info.items = items;
                }

                //READ INVENTORY ARMOR
                int armorLength = reader.ReadInt32Packed();
                if (armorLength > 0)
                {
                    Item[] items = new Item[armorLength];
                    for (int i = 0; i < armorLength; i++)
                    {
                        items[i] = new Item()
                        {
                            itemID = reader.ReadInt32Packed(),
                            itemStack = reader.ReadInt32Packed(),
                            currSlot = reader.ReadInt32Packed(),
                            durability = reader.ReadInt32Packed()
                        };
                    }
                    info.armor = items;
                }

                info.blueprints = reader.ReadIntArrayPacked();


                playerInfoManager.UpdateAll(info);
            }
            else if(depth == 2)//Health
            {
                int health = reader.ReadInt32Packed();
                playerInfoManager.UpdateHealth(health);
            }
            else if (depth == 3)//Food
            {
                int food = reader.ReadInt32Packed();
                playerInfoManager.UpdateFood(food);
            }
            else if (depth == 4)//Water
            {
                int water = reader.ReadInt32Packed();
                playerInfoManager.UpdateWater(water);
            }
            else if (depth == 5)//Items
            {
                int inventoryLength = reader.ReadInt32Packed();
                if (inventoryLength > 0)
                {
                    Item[] items = new Item[inventoryLength];
                    for (int i = 0; i < inventoryLength; i++)
                    {
                        items[i] = new Item()
                        {
                            itemID = reader.ReadInt32Packed(),
                            itemStack = reader.ReadInt32Packed(),
                            currSlot = reader.ReadInt32Packed(),
                            durability = reader.ReadInt32Packed()
                        };
                    }
                    playerInfoManager.UpdateItems(items);
                }
            }
            else if (depth == 6)//Armor
            {
                int armorLength = reader.ReadInt32Packed();
                if (armorLength > 0)
                {
                    Item[] items = new Item[armorLength];
                    for (int i = 0; i < armorLength; i++)
                    {
                        items[i] = new Item()
                        {
                            itemID = reader.ReadInt32Packed(),
                            itemStack = reader.ReadInt32Packed(),
                            currSlot = reader.ReadInt32Packed(),
                            durability = reader.ReadInt32Packed()
                        };
                    }
                    playerInfoManager.UpdateArmor(items);
                }
            }
            else if (depth == 7) //Blueprints
            {
                int[] blueprints = reader.ReadIntArrayPacked();
                playerInfoManager.UpdateBlueprints(blueprints);
            }
            else if (depth == 8) //Health / Food / Water
            {
                playerInfoManager.UpdateHealth(reader.ReadInt32Packed());
                playerInfoManager.UpdateFood(reader.ReadInt32Packed());
                playerInfoManager.UpdateWater(reader.ReadInt32Packed());
            }
        }
    }
    

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

}
//1156 06/20/20
//1692 06/25/20
//2106 07/11/20 
//2212 08/15/20 
//1451 08/25/20 
//1123 09/22/20 
//1374 11/23/20 
//1229 11/30/20