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
    public GameObject deathDropPrefab;
    public GameObject[] particlePrefabs;


    // -- SYSTEMS
    private PlayerInfoSystem pis;
    private WorldAISystem worldAISystem;
    private WorldObjectSystem worldObjectSystem;

    // -- MANAGERS
    private PlayerInfoManager playerInfoManager;
    private PlayerActionManager playerActionManager;
    private ClickableSystem clickableSystem;

    //Properties
    private ServerProperties storedProperties;

    //Temp Location for Respawn
    public Vector3 tempPlayerPosition = new Vector3(0, -5000, 0);

    //Temp Invent ory
    private Item[] tempInventory;


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

        //Start Player Info System
        pis = GetComponent<PlayerInfoSystem>();
        if (pis == null || !pis.StartSystem())
        {
            DebugMsg.End(1, "Failed to Start Player Info System", 1);
            return;
        }

        //Start World AI System
        worldAISystem = GetComponent<WorldAISystem>();
        if (worldAISystem == null || !worldAISystem.StartSystem())
        {
            DebugMsg.End(1, "Failed to Start World AI System", 1);
            return;
        }

        //Start World Object System
        worldObjectSystem = GetComponent<WorldObjectSystem>();
        if (worldObjectSystem == null || !worldObjectSystem.StartSystem())
        {
            DebugMsg.End(1, "Failed to Start World Object System", 1);
            return;
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
    }

    //-----------------------------------------------------------------//
    //             CLIENT CALLBACKS                                    //
    //-----------------------------------------------------------------//

    //Player has Connected callback.
    public void PlayerConnected_Player(ulong networkId)
    {
        InvokeServerRpc(HandoverNetworkId, OwnerClientId, PlayerPrefs.GetInt("userId"), PlayerPrefs.GetString("authKey"), networkId);
    }

    //Player has Connectd handover.
    [ServerRPC(RequireOwnership = false)]
    private void HandoverNetworkId(ulong clientId, int id, string authKey, ulong networkId)
    {
        pis.SetPlayerNetworkId(clientId, id, authKey, networkId);
    }

    //Player has Disconnected callback.
    public void PlayerDisconnected_Player(ulong obj)
    {

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
        foreach(string data in itemData.itemUse) 
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
        if(items != null) 
        {
            GameObject deathDropObj = Instantiate(deathDropPrefab, location, Quaternion.identity);
            deathDropObj.transform.position = location;
            DeathDrop deathDrop = deathDropObj.GetComponent<DeathDrop>();
            NetworkedObject networkedObject = deathDropObj.GetComponent<NetworkedObject>();
            List<Item> dropItemTemp = items.ToList();
            if(armor != null) 
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
        else if(armor != null) 
        {
            GameObject deathDropObj = Instantiate(deathDropPrefab, location, Quaternion.identity);
            deathDropObj.transform.position = location;
            DeathDrop deathDrop = deathDropObj.GetComponent<DeathDrop>();
            NetworkedObject networkedObject = deathDropObj.GetComponent<NetworkedObject>();
            deathDrop.UpdateDropItems(armor.ToList());
            deathDrop.unique= GenerateUnique();
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
        PlayerActionManager.singleton.ShowDeathScreen();
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
    //             Player Request : Retrieve Player Info               //
    //-----------------------------------------------------------------//

    //--Request Health
    public void GetPlayerHealth(ulong clientId, Action<int> callback)
    {
        DebugMsg.Notify("Requesting Health.", 2);
        StartCoroutine(GetPlayerHealth_Wait(clientId, returnValue =>
        {
            DebugMsg.Notify("Requesting Health Success. Amount: " + returnValue, 2);
            callback(returnValue);
        }));
    }

    private IEnumerator GetPlayerHealth_Wait(ulong clientId, Action<int> callback)
    {
        RpcResponse<int> response = InvokeServerRpc(GetPlayerHealth_Rpc, clientId);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }

    [ServerRPC(RequireOwnership = false)]
    private int GetPlayerHealth_Rpc(ulong clientId)
    {
        return pis.GetPlayerHealth(clientId);
    }
    

    //--Request Water
    public void GetPlayerWater(ulong clientId, Action<int> callback)
    {
        DebugMsg.Notify("Requesting Water.", 2);
        StartCoroutine(GetPlayerWater_Wait(clientId, returnValue =>
        {
            DebugMsg.Notify("Requesting Water Success. Amount: " + returnValue, 2);
            callback(returnValue);
        }));
    }

    private IEnumerator GetPlayerWater_Wait(ulong clientId, Action<int> callback)
    {
        RpcResponse<int> response = InvokeServerRpc(GetPlayerWater_Rpc, clientId);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }

    [ServerRPC(RequireOwnership = false)]
    private int GetPlayerWater_Rpc(ulong clientId)
    {
        return pis.GetPlayerWater(clientId);
    }


    //--Request Food
    public void GetPlayerFood(ulong clientId, Action<int> callback)
    {
        DebugMsg.Notify("Requesting Food.", 2);
        StartCoroutine(GetPlayerFood_Wait(clientId, returnValue =>
        {
            DebugMsg.Notify("Requesting Food Success. Amount: " + returnValue, 2);
            callback(returnValue);
        }));
    }
    private IEnumerator GetPlayerFood_Wait(ulong clientId, Action<int> callback)
    {
        RpcResponse<int> response = InvokeServerRpc(GetPlayerFood_Rpc, clientId);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private int GetPlayerFood_Rpc(ulong clientId)
    {
        return pis.GetPlayerFood(clientId);
    }


    //--Request Inventory items
    public void GetPlayerInventoryItems(ulong clientId, Action<Item[]> callback)
    {
        DebugMsg.Notify("Requesting Inventory Items.", 2);
        StartCoroutine(GetPlayerInventoryItems_Wait(clientId, returnValue => { callback(returnValue); }));
    }
    private IEnumerator GetPlayerInventoryItems_Wait(ulong clientId, Action<Item[]> callback)
    {
        RpcResponse<Item[]> response = InvokeServerRpc(GetPlayerInventoryItems_Rpc, clientId);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private Item[] GetPlayerInventoryItems_Rpc(ulong clientId)
    {
        return pis.GetPlayerItems(clientId);
    }


    //--Request Inventory armor
    public void GetPlayerInventoryArmor(ulong clientId, Action<Item[]> callback)
    {
        DebugMsg.Notify("Requesting Inventory Armor.", 2);
        StartCoroutine(GetPlayerInventoryArmor_Wait(clientId, returnValue => { callback(returnValue); }));
    }
    private IEnumerator GetPlayerInventoryArmor_Wait(ulong clientId, Action<Item[]> callback)
    {
        RpcResponse<Item[]> response = InvokeServerRpc(GetPlayerInventoryArmor_Rpc, clientId);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private Item[] GetPlayerInventoryArmor_Rpc(ulong clientId)
    {
        return pis.GetPlayerArmor(clientId);
    }


    //--Request Inventory blueprints
    public void GetPlayerInventoryBlueprints(ulong clientId, Action<int[]> callback)
    {
        DebugMsg.Notify("Requesting Inventory Blueprints", 2);
        StartCoroutine(GetPlayerInventoryBlueprints_Wait(clientId, returnValue => { callback(returnValue); }));
    }
    private IEnumerator GetPlayerInventoryBlueprints_Wait(ulong clientId, Action<int[]> callback)
    {
        RpcResponse<int[]> response = InvokeServerRpc(GetPlayerInventoryBlueprints_Rpc, clientId);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private int[] GetPlayerInventoryBlueprints_Rpc(ulong clientId)
    {
        return pis.GetPlayerBlueprints(clientId);
    }


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
        if(NetworkingManager.Singleton != null) 
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
            if(pis.Confirm(clientId, reader.ReadStringPacked().ToString()))
            {
               pis.SetPlayerLocation(clientId, new Vector3(reader.ReadSinglePacked(), reader.ReadSinglePacked(), reader.ReadSinglePacked()));
            }
        }
    }




    //-----------------------------------------------------------------//
    //         Player Request : Inventory Items Modification           //
    //-----------------------------------------------------------------//

    //--Move Player Item
    public void MovePlayerItemBySlot(ulong clientId, string authKey, int oldSlot, int newSlot)
    {
        DebugMsg.Notify("Requesting to Modify Inventory.", 2);
        InvokeServerRpc(MovePlayerItemBySlot_Rpc, clientId, authKey, oldSlot, newSlot);
    }
    [ServerRPC(RequireOwnership = false)]
    private void MovePlayerItemBySlot_Rpc(ulong clientId, string authKey, int oldSlot, int newSlot)
    {
        pis.Inventory_MoveItem(clientId, authKey, oldSlot, newSlot);
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
            if(returnedItem != null) 
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
    //             Player Request : Use Selected Item                  //
    //-----------------------------------------------------------------//

    //CLIENT : Use Selected Item
    public void UseSelectedItem(string authKey, int itemSlot, Transform aim) 
    {
        DebugMsg.Notify("Requesting to Use Selected Item", 2);
        Vector3 rot = aim.TransformDirection(Vector3.forward);
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                writer.WriteInt32Packed(itemSlot);
                writer.WriteSinglePacked(aim.position.x);
                writer.WriteSinglePacked(aim.position.y);
                writer.WriteSinglePacked(aim.position.z);
                writer.WriteSinglePacked(rot.x);
                writer.WriteSinglePacked(rot.y);
                writer.WriteSinglePacked(rot.z);
                InvokeServerRpcPerformance(UseSelectedItem_Rpc, writeStream);
            }
        }
    }

    //SERVER : Use Selected Item 
    [ServerRPC(RequireOwnership = false)]
    private void UseSelectedItem_Rpc(ulong clientId, Stream stream)
    {
        bool success = false;
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            string authKey = reader.ReadStringPacked().ToString();
            int itemSlot = reader.ReadInt32Packed();
            float xPos = reader.ReadSinglePacked();
            float yPos = reader.ReadSinglePacked();
            float zPos = reader.ReadSinglePacked();
            float xRot = reader.ReadSinglePacked();
            float yRot = reader.ReadSinglePacked();
            float zRot = reader.ReadSinglePacked();
          
            Vector3 aimPos = new Vector3(xPos, yPos, zPos);
            Vector3 aimRot = new Vector3(xRot, yRot, zRot);
            Item selectedItem = pis.Inventory_GetItemFromSlot(clientId, itemSlot);
            if (selectedItem != null)
            {
                ItemData selectedData = pis.Inventory_GetItemData(selectedItem.itemID);
                if (selectedData.useType == 1)
                {
                    success = ServerShootSelected(clientId, aimPos, aimRot, selectedItem, selectedData);
                }
                if (selectedData.useType == 2)
                {
                    success = ServerMeleeSelected(clientId, aimPos, aimRot, selectedItem, selectedData);
                }
                if (selectedData.useType == 3)
                {
                    success = ServerPlaceSelected(clientId, aimPos, aimRot, selectedItem, selectedData);
                }
            }
            else
            {
                success = ServerPunchSelected(clientId, aimPos, aimRot);
            }
        }
            InvokeClientRpcOnClient(UseSelectedItemReturn, clientId, success);
    }
    [ClientRPC]
    private void UseSelectedItemReturn(bool success) 
    {
        playerActionManager.UseSelectedItemReturn(success);
    }

    //Server Shoot Selected
    private bool ServerShootSelected(ulong clientId, Vector3 pos, Vector3 rot, Item item, ItemData data) 
    {
        bool wasTaken = pis.Inventory_ChangeItemDurability(clientId, -1, data.maxDurability, item.currSlot);
        if (item.durability > 0 && wasTaken)
        {
            NetworkedObject netObject = Server_RaycastRequest(pos, rot, true, data.useRange);
            if (netObject != null)
            {
                int damage = ServerGetItemDamage(data);
                if(damage > 0) 
                {
                    DebugMsg.Notify("Shoot. Damaging Network Object for Player: " + clientId, 2);
                    Server_DamageNetworkedObject(netObject, damage);
                }
            }
        }
        return wasTaken;
    }
    
    //Server Melee Seleceted
    private bool ServerMeleeSelected(ulong clientId, Vector3 pos, Vector3 rot, Item item, ItemData data) 
    {
        if (item.durability > 0 && pis.Inventory_ChangeItemDurability(clientId, -1, data.maxDurability, item.currSlot))
        {
            NetworkedObject netObject = Server_RaycastRequest(pos, rot, true, data.useRange);
            if (netObject != null)
            {
                int damage = ServerGetItemDamage(data);
                if (damage > 0)
                {
                    DebugMsg.Notify("Melee. Damaging Network Object for Player: " + clientId, 2);
                    Server_DamageNetworkedObject(netObject, damage);
                }
            }
            return true;
        }
        else { return false; }
    }
    
    //Server Place Selected
    private bool ServerPlaceSelected(ulong clientId, Vector3 pos, Vector3 rot, Item item, ItemData data) 
    {
        return false;
    }
    
    //Server Punch Selected
    private bool ServerPunchSelected(ulong clientId, Vector3 pos, Vector3 rot) 
    {
        NetworkedObject netObject = Server_RaycastRequest(pos, rot, true, 1);
        if (netObject != null)
        {
            DebugMsg.Notify("Punch. Damaging Network Object for Player: " + clientId, 2);
            Server_DamageNetworkedObject(netObject, 2);
        }
        return false;
    }

    //CLIENT : Add to Durability
    public void ReloadToDurability(string authKey, int slot)
    {
        DebugMsg.Notify("Requesting to Reload", 2);
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                writer.WriteInt32Packed(slot);
                InvokeServerRpcPerformance(ReloadToDurability_Rpc, writeStream);
            }
        }
    }

    //SERVER : Add to Durability 
    [ServerRPC(RequireOwnership = false)]
    private void ReloadToDurability_Rpc(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            string authKey = reader.ReadStringPacked().ToString();
            int slot = reader.ReadInt32Packed();
            pis.Inventory_ReloadToDurability(clientId, authKey, slot);
        }
    }





    //-----------------------------------------------------------------//
    //         Player Request : World Interactions                     //
    //-----------------------------------------------------------------//

    //--Interact with Clickable
    public void InteractWithClickable(ulong clientId, string authKey, string uniqueId)
    {
        DebugMsg.Notify("Requesting to Interact with Clickable.", 2);
        InvokeServerRpc(InteractWithClickable_Rpc, clientId, authKey, uniqueId);
    }
    [ServerRPC(RequireOwnership = false)]
    private void InteractWithClickable_Rpc(ulong clientId, string authKey, string uniqueId)
    {
        DebugMsg.Begin(24, "Starting Clickable Interaction", 3);
        
        if(pis.Confirm(clientId, authKey)) 
        {
            Clickable clickable = clickableSystem.FindClickableByUnique(uniqueId);
            if (clickable == null)
                return;
            //Pickup Object
            if (clickable.clickType == 1)
            {
                pis.Inventory_AddNew(clientId, Convert.ToInt32(clickable.data), 1, itemPlaced => 
                {
                    if (itemPlaced) 
                    {
                        clickableSystem.RemoveClickable(clickable);
                    }
                    DebugMsg.End(24, "Finished Clickable Interaction", 3);
                });
                
            }
            else if (clickable.clickType == 2)
            {
                string[] datas = clickable.data.Split(',');
                int itemId = Convert.ToInt32(datas[0]);
                int amount = Convert.ToInt32(datas[1]);
                pis.Inventory_AddNew(clientId, itemId, amount, itemPlaced => 
                { 
                    if (itemPlaced) 
                    {
                        clickableSystem.RemoveClickable(clickable);
                    }
                    DebugMsg.End(24, "Finished Clickable Interaction", 3);
                });
            }
            else if (clickable.clickType == 3)
            {
                Server_UIShowStorage(clientId, clickable.data);
            }
        }
    }

    //--Interact with Resource
    public void InteractWithResource(ulong clientId, string authKey, string uniqueId)
    {
        DebugMsg.Notify("Requesting to Interact with Resource.", 2);
        InvokeServerRpc(InteractWithResource_Rpc, clientId, authKey, uniqueId);
    }
    [ServerRPC(RequireOwnership = false)]
    private void InteractWithResource_Rpc(ulong clientId, string authKey, string uniqueId)
    {
        if (pis.Confirm(clientId, authKey))
        {
            //for (int i = 0; i < activeResources.Count; i++)
            //{
            //    if(activeResources[i].uniqueId == unique) 
            //    {
            //        int amountLeft = activeResources[i].gatherAmount - activeResources[i].gatherPerAmount;
            //        if (amountLeft >= 0) 
            //        {
            //            if (Server_AddNewItemToInventory(clientId, activeResources[i].gatherItemId, activeResources[i].gatherPerAmount)) 
            //            {
            //                if (amountLeft == 0)
            //                {
            //                   //Destroy Resource
            //                    Destroy(activeResources[i].gameObject);
            //                    break;
            //                }
            //                activeResources[i].gatherAmount -= activeResources[i].gatherPerAmount;
            //            }
            //        }
            //    }
            //}
        }
    }

    //--Interact With Death Drop
    public void InteractWithDeathDrop(ulong clientId, string authKey, string uniqueId, int itemSlot, Action<Item[]> callback)
    {
        DebugMsg.Notify("Requesting to Interact with DeathDrop.", 2);
        StartCoroutine(InteractWithDeathDropWait(clientId, authKey, uniqueId, itemSlot, returnValue =>
        {
            callback(returnValue);
        }));
    }
    private IEnumerator InteractWithDeathDropWait(ulong clientId, string authKey, string uniqueId, int itemSlot, Action<Item[]> callback) 
    {
        RpcResponse<Item[]> response = InvokeServerRpc(InteractWithDeathDrop_Rpc, clientId, authKey, uniqueId, itemSlot);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private Item[] InteractWithDeathDrop_Rpc(ulong clientId, string authKey, string uniqueId, int itemSlot)
    {
        Item[] placed = null;
        DeathDrop[] deathDrops = FindObjectsOfType<DeathDrop>();
        foreach (DeathDrop drop in deathDrops)
        {
            if (drop.unique == uniqueId)
            {
                if (itemSlot == 100)
                {
                    for (int e = 0; e < drop.dropItems.Count; e++)
                    {
                        pis.Inventory_Add(clientId, drop.dropItems[e], success =>
                        {
                             drop.dropItems.RemoveAt(e--);
                        });
                    }
                }
                else
                {
                    for (int e = 0; e < drop.dropItems.Count; e++)
                    {
                        if (drop.dropItems[e].currSlot == itemSlot)
                        {
                            pis.Inventory_Add(clientId, drop.dropItems[e], success =>
                            {
                                if (success)
                                {
                                    drop.dropItems.RemoveAt(e--);
                                }
                            });
                            break;
                        }
                    }
                }
                drop.UpdateDropItems();
                placed = drop.dropItems.ToArray();
                break;
            }
        }
        return placed;
    }

    //-- Place Placeable Object
    public void Client_PlacePlaceableObject(string authKey, int itemId, int itemSlot, Transform loc) 
    {
        DebugMsg.Notify("Requesting to Place Placeable", 2);
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteStringPacked(authKey);
                writer.WriteInt32Packed(itemId);
                writer.WriteInt32Packed(itemSlot);
                writer.WriteSinglePacked(loc.position.x);
                writer.WriteSinglePacked(loc.position.y);
                writer.WriteSinglePacked(loc.position.z);
                writer.WriteSinglePacked(loc.localRotation.x);
                writer.WriteSinglePacked(loc.localRotation.y);
                writer.WriteSinglePacked(loc.localRotation.z);
                InvokeServerRpcPerformance(Client_PlacePlaceableObjectRpc, writeStream);
            }
        }
    }
    [ServerRPC(RequireOwnership = false)]
    private void Client_PlacePlaceableObjectRpc(ulong clientId, Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            string authKey = reader.ReadStringPacked().ToString();
            int itemId = reader.ReadInt32Packed();
            int itemSlot = reader.ReadInt32Packed();
            float xPos = reader.ReadSinglePacked();
            float yPos = reader.ReadSinglePacked();
            float zPos = reader.ReadSinglePacked();
            float xRot = reader.ReadSinglePacked();
            float yRot = reader.ReadSinglePacked();
            float zRot = reader.ReadSinglePacked();
            Vector3 objPos = new Vector3(xPos, yPos, zPos);

            DebugMsg.Begin(20, "Trying to Place Placeable", 2);

            if(pis.Confirm(clientId, authKey)) 
            {
                ItemData selectedData = pis.Inventory_GetItemData(itemId);
                if (selectedData.isPlaceable)
                {
                    pis.Inventory_RemoveItem(clientId, authKey, itemSlot, droppedItem =>
                    {
                        if (droppedItem != null && droppedItem.itemID == itemId)
                        {
                            GameObject placeableObject = Instantiate(selectedData.placeableItem);
                            placeableObject.transform.position = objPos;
                            placeableObject.transform.localRotation = Quaternion.Euler(zRot, yRot, zRot);
                            CollideSensor collide = placeableObject.GetComponent<CollideSensor>();
                            if (collide.isOverlapping)
                            {
                                DebugMsg.End(20, "Could Not Place Placeable", 2);
                                Destroy(placeableObject);
                                InvokeClientRpcOnClient(UseSelectedItemReturn, clientId, false);
                            }
                            else
                            {
                                NetworkedObject networkObject = placeableObject.GetComponent<NetworkedObject>();
                                networkObject.Spawn();
                                Server_RegisterClickable(placeableObject, selectedData);
                                InvokeClientRpcOnClient(UseSelectedItemReturn, clientId, true);
                                DebugMsg.End(20, "Placed Placeable Successfully", 2);
                            }
                        }
                    });
                }
            }
        }
    }
    private void Server_RegisterClickable(GameObject clickableObject, ItemData data) 
    {
        Clickable clickable = clickableObject.GetComponent<Clickable>();
        if (clickable != null)
        {
            if(clickable.clickType == 3) 
            {
                UIData newData = new UIData();
                newData.type = clickable.uiType;
                clickableSystem.RegisterClickable(clickable, newData);
            }
        }
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
                GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
                Vector3 spawnpoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length)].transform.position;
                Server_TeleportPlayerToLocation(clientId, spawnpoint);
                pis.ResetPlayerInfo(clientId, spawnpoint);
                Server_UIHideDeathScreen(clientId);
            }
        }
    }

    //Request Network Ping
    public void GetPlayerPing(ulong clientId, Action<int> callback)
    {
        DebugMsg.Notify("Requesting Ping.", 2);
        StartCoroutine(GetPlayerPing_Wait(clientId, returnValue =>
        {
            int ping = (int)((NetworkingManager.Singleton.NetworkTime - returnValue) * 1000);
            callback(ping);
        }));
    }
    private IEnumerator GetPlayerPing_Wait(ulong clientId, Action<float> callback)
    {
        RpcResponse<float> response = InvokeServerRpc(GetPlayerPing_Rpc, clientId);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private float GetPlayerPing_Rpc(ulong clientId)
    {
        //calculate ping for clientId
        return NetworkingManager.Singleton.NetworkTime;
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
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream);
                }
                else if (depth == 2) //Health
                {
                    writer.WriteInt32Packed(pis.GetPlayerHealth(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream);
                }
                else if (depth == 3) //Food
                {
                    writer.WriteInt32Packed(pis.GetPlayerFood(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream);
                }
                else if (depth == 4) //Water
                {
                    writer.WriteInt32Packed(pis.GetPlayerWater(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream);
                }
                else if (depth == 5) //Items
                {
                    writer.WriteStringPacked(ItemArrayToJson(pis.GetPlayerItems(clientId)));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream);
                }
                else if (depth == 6) //Armor
                {
                    writer.WriteStringPacked(ItemArrayToJson(pis.GetPlayerArmor(clientId)));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream);

                }
                else if (depth == 7) //Blueprints
                {
                    writer.WriteIntArrayPacked(pis.GetPlayerBlueprints(clientId));
                    InvokeClientRpcPerformance(SendInfoToPlayer_Rpc, (new ulong[] { clientId }).ToList(), writeStream);
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

        yield return new WaitForSeconds(interval * 60f);
        pis.AutoSave();
    }

}
//1156 6/20/20
//1692 6/25/20
//2106 7/11/20
//2212 8/15/20
//1451 8/25/20