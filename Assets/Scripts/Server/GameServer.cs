using MLAPI;
using MLAPI.LagCompensation;
using MLAPI.Messaging;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        //Serialization for <Item> Object. 
        SerializationManager.RegisterSerializationHandlers<Item>((Stream stream, Item instance) =>
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(stream))
            {
                writer.WriteInt32Packed(instance.itemID);
                writer.WriteInt32Packed(instance.itemStack);
                writer.WriteInt32Packed(instance.maxItemStack);
                writer.WriteInt32Packed(instance.currSlot);
                writer.WriteInt32Packed(instance.armorType);
                writer.WriteInt32Packed(instance.durability);

                writer.WriteStringPacked(instance.special);

                writer.WriteBool(instance.isPlaceable);
                writer.WriteBool(instance.isCraftable);
                writer.WriteBool(instance.isHoldable);
                writer.WriteBool(instance.isArmor);
                writer.WriteBool(instance.showInInventory);

            }
        }, (Stream stream) =>
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                Item item = new Item();
                item.itemID = reader.ReadInt32Packed();
                item.itemStack = reader.ReadInt32Packed();
                item.maxItemStack = reader.ReadInt32Packed();
                item.currSlot = reader.ReadInt32Packed();
                item.armorType = reader.ReadInt32Packed();
                item.durability = reader.ReadInt32Packed();

                item.special = reader.ReadStringPacked().ToString();

                item.isPlaceable = reader.ReadBool();
                item.isCraftable = reader.ReadBool();
                item.isHoldable = reader.ReadBool();
                item.isArmor = reader.ReadBool();
                item.showInInventory = reader.ReadBool();
                return item;
            }
        });




    }

    #endregion
    [Header("Prefabs")]
    public GameObject deathDropPrefab;
    public GameObject[] particlePrefabs;


    // -- SYSTEMS
    [Header("Systems")]
    public PlayerInfoSystem pis;
    public WorldAISystem worldAISystem;
    public WorldObjectSystem worldObjectSystem;
    private ClickableSystem clickableSystem;
    public ChatSystem chatSystem;

    // -- MANAGERS
    [Header("Managers")]
    public PlayerInfoManager playerInfoManager;
    public PlayerActionManager playerActionManager;
    public ChatManager chatManager;

    //Properties
    private ServerProperties storedProperties;

    //Temp Location for Respawn
    public Vector3 tempPlayerPosition = new Vector3(0, -5000, 0);

    private void Start()
    {
        playerInfoManager = PlayerInfoManager.singleton;
        playerActionManager = PlayerActionManager.singleton;
        if (IsServer)
        {
            StartGameServer();
        }
    }


    //-----------------------------------------------------------------//
    //             SERVER FUNCTIONS                                    //
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
        if (pis == null || !pis.StartSystem())
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

        clickableSystem = GetComponent<ClickableSystem>();

        //Start Loops
        StartCoroutine(AutoSaveLoop());
        DebugMsg.End(1, "Game Server Started.", 1);
    }

    //Stop the Game Server
    public void StopGameServer()
    {
        DebugMsg.Begin(2, "Stopping Game Server.", 1);

        NetworkingManager.Singleton.StopServer();

        pis.StopSystem();


        DebugMsg.End(2, "Finsihed Stopping Game Server.", 1);

    }

    //Create New Player
    public bool CreatePlayer(PlayerInfo playerInfo)
    {
        return pis.CreatePlayer(playerInfo);
    }

    //Get Player Location
    public Vector3 GetPlayerLocation(ulong clientId)
    {
        return pis.GetPlayerLocation(clientId);
    }

    public void InitializePlayerInfo(ulong clientId)
    {
        pis.SetPlayerTime(clientId, DateTime.Now);
        pis.Inventory_AddNew(clientId, 21, 1, returnValue => { });
    }

    public void PlayerConnected(ulong clientId)
    {
        string playerName = pis.GetPlayerName(clientId);
        chatSystem.PlayerConnected_AllMessage(playerName);
        chatSystem.PlayerWelcome_Specific(playerName, storedProperties.serverName, clientId);
    }
    public void PlayerDisconnected(ulong clientId) 
    {
        chatSystem.PlayerDisconnected_AllMessage(pis.GetPlayerName(clientId));
    }


    //-----------------------------------------------------------------//
    //             CLIENT CALLBACKS                                    //
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
            pis.SetPlayerNetworkId(clientId, reader.ReadInt32Packed(), reader.ReadStringPacked().ToString(), reader.ReadUInt64Packed());
            if (GetPlayerIsDead(clientId))
            {
                Server_RespawnPlayerTask(clientId);
            }
            ForceRequestInfoById(clientId);
        }
    }




    //-----------------------------------------------------------------//
    //             SERVER SIDE TOOLS                                   //
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
    //             Server Action : Change Player Info                  //
    //-----------------------------------------------------------------//

    //Move Player To Active List
    public bool MovePlayerToActive(ulong clientId, int userId, string authKey)
    {
        return pis.MovePlayerToActive(clientId, userId, authKey);
    }

    //Move Player To Inactive List
    public PlayerInfo Server_MovePlayerToInactive(ulong clientId)
    {
        return pis.MovePlayerToInactive(clientId);
    }

    //Respawn Player
    public void Server_RespawnPlayer(ulong clientId)
    {
        DebugMsg.Notify("Respawning Player '" + clientId + "'.", 2);

        //Show the Death Screen
        Server_UIShowDeathScreen(clientId);

        //Teleport Player to Temp Location
        Server_TeleportPlayerToLocation(clientId, tempPlayerPosition);
        pis.SetPlayerLocation(clientId, tempPlayerPosition);

        //Spawn the Death Drop
        Server_SpawnDeathDrop(pis.GetPlayerItems(clientId), pis.GetPlayerArmor(clientId), NetworkingManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.position, pis.GetPlayerName(clientId));
        pis.ClearPlayerInventory(clientId);
        pis.SetPlayerDead(clientId, true);
        //Force Request Info
        ForceRequestInfoById(clientId);
    }

    //Teleport Player
    private void Server_TeleportPlayerToLocation(ulong clientId, Vector3 position)
    {
        MovementManager movement = NetworkingManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<MovementManager>();
        movement.TeleportClient(clientId, position);

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

    //Server Teleport Player to Player (TEMP)
    public void Server_Teleport(string playerName, string targetName)
    {
        //ulong clientId = 0;
        //ulong targetId = 0;

        //for (int i = 0; i < activePlayers.Count; i++)
        //{
        //    if (activePlayers[i].name == playerName)
        //    {
        //        clientId = activePlayers[i].clientId;
        //    }
        //    else if (activePlayers[i].name == targetName) 
        //    {
        //        targetId = activePlayers[i].clientId;
        //    }
        //}

        //if(clientId != 0 && targetId != 0) 
        //{
        //    MovementManager movement = NetworkingManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<MovementManager>();
        //    if (movement != null)
        //    {
        //        movement.TeleportClient(clientId, NetworkingManager.Singleton.ConnectedClients[targetId].PlayerObject.transform.position);
        //    }
        //}

    }




    //-----------------------------------------------------------------//
    //             Server Action : User Interface                      //
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
        List<ulong> clients = new List<ulong>();
        clients.Add(clientId);
        InvokeClientRpcOnClient(Server_UIShowDeathScreenRpc, clientId);
    }

    //Client RPC - Force Death Screen
    [ClientRPC]
    private void Server_UIShowDeathScreenRpc()
    {
        //PlayerActionManager.singleton.ShowDeathScreen();
    }

    //Hide Death Screen on Client
    public void Server_UIHideDeathScreen(ulong clientId)
    {
        List<ulong> clients = new List<ulong>();
        clients.Add(clientId);
        InvokeClientRpcOnClient(Server_UIHideDeathScreenRpc, clientId);
    }

    //Client RPC - Hide Death Screen
    [ClientRPC]
    private void Server_UIHideDeathScreenRpc()
    {
        //PlayerActionManager.singleton.HideDeathScreen();
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
    //             Player Request : Retrieve Player Info               //
    //-----------------------------------------------------------------//

    //--Request Player Name by Client Id
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
        return pis.GetPlayerName(clientId);
    }

    //--Request Player Name by Client Id
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




    //-----------------------------------------------------------------//
    //             Player Request : Modify Own Values                  //
    //-----------------------------------------------------------------//

    //--Set Player Location
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
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerLocation_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            if (pis.Confirm(clientId, reader.ReadStringPacked().ToString()))
            {
                pis.SetPlayerLocation(clientId, new Vector3(reader.ReadSinglePacked(), reader.ReadSinglePacked(), reader.ReadSinglePacked()));
            }
        }
    }




    //-----------------------------------------------------------------//
    //         Player Request : Inventory Items Modification           //
    //-----------------------------------------------------------------//

    //--Move Player Item
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
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            DebugMsg.Notify("Player Requesting to Modify Inventory.", 4);
            pis.Inventory_MoveItem(clientId, reader.ReadStringPacked().ToString(), reader.ReadInt32Packed(), reader.ReadInt32Packed());
        }
    }

    //--Remove Player Item
    public void RemovePlayerItemBySlot(ulong clientId, string authKey, int slot)
    {
        DebugMsg.Notify("Requesting to Modify Inventory.", 2);
        InvokeServerRpc(RemovePlayerItemBySlot_Rpc, clientId, authKey, slot);
    }
    [ServerRPC(RequireOwnership = false)]
    private void RemovePlayerItemBySlot_Rpc(ulong clientId, string authKey, int slot)
    {
        pis.Inventory_RemoveItem(clientId, authKey, slot, returnedItem =>
        {
            if (returnedItem != null)
            {
                //Drop returnedItem.itemID;
            }
        });
    }

    //--Craft Item
    public void CraftItemById(ulong clientId, string authKey, int itemId, int amount)
    {
        DebugMsg.Notify("Requesting to Modify Inventory.", 2);
        InvokeServerRpc(CraftItemById_Rpc, clientId, authKey, itemId, amount);
    }
    [ServerRPC(RequireOwnership = false)]
    private void CraftItemById_Rpc(ulong clientId, string authKey, int itemId, int amount)
    {
        pis.Inventory_CraftItem(clientId, authKey, itemId, amount);
    }




    //-----------------------------------------------------------------//
    //                     Player Interaction                          //
    //-----------------------------------------------------------------//

    //Player Interact
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
    [ServerRPC(RequireOwnership = false)]
    private void PlayerInteractRpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            if (pis.Confirm(clientId, reader.ReadStringPacked().ToString()))
            {
                DebugMsg.Notify("Interact Confirmed", 1);
                int selectedSlot = reader.ReadInt32Packed();
                Ray ray = reader.ReadRayPacked();
                LagCompensationManager.Simulate(clientId, () =>
                {
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 30))
                    {
                        if (hit.collider != null)
                        {
                            DebugMsg.Notify("Ray has collided with " + hit.collider.tag, 1);
                            NetworkedObject networkObject = hit.collider.GetComponentInParent<NetworkedObject>();
                            if(networkObject != null) 
                            {
                                Server_Interact(clientId, selectedSlot, hit.distance, hit.collider.tag, networkObject.NetworkId);
                            }
                        }
                    }
                });
            }
        }
    }

    //Server Interact
    private void Server_Interact(ulong clientId, int selectedSlot, float distance, string tag, ulong networkId) 
    {

        DebugMsg.Notify("Server_Interact Started", 1);
        Item item = pis.Inventory_GetItemFromSlot(clientId, selectedSlot);
        if (item != null) //Has Item in Hand 
        {
            DebugMsg.Notify("Item: " + item.itemID + " detected.", 1);
            ItemData data = pis.Inventory_GetItemData(item.itemID);
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
                            worldObjectSystem.DepleteWorldObject(networkId, data.toolId, data.toolGatherAmount, returnValue =>
                            {
                                pis.Inventory_AddNew(clientId, returnValue.itemId, returnValue.amount, itemPlaced =>
                                {
                                    if (itemPlaced)
                                    {

                                        DebugMsg.Notify("Interact Gather Added", 1);
                                        if (pis.Inventory_ChangeItemDurability(clientId, -1, data.maxDurability, selectedSlot))
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
        else if(distance < 3)//Hand
        {
            
        }
    }
    
    //Server Interact Callback
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

    //Get If Player Is Dead
    public bool GetPlayerIsDead(ulong clientId) 
    {
        return pis.GetPlayerDead(clientId);
    }




    //-----------------------------------------------------------------//
    //         Player Request :    Extras                              //
    //-----------------------------------------------------------------//

    // Player Request to Disconnect
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
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            string authKey = reader.ReadStringPacked().ToString();
            if (pis.Confirm(clientId, authKey))
            {
                NetworkingManager.Singleton.DisconnectClient(clientId);
            }
        }
    }

    // Player Request to Respawn
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
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            string authKey = reader.ReadStringPacked().ToString();
            if (pis.Confirm(clientId, authKey))
            {
                Server_RespawnPlayerTask(clientId);
            }
        }
    }
    public void Server_RespawnPlayerTask(ulong clientId) 
    {
        GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
        Vector3 spawnpoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length)].transform.position;
        Server_TeleportPlayerToLocation(clientId, spawnpoint);
        pis.SetPlayerDead(clientId, false);
        pis.ResetPlayerInfo(clientId, spawnpoint);
        Server_UIHideDeathScreen(clientId);
        Server_UICloseInventory(clientId);
    }

    //Request Network Ping
    public void GetPlayerPing(ulong clientId, Action<int> callback)
    {
        float time = NetworkingManager.Singleton.NetworkTime;
        DebugMsg.Notify("Requesting Ping.", 2);
        StartCoroutine(GetPlayerPing_Wait(clientId, returnValue =>
        {
            int ping = (int)(((NetworkingManager.Singleton.NetworkTime - time)/ 2) * 1000);
            callback(ping);
        }));
    }
    private IEnumerator GetPlayerPing_Wait(ulong clientId, Action<bool> callback)
    {
        RpcResponse<bool> response = InvokeServerRpc(GetPlayerPing_Rpc, clientId, "UnSpeed");
        while (!response.IsDone) { yield return null; }
        callback(true);
    }
    [ServerRPC(RequireOwnership = false)]
    private bool GetPlayerPing_Rpc(ulong clientId)
    {
        //calculate ping for clientId
        return true;
    }

    //Cheating Debug
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
    [ServerRPC(RequireOwnership = false)]
    private void RequestToCheat_ItemRpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            pis.Inventory_AddNew(clientId, reader.ReadInt32Packed(), reader.ReadInt32Packed(), returnValue => { });
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
    //             Client RPC : Force Request                          //
    //-----------------------------------------------------------------//

    //------DEPTH KEY------//
    //   1 - ALL           //
    //   2 - HEALTH        //
    //   3 - FOOD          //
    //   4 - WATER         //
    //   5 - ITEMS         //
    //   6 - ARMOR         //
    //   7 - BLUEPRINTS    //

    public void ForceRequestInfoById(ulong clientId, int depth = 1) 
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(depth);
                if (depth == 1) //All
                {
                    PlayerInfo player = pis.GetPlayerInfo(clientId);
                    if (player != null)
                    {
                        writer.WriteInt32Packed(player.health);
                        writer.WriteInt32Packed(player.food);
                        writer.WriteInt32Packed(player.water);
                        writer.WriteVector3Packed(player.location);
                        writer.WriteStringPacked(ItemArrayToJson(player.items));
                        writer.WriteStringPacked(ItemArrayToJson(player.armor));
                        writer.WriteIntArrayPacked(player.blueprints);
                    }
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 2) //Health
                {
                    writer.WriteInt32Packed(pis.GetPlayerHealth(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 3) //Food
                {
                    writer.WriteInt32Packed(pis.GetPlayerFood(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 4) //Water
                {
                    writer.WriteInt32Packed(pis.GetPlayerWater(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 5) //Items
                {
                    writer.WriteStringPacked(ItemArrayToJson(pis.GetPlayerItems(clientId)));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 6) //Armor
                {
                    writer.WriteStringPacked(ItemArrayToJson(pis.GetPlayerArmor(clientId)));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");

                }
                else if (depth == 7) //Blueprints
                {
                    writer.WriteIntArrayPacked(pis.GetPlayerBlueprints(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream, "PlayerInfo");
                }
                else if (depth == 8) //Health / Food / Water
                {
                    PlayerInfo player = pis.GetPlayerInfo(clientId);
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
                    location = reader.ReadVector3Packed(),
                    items = ItemArrayFromJson(reader.ReadStringPacked().ToString()),
                    armor = ItemArrayFromJson(reader.ReadStringPacked().ToString()),
                    blueprints = reader.ReadIntArrayPacked()
                };
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
                Item[] items = ItemArrayFromJson(reader.ReadStringPacked().ToString());
                playerInfoManager.UpdateItems(items);
            }
            else if (depth == 6)//Armor
            {
                Item[] armor = ItemArrayFromJson(reader.ReadStringPacked().ToString());
                playerInfoManager.UpdateArmor(armor);
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
            pis.AutoSave();
        }
    }

}
//1156 6/20/20
//1692 6/25/20
//2106 7/11/20
//2212 8/15/20
//1451 8/25/20