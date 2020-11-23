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
    public PlayerActionManager playerActionManager;
    public ChatManager chatManager;
    public WorldSnapshotManager snapshotManager;

    private NetworkingManager networkingManager;

    //Properties
    private ServerProperties storedProperties;

    private int networkPing;

    //Temp Location for Respawn
    public Vector3 tempPlayerPosition = new Vector3(0, -5000, 0);

    private void Start()
    {
        playerInfoManager = PlayerInfoManager.singleton;
        playerActionManager = PlayerActionManager.singleton;
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

    //Initialize Player Info
    public void InitializePlayerInfo(ulong clientId)
    {
        playerInfoSystem.SetPlayerTime(clientId, DateTime.Now);
        playerInfoSystem.Inventory_AddNew(clientId, 21, 1, returnValue => { });
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
            if (GetPlayerIsDead(clientId))
            {
                Server_RespawnPlayerTask(clientId);
            }
            ForceRequestInfoById(clientId);
        }
    }


    //-----------------------------------------------------------------//
    // (S)  SERVER            SIDE TOOL                                //
    //-----------------------------------------------------------------//

    //Generate UniqueID
    public string GenerateUnique()
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[20];
        var random = new System.Random();
        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(stringChars);
    }

    //Get Item from ItemData Damage
    public int ServerGetItemDamage(ItemData itemData)
    {
        int damage = 0;
        foreach (string data in itemData.itemUse)
        {
            string[] datas = data.Split('-');
            int type = Convert.ToInt32(datas[0]);
            int amount = Convert.ToInt32(datas[1]);
            if (type == 4)
            {
                damage += amount;
            }
        }
        return damage;
    }


    //-----------------------------------------------------------------//
    // (S) SERVER         Task : Change Player Info                    //
    //-----------------------------------------------------------------//

    //Move Player To Active List
    public bool MovePlayerToActive(ulong clientId, int userId, string authKey) => playerInfoSystem.MovePlayerToActive(clientId, userId, authKey);

    //Move Player To Inactive List
    public PlayerInfo Server_MovePlayerToInactive(ulong clientId) => playerInfoSystem.MovePlayerToInactive(clientId);

    //Respawn Player
    public void Server_RespawnPlayer(ulong clientId)
    {
        DebugMsg.Notify("Respawning Player '" + clientId + "'.", 2);

        //Show the Death Screen
        Server_UIShowDeathScreen(clientId);

        //Teleport Player to Temp Location
        Server_TeleportPlayerToLocation(clientId, tempPlayerPosition);
        playerInfoSystem.SetPlayerLocation(clientId, tempPlayerPosition);

        //Spawn the Death Drop
        Server_SpawnDeathDrop(playerInfoSystem.GetPlayerItems(clientId), playerInfoSystem.GetPlayerArmor(clientId), NetworkingManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.position, playerInfoSystem.GetPlayerName(clientId));
        playerInfoSystem.ClearPlayerInventory(clientId);
        playerInfoSystem.SetPlayerDead(clientId, true);
        //Force Request Info
        ForceRequestInfoById(clientId);
    }

    //Teleport Player to Vector3
    private void Server_TeleportPlayerToLocation(ulong clientId, Vector3 position)
    {
        if(playerCommandSystem != null && playerCommandSystem.players != null && playerCommandSystem.players.ContainsKey(clientId)) 
        {
            playerCommandSystem.players[clientId].transform.position = position;
        }
    }

    //Spawn DeathDrop
    private void Server_SpawnDeathDrop(Item[] items, Item[] armor, Vector3 location, string username)
    {
        if (items != null)
        {
            GameObject deathDropObj = Instantiate(deathDropPrefab, location, Quaternion.identity);
            deathDropObj.transform.position = location;
            DeathDrop deathDrop = deathDropObj.GetComponent<DeathDrop>();
            NetworkedObject networkedObject = deathDropObj.GetComponent<NetworkedObject>();
            List<Item> dropItemTemp = items.ToList();
            if (armor != null)
            {
                foreach (Item item in armor)
                {
                    dropItemTemp.Add(item);
                }
            }
            deathDrop.UpdateDropItems(dropItemTemp);
            deathDrop.toolTip = "Death of " + username;
            deathDrop.unique = GenerateUnique();
            networkedObject.Spawn();
        }
        else if (armor != null)
        {
            GameObject deathDropObj = Instantiate(deathDropPrefab, location, Quaternion.identity);
            deathDropObj.transform.position = location;
            DeathDrop deathDrop = deathDropObj.GetComponent<DeathDrop>();
            NetworkedObject networkedObject = deathDropObj.GetComponent<NetworkedObject>();
            deathDrop.UpdateDropItems(armor.ToList());
            deathDrop.unique = GenerateUnique();
            networkedObject.Spawn();
        }
    }

    //Server Raycast Request
    private NetworkedObject Server_RaycastRequest(Vector3 aimPos, Vector3 aimRot, bool spawnParticle, int range)
    {
        NetworkedObject netObject = null; ;
        RaycastHit hit;
        LagCompensationManager.Simulate(1, () =>
        {
            if (Physics.Raycast(aimPos, aimRot, out hit, range))
            {
                if (hit.collider != null)
                {
                    Vector3 hitPosition = hit.point;
                    netObject = hit.collider.GetComponent<NetworkedObject>();
                    if (spawnParticle)
                    {
                        Server_SpawnParticle(hitPosition, netObject.tag);
                    }
                }
            }
        });
        return netObject;
    }

    //Server Spawn Particle
    private void Server_SpawnParticle(Vector3 pos, string tag)
    {
        //GameObject newObject = Instantiate(particle, pos, Quaternion.identity);
        //newObject.GetComponent<NetworkedObject>().Spawn();
    }

    //Server Damage Networked Object
    private void Server_DamageNetworkedObject(NetworkedObject netObject, int amount)
    {
        //bool damaged = false;
        //for (int i = 0; i < activePlayers.Count; i++)
        //{
        //    if (activePlayers[i].networkId == netObject.NetworkId)
        //    {
        //        Server_SetHealth(activePlayers[i].clientId, -1 * amount);
        //        damaged = true;
        //        break;
        //    }
        //}
        //if (!damaged)
        //{
        //    //Damage AI through AI Controller by NetworkID
        //}
    }


    //-----------------------------------------------------------------//
    // (S)  SERVER         Tasks : User Interface                      //
    //-----------------------------------------------------------------//

    //Close Inventory on Client
    public void Server_UICloseInventory(ulong clientId)
    {
        InvokeClientRpcOnClient(Server_UICloseInventoryRpc, clientId);
    }
    [ClientRPC]
    private void Server_UICloseInventoryRpc()
    {
        playerInfoManager.CloseInventory();
    }

    //Force Death Screen on Client
    public void Server_UIShowDeathScreen(ulong clientId)
    {
        InvokeClientRpcOnClient(Server_UIShowDeathScreenRpc, clientId);
    }

    //Client RPC - Force Death Screen
    [ClientRPC]
    private void Server_UIShowDeathScreenRpc()
    {
        PlayerActionManager.singleton.ShowDeathScreen();
    }

    //Hide Death Screen on Client
    public void Server_UIHideDeathScreen(ulong clientId)
    {
        InvokeClientRpcOnClient(Server_UIHideDeathScreenRpc, clientId);
    }

    //Client RPC - Hide Death Screen
    [ClientRPC]
    private void Server_UIHideDeathScreenRpc()
    {
        PlayerActionManager.singleton.HideDeathScreen();
    }

    private void Server_UIShowStorage(ulong clientId, string data)
    {
        InvokeClientRpcOnClient(Server_UIShowStorageRpc, clientId, data);
    }
    [ClientRPC]
    private void Server_UIShowStorageRpc(string data)
    {
        playerInfoManager.ShowStorage(data);
    }

    private void Server_UIUpdateStorage(ulong clientId, string data)
    {
        InvokeClientRpcOnClient(Server_UIUpdateStorageRpc, clientId, data);
    }
    [ClientRPC]
    private void Server_UIUpdateStorageRpc(string data)
    {
        playerInfoManager.UpdateExtraUIData(data);
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


    //--Request Player Name
    //CLIENT
    public void GetAllConnectedClients(Action<ulong[]> callback)
    {
        DebugMsg.Notify("Requesting a List of All Clients.", 2);
        StartCoroutine(GetAllConnectedClients_Wait(returnValue => { callback(returnValue); }));
    }
    private IEnumerator GetAllConnectedClients_Wait(Action<ulong[]> callback)
    {
        RpcResponse<ulong[]> response = InvokeServerRpc(GetAllConnectedClients_Rpc);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private ulong[] GetAllConnectedClients_Rpc()
    {
        if (NetworkingManager.Singleton != null)
        {
            return NetworkingManager.Singleton.ConnectedClients.Keys.ToArray();
        }
        else
        {
            return null;
        }
    }


    //--Request to Set Location
    //CLIENT
    public void SetPlayerLocation(string authKey, Vector3 location)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                writer.WriteSinglePacked(location.x);
                writer.WriteSinglePacked(location.y);
                writer.WriteSinglePacked(location.z);
                InvokeServerRpcPerformance(SetPlayerLocation_Rpc, writeStream);
            }
        }
    }
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerLocation_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            if (playerInfoSystem.Confirm(clientId, reader.ReadStringPacked().ToString()))
            {
                playerInfoSystem.SetPlayerLocation(clientId, new Vector3(reader.ReadSinglePacked(), reader.ReadSinglePacked(), reader.ReadSinglePacked()));
            }
        }
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
            DebugMsg.Notify("Player Requesting to Modify Inventory.", 4);
            playerInfoSystem.Inventory_MoveItem(clientId, reader.ReadStringPacked().ToString(), reader.ReadInt32Packed(), reader.ReadInt32Packed());
        }
    }


    //--Request to Remove Item
    //CLIENT
    public void RemovePlayerItemBySlot(ulong clientId, string authKey, int slot)
    {
        DebugMsg.Notify("Requesting to Modify Inventory.", 2);
        InvokeServerRpc(RemovePlayerItemBySlot_Rpc, clientId, authKey, slot);
    }
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void RemovePlayerItemBySlot_Rpc(ulong clientId, string authKey, int slot)
    {
        playerInfoSystem.Inventory_RemoveItem(clientId, authKey, slot, returnedItem =>
        {
            if (returnedItem != null)
            {
                //Drop returnedItem.itemID;
            }
        });
    }


    //--Request to Craft Item
    //ClIENT
    public void CraftItemById(ulong clientId, string authKey, int itemId, int amount)
    {
        DebugMsg.Notify("Requesting to Modify Inventory.", 2);
        InvokeServerRpc(CraftItemById_Rpc, clientId, authKey, itemId, amount);
    }
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void CraftItemById_Rpc(ulong clientId, string authKey, int itemId, int amount)
    {
        playerInfoSystem.Inventory_CraftItem(clientId, authKey, itemId, amount);
    }


    //--Request to Interact
    //CLIENT
    public void PlayerInteract(string authKey, Ray ray, int selectedSlot) 
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                writer.WriteInt32Packed(selectedSlot);
                writer.WriteRayPacked(ray);
                InvokeServerRpcPerformance(PlayerInteractRpc, writeStream);
            }
        }
    }
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void PlayerInteractRpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            if (playerInfoSystem.Confirm(clientId, reader.ReadStringPacked().ToString()))
            {
                DebugMsg.Notify("Interact Confirmed", 1);
                int selectedSlot = reader.ReadInt32Packed();
                Ray ray = reader.ReadRayPacked();
                //LagCompensationManager.Simulate(clientId, () => {});
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 30))
                {
                    if (hit.collider != null)
                    {
                        DebugMsg.Notify("Ray has collided with " + hit.collider.tag, 1);
                        
                        NetworkedObject networkObject = hit.collider.GetComponentInParent<NetworkedObject>();
                        if (networkObject != null)
                        {
                            Server_Interact(clientId, selectedSlot, hit.distance, hit.collider.tag, networkObject.NetworkId);
                        }
                    }
                }
            }
        }
    }
    private void Server_Interact(ulong clientId, int selectedSlot, float distance, string tag, ulong networkId) 
    {

        DebugMsg.Notify("Server_Interact Started", 1);
        Item item = playerInfoSystem.Inventory_GetItemFromSlot(clientId, selectedSlot);
        if (item != null) //Has Item in Hand 
        {
            DebugMsg.Notify("Item: " + item.itemID + " detected.", 1);
            ItemData data = ItemDataManager.Singleton.GetItemData(item.itemID);
            if (distance < data.useRange)
            {
                DebugMsg.Notify("Object within useRange. Distance: " + distance, 1);
                if (data.useType == 1) //Shoot
                {

                }
                else if (data.useType == 2) //Melee
                {

                }
                else if (data.useType == 3) //Tool
                {
                    if (tag == "WorldObject") //World Object
                    {
                        DebugMsg.Notify("Object is WorldObject. Item is a TOOL", 1);
                        if (item.durability > 0)
                        {
                            DebugMsg.Notify("Interact Object has Durability", 1);
                            if (data.useSound.Length > 0)
                            {
                                LocalSoundManager.Singleton.PlaySound(data.useSound, NetworkingManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.position);
                            }
                            worldObjectSystem.DepleteWorldObject(networkId, data.toolId, data.toolGatherAmount, returnValue =>
                            {
                                if (data.hitSound.Length > 0)
                                {
                                    LocalSoundManager.Singleton.PlaySound(data.hitSound, GetNetworkedObject(networkId).transform.position);
                                }
                                playerInfoSystem.Inventory_AddNew(clientId, returnValue.itemId, returnValue.amount, itemPlaced =>
                                {
                                    if (itemPlaced)
                                    {

                                        DebugMsg.Notify("Interact Gather Added", 1);
                                        if (playerInfoSystem.Inventory_ChangeItemDurability(clientId, -1, data.maxDurability, selectedSlot))
                                        {
                                            Server_InteractCallback(clientId, true);
                                            return;
                                        }
                                    }
                                    Server_InteractCallback(clientId, false);
                                    return;
                                });
                            });
                        }
                        else
                        {
                            Server_InteractCallback(clientId, false);
                        }
                    }
                }
            }
        }
        else 
        {
            //LocalSoundManager.Singleton.PlaySound("PLAYER_FIST_SWING", NetworkingManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.position);

            if (distance < 3)//Hand
            {
                //LocalSoundManager.Singleton.PlaySound("PLAYER_FIST_HIT", NetworkingManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.position);
            }
        }
    }
    //CALLBACK
    private void Server_InteractCallback(ulong clientId, bool success) 
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteBool(success);
                InvokeClientRpcOnClientPerformance(InteractCallbackRpc, clientId, writeStream);
            }
        }
    }
    [ClientRPC]
    private void InteractCallbackRpc(ulong clientId, Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            playerActionManager.PlayerInteractCallback(reader.ReadBool());
            
        }
    }
    public bool GetPlayerIsDead(ulong clientId) 
    {
        return playerInfoSystem.GetPlayerDead(clientId);
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
                Server_RespawnPlayerTask(clientId);
            }
        }
    }
    public void Server_RespawnPlayerTask(ulong clientId) 
    {
        GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
        Vector3 spawnpoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length)].transform.position;
        playerCommandSystem.Teleport_ToVector(clientId, spawnpoint);
        playerInfoSystem.SetPlayerDead(clientId, false);
        playerInfoSystem.ResetPlayerInfo(clientId, spawnpoint);
        Server_UIHideDeathScreen(clientId);
        Server_UICloseInventory(clientId);
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

    //--Request to Send Player Command
    //CLIENT
    public void ClientSendPlayerCommand(PlayerCommand command)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                //CommandBreakdown
                writer.WriteVector2Packed(command.move);
                writer.WriteVector2Packed(command.look);
                writer.WriteVector2Packed(command.sensitivity);
                writer.WriteBool(command.jump);
                writer.WriteBool(command.crouch);
                InvokeServerRpcPerformance(Server_AuthenticatePlayerCommand, writeStream, "PlayerCommand");
            }
        }
    }
    //SERVER
    [ServerRPC(RequireOwnership = false)]
    private void Server_AuthenticatePlayerCommand(ulong _clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            //PlayerCommand Breakdown
            playerCommandSystem.ExecuteCommand(new PlayerCommand()
            {
                clientId = _clientId,
                move = reader.ReadVector2Packed(),
                look = reader.ReadVector2Packed(),
                sensitivity = reader.ReadVector2Packed(),
                jump = reader.ReadBool(),
                crouch = reader.ReadBool()
            });
        }
    }

    public void ServerSendSnapshot(Snapshot snapshot) 
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                ulong[] clientIds = networkingManager.ConnectedClients.Keys.ToArray();

                for (int i = 0; i < clientIds.Length; i++)
                {
                    if (playerCommandSystem.players.ContainsKey(clientIds[i]))
                    {
                        Snapshot talored = Snapshot.TrimForDistance(playerCommandSystem.players[clientIds[i]].transform.position, snapshot);
                        writer.WriteInt32Packed(talored.snapshotId);
                        writer.WriteSinglePacked(talored.networkTime);
                        if (talored.players != null)
                        {
                            writer.WriteInt32Packed(talored.players.Length);
                            for (int e = 0; e < talored.players.Length; e++)
                            {
                                writer.WriteUInt64Packed(talored.players[e].networkId);
                                writer.WriteVector3Packed(talored.players[e].location);
                                writer.WriteVector2Packed(talored.players[e].rotation);
                            }
                        }
                        else
                        {
                            writer.WriteInt32Packed(0);
                        }
                        Debug.Log("Current Snapshot Size: " + writeStream.BitLength / 8 + " bytes");
                        InvokeClientRpcOnClientPerformance(Client_RecieveWorldSnapshot, clientIds[i], writeStream);
                    }
                    else
                    {
                        writer.WriteInt32Packed(snapshot.snapshotId);
                        writer.WriteSinglePacked(snapshot.networkTime);
                        if (snapshot.players != null)
                        {
                            writer.WriteInt32Packed(snapshot.players.Length);
                            for (int e = 0; e < snapshot.players.Length; e++)
                            {
                                writer.WriteUInt64Packed(snapshot.players[e].networkId);
                                writer.WriteVector3Packed(snapshot.players[e].location);
                                writer.WriteVector2Packed(snapshot.players[e].rotation);
                            }
                        }
                        else
                        {
                            writer.WriteInt32Packed(0);
                        }
                        InvokeClientRpcOnClientPerformance(Client_RecieveWorldSnapshot, clientIds[i], writeStream);
                    }
                }
            }
        } 
    }

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

            int playerLength = reader.ReadInt32Packed();
            if(playerLength > 0) 
            {
                List<Snapshot_Player> playerList = new List<Snapshot_Player>();
                for (int i = 0; i < playerLength; i++)
                {
                    playerList.Add(new Snapshot_Player() 
                    {
                        networkId = reader.ReadUInt64Packed(),
                        location = reader.ReadVector3Packed(),
                        rotation = reader.ReadVector2Packed()
                    });
                }
                snapshot.players = playerList.ToArray();
            }
            snapshotManager.ProcessSnapshot(snapshot);
            if (networkingManager != null)
            {
                networkPing = Mathf.Clamp((int)(networkingManager.NetworkTime - snapshot.networkTime) * 1000, 1, 999);
            }
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
    private string ItemArrayToJson(Item[] items)
    {
        return JsonHelper.ToJson(items);
    }
    private Item[] ItemArrayFromJson(string json) 
    {
        return JsonHelper.FromJson<Item>(json);
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
//1156 6/20/20
//1692 6/25/20
//2106 7/11/20
//2212 8/15/20
//1451 8/25/20
//1123 9/22/20
//1374 11/23/20