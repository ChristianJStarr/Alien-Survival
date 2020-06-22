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

                writer.WriteStringPacked(instance.special);

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

                item.special = reader.ReadStringPacked().ToString();

                item.isCraftable = reader.ReadBool();
                item.isHoldable = reader.ReadBool();
                item.isArmor = reader.ReadBool();
                item.showInInventory = reader.ReadBool();
                return item;
            }
        });

        ////Serialization for <Vector3> Object. 
        //SerializationManager.RegisterSerializationHandlers<Vector3>((Stream stream, Vector3 instance) =>
        //{
        //    using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        //    {
        //        float[] flarray= { instance.x, instance.y, instance.z };
        //        writer.WriteFloatArrayPacked(flarray);
                
        //    }
        //}, (Stream stream) =>
        //{
        //    using (PooledBitReader reader = PooledBitReader.Get(stream))
        //    {
        //        float[] flarray = reader.ReadFloatArrayPacked();
        //        Vector3 item = new Vector3
        //        {
        //            x = flarray[0],
        //            y = flarray[1],
        //            z = flarray[2]
        //        };
        //        return item;
        //    }
        //});
    }

    #endregion
    public GameObject deathDropPrefab;

    //Player lists
    public List<PlayerInfo> activePlayers;
    public List<PlayerInfo> inactivePlayers;

    //Object lists
    private List<GameObject> activeGameObjects;
    private List<Resource> activeResources;

    //Systems
    private PlayerInfoManager playerInfoManager;
    private ServerSaveData serverSaveData;
    private InventorySystem inventorySystem;
    private ClickableSystem clickableSystem;
    //Item Datas
    private ItemData[] allItems;

    //Properties
    private ServerProperties storedProperties;

    [SerializeField]
    private int logLevel = 3;

    private void Start()
    {
        allItems = Resources.LoadAll("Items", typeof(ItemData)).Cast<ItemData>().ToArray();
        playerInfoManager = PlayerInfoManager.singleton;
        if (IsServer)
        {
            StartGameServer();
        }
    }

    //-----------------------------------------------------------------//
    //             SERVER FUNCTIONS                                    //
    //-----------------------------------------------------------------//

    private void StartGameServer()
    {
        DebugMessage("Starting Game Server.", 1);
        activePlayers = new List<PlayerInfo>();
        inactivePlayers = new List<PlayerInfo>();
        
        serverSaveData = Resources.Load("Data/ServerSaveData") as ServerSaveData;
        if (serverSaveData.playerData != null)
        {
            inactivePlayers = serverSaveData.playerData;
            activeGameObjects = serverSaveData.objData;
        }
        activeResources = FindObjectsOfType<Resource>().ToList();
        inventorySystem = GetComponent<InventorySystem>();
        clickableSystem = GetComponent<ClickableSystem>();
        LoadGameObjects();

        storedProperties = ServerConnect.singleton.GetServerProperties();
        if (storedProperties != null)
        {
            StartCoroutine(AutoSaveLoop());
        }
    }
    public void StopGameServer()
    {
        DebugMessage("Stopping Game Server.", 1);
        inactivePlayers.Concat(activePlayers).ToList();
        if (serverSaveData != null)
        {
            serverSaveData.playerData = inactivePlayers;
            serverSaveData.objData = activeGameObjects;
            DebugMessage("Saving Server Data.", 1);
            NetworkingManager.Singleton.StopServer();
        }
    }
    public void ActiveManage(PlayerInfo info, bool add)
    {
        if (add)
        {
            DebugMessage("Added Player '" + info.name + "' to Active Players.", 2);
            DebugMessage("PlayerInfo -- " + info.health + " " + info.water + " " + info.food, 3);
            activePlayers.Add(info);
        }
        else
        {
            DebugMessage("Removed Player '" + info.name + "' to Active Players.", 2);
            DebugMessage("PlayerInfo -- " + info.health + " " + info.water + " " + info.food, 3);
            activePlayers.Remove(info);
        }
    }
    public void InactiveManage(PlayerInfo info, bool add)
    {
        if (add)
        {
            DebugMessage("Added Player '" + info.name + "' to Inactive Players.", 2);
            inactivePlayers.Add(info);
        }
        else
        {
            DebugMessage("Removed Player '" + info.name + "' to Inactive Players.", 2);
            inactivePlayers.Remove(info);
        }
    }


    //-----------------------------------------------------------------//
    //             CLIENT CALLBACKS                                    //
    //-----------------------------------------------------------------//

    //Player has Connected callback.
    public void PlayerConnected_Player(ulong networkId)
    {
        Debug.Log("Network - Game - Connected to Server.");
        InvokeServerRpc(HandoverNetworkId, PlayerPrefs.GetInt("userId"), PlayerPrefs.GetString("authKey"), networkId);
    }

    //Player has Connectd handover.
    [ServerRPC(RequireOwnership = false)]
    private void HandoverNetworkId(int id, string authKey, ulong networkId)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                DebugMessage("Handing Over Player Network ID '" + activePlayers[i].name + "' to Active Players.", 3);
                activePlayers[i].networkId = networkId;
                break;
            }
        }
    }

    //Player has Disconnected callback.
    public void PlayerDisconnected_Player(ulong obj)
    {

    }

    //-----------------------------------------------------------------//
    //             SERVER SIDE TOOLS                                   //
    //-----------------------------------------------------------------//


    //Get ItemData by Item ID
    public ItemData GetItemDataById(int id)
    {
        ItemData itemData = null;
        foreach (ItemData data in allItems)
        {
            if (data.itemID == id)
            {
                itemData = data;
                break;
            }
        }
        return itemData;
    }


    //Create Item from Item Data
    private Item CreateItemFromData(ItemData itemData, int id, int amount)
    {
        Item item = new Item();
        item.itemID = id;
        item.itemStack = amount;
        item.maxItemStack = itemData.maxItemStack;
        item.currSlot = 44;
        item.armorType = itemData.armorType;
        item.isCraftable = itemData.isCraftable;
        item.isHoldable = itemData.isHoldable;
        item.isArmor = itemData.isArmor;
        item.showInInventory = itemData.showInInventory;
        return item;
    }


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

    //-----------------------------------------------------------------//
    //             Server Action : Change Player Info                  //
    //-----------------------------------------------------------------//

    //Health
    public void ServerSetHealth(ulong clientId, int amount)
    {

        DebugMessage("Setting Health of Player '" + clientId + "'.", 2);
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].clientId == clientId)
            {
                activePlayers[i].health += amount;
                ForceRequestInfoById(clientId, 2);
                break;
            }
            else
            {
                DebugMessage("Unable to Set Health of Player '" + clientId + "'.", 2);
            }
        }
    }

    //Inventory Items Add by ID
    public bool ServerAddNewItemToInventory(ulong clientId, int id, int amount)
    {
        bool returnValue = false;
        DebugMessage("Adding Item(s) to Player '" + clientId + "'.", 2);
        ItemData itemData = GetItemDataById(id);
        if (itemData != null)
        {
            while (amount > 0)
            {
                if (amount > itemData.maxItemStack)
                {
                    amount -= itemData.maxItemStack;
                    ServerAddItemToInventory(clientId, CreateItemFromData(itemData, id, itemData.maxItemStack));
                }
                else
                {
                    returnValue = ServerAddItemToInventory(clientId, CreateItemFromData(itemData, id, amount));
                    break;
                }
            }
        }
        return returnValue;
    }

    //Inventory Items Add by Item
    public bool ServerAddItemToInventory(ulong clientId, Item item)
    {
        item.currSlot = 44;
        bool wasPlaced = false;
        DebugMessage("Adding Item(s) to Player '" + clientId + "'.", 2);
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].clientId == clientId)
            {
                activePlayers[i].items = inventorySystem.AddItemToInventory(item, activePlayers[i].items, returnValue => 
                {
                    wasPlaced = returnValue;
                });
                ForceRequestInfoById(clientId, 5);
                break;
            }
        }
        return wasPlaced;
    }

    //Inventory Items Add by Item
    public void ServerCraftItemToInventory(ulong clientId, ItemData item, int amount)
    {
        DebugMessage("Crafting Item to Player '" + clientId + "'.", 2);
        if (item != null)
        {
            for (int i = 0; i < activePlayers.Count; i++)
            {
                if (activePlayers[i].clientId == clientId)
                {
                    Item[] newInventory = activePlayers[i].items;
                    while (amount > 0)
                    {
                        if (amount > item.maxItemStack)
                        {
                            amount -= item.maxItemStack;
                            newInventory = inventorySystem.AddItemToInventory(CreateItemFromData(item, item.itemID, item.maxItemStack), newInventory, returnValue => { });
                        }
                        else
                        {
                            newInventory = inventorySystem.AddItemToInventory(CreateItemFromData(item, item.itemID, amount), newInventory, returnValue => { });
                            break;
                        }
                    }
                    activePlayers[i].items = inventorySystem.RemoveItemsByRecipe(item.recipe, amount, newInventory, allItems);
                    ForceRequestInfoById(clientId, 5);
                    break;
                }
                else
                {
                    DebugMessage("Unable to Craft Item to Player '" + clientId + "'.", 1);
                }
            }
        }
    }

    //Inventory Items Remove
    public void ServerRemoveItemFromInventory(ulong clientId, int itemId, int amount)
    {
        DebugMessage("Removing Item from Player '" + clientId + "'.", 2);
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].clientId == clientId)
            {
                activePlayers[i].items = inventorySystem.RemoveItemFromInventory(itemId, amount, activePlayers[i].items);
                ForceRequestInfoById(clientId, 5);
                break;
            }
            else
            {
                DebugMessage("Unable to Add Item to Player '" + clientId + "'.", 1);
            }
        }
    }

    //Inventory Blueprints Add
    public void ServerAddBlueprint(ulong clientId, Item newBp)
    {
        DebugMessage("Adding Blueprint to Player '" + clientId + "'.", 2);
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].clientId == clientId)
            {
                activePlayers[i].blueprints = inventorySystem.AddBlueprintToBlueprints(newBp.itemID, activePlayers[i].blueprints);
                ForceRequestInfoById(clientId, 7);
                break;
            }
            else
            {
                DebugMessage("Unable to Add Blueprint to Player '" + clientId + "'.", 1);
            }
        }
    }

    //Inventory Blueprints Whipe
    public void ServerWipeBlueprints(ulong clientId)
    {
        DebugMessage("Wiping Blueprints of Player '" + clientId + "'.", 2);
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].clientId == clientId)
            {
                activePlayers[i].blueprints = null;
                ForceRequestInfoById(clientId, 7);
                break;
            }
            else
            {
                DebugMessage("Unable to Wipe Blueprints of Player '" + clientId + "'.", 2);
            }
        }
    }

    //Move Player To Inactive List
    public void MovePlayerToInactive(ulong clientId)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].clientId == clientId)
            {
                inactivePlayers.Add(activePlayers[i]);
                activePlayers.Remove(activePlayers[i]);
                break;
            }
        }
    }

    //Respawn Player
    public void ServerRespawnPlayer(ulong clientId)
    {
        DebugMessage("Respawning Player '" + clientId + "'.", 2);
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].clientId == clientId)
            {
                activePlayers[i].health = 100;
                activePlayers[i].food = 100;
                activePlayers[i].water = 100;
                SpawnDeathDrop(activePlayers[i].items, activePlayers[i].armor, NetworkingManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.position, activePlayers[i].name);
                activePlayers[i].items = activePlayers[i].armor = null;
                GameObject[] availableSpawns = GameObject.FindGameObjectsWithTag("spawnpoint");
                Transform spawnpoint = availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Length)].transform;
                MovementManager movement = NetworkingManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<MovementManager>();
                movement.TeleportClient(clientId, spawnpoint.position);
                activePlayers[i].location = spawnpoint.position;
                ServerUIDeathScreen(clientId);
                ForceRequestInfoById(clientId);
                
                break;
            }
            else
            {
                DebugMessage("Unable to Set Health of Player '" + clientId + "'.", 2);
            }
        }
    }

    //Spawn DeathDrop
    private void SpawnDeathDrop(Item[] items, Item[] armor, Vector3 location, string username)
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

    //Server Deplete Resource
    private void ServerDepleteResource(ulong clientId, string unique)
    {
        for (int i = 0; i < activeResources.Count; i++)
        {
            if(activeResources[i].unique == unique) 
            {
                int amountLeft = activeResources[i].gatherAmount - activeResources[i].gatherPerAmount;
                if (amountLeft >= 0) 
                {
                    if (ServerAddNewItemToInventory(clientId, activeResources[i].gatherItemId, activeResources[i].gatherPerAmount)) 
                    {
                        if (amountLeft == 0)
                        {
                            //Destroy Resource
                            Destroy(activeResources[i].gameObject);
                            break;
                        }
                        activeResources[i].gatherAmount -= activeResources[i].gatherPerAmount;
                    }
                }
            }
        }
    }
    //-----------------------------------------------------------------//
    //             Server Action : User Interface                      //
    //-----------------------------------------------------------------//

    public void ServerUIDeathScreen(ulong clientId)
    {
        List<ulong> clients = new List<ulong>();
        clients.Add(clientId);
        InvokeClientRpcOnClient(ServerUIDeathScreenRpc, clientId);
    }

    [ClientRPC]
    private void ServerUIDeathScreenRpc()
    {
        PlayerActionManager.singleton.ShowDeathScreen();
    }


    //-----------------------------------------------------------------//
    //             Server Action : Handle GameObjs                     //
    //-----------------------------------------------------------------//

    //Load all Game Objects.
    private void LoadGameObjects()
    {
        DebugMessage("Loading Game Objects.", 2);
        if (activeGameObjects != null)
        {
            if (activeGameObjects.Count > 0)
            {
                foreach (GameObject obj in activeGameObjects)
                {
                    GameObject newObject = Instantiate(obj, obj.transform.position, obj.transform.rotation);
                    Clickable clickable = newObject.GetComponent<Clickable>();
                    Placeable placeable = newObject.GetComponent<Placeable>();
                    if (clickable != null)
                    {

                    }
                    else if (placeable != null)
                    {

                    }
                }
            }
        }
    }

    //-----------------------------------------------------------------//
    //             Player Request : Retrieve Player Info               //
    //-----------------------------------------------------------------//


    //--Request Health
    public void GetPlayerHealth(int id, Action<int> callback)
    {
        DebugMessage("Requesting Health.", 2);
        StartCoroutine(GetPlayerHealth_Wait(id, returnValue =>
        {
            DebugMessage("Requesting Health Success. Amount: " + returnValue, 2);
            callback(returnValue);
        }));
    }

    private IEnumerator GetPlayerHealth_Wait(int id, Action<int> callback)
    {
        RpcResponse<int> response = InvokeServerRpc(GetPlayerHealth_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }

    [ServerRPC(RequireOwnership = false)]
    private int GetPlayerHealth_Rpc(int id)
    {
        int value = 200;
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id)
            {
                value = activePlayers[i].health;
                break;
            }
        }
        if (value == 200)
        {
            DebugMessage("Unable to Get Health of Player ID: " + id, 1);
            value = 100;
        }
        return value;
    }


    //--Request Water
    public void GetPlayerWater(int id, Action<int> callback)
    {
        DebugMessage("Requesting Water.", 2);
        StartCoroutine(GetPlayerWater_Wait(id, returnValue =>
        {
            DebugMessage("Requesting Water Success. Amount: " + returnValue, 2);
            callback(returnValue);
        }));
    }

    private IEnumerator GetPlayerWater_Wait(int id, Action<int> callback)
    {
        RpcResponse<int> response = InvokeServerRpc(GetPlayerWater_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }

    [ServerRPC(RequireOwnership = false)]
    private int GetPlayerWater_Rpc(int id)
    {
        int value = 200;
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id)
            {
                value = activePlayers[i].water;
                break;
            }
        }
        if (value == 200)
        {
            DebugMessage("Unable to Get Water of Player ID: " + id, 1);
            value = 100;
        }
        return value;
    }


    //--Request Food
    public void GetPlayerFood(int id, Action<int> callback)
    {
        DebugMessage("Requesting Food.", 2);
        StartCoroutine(GetPlayerFood_Wait(id, returnValue =>
        {
            DebugMessage("Requesting Food Success. Amount: " + returnValue, 2);
            callback(returnValue);
        }));
    }

    private IEnumerator GetPlayerFood_Wait(int id, Action<int> callback)
    {
        RpcResponse<int> response = InvokeServerRpc(GetPlayerFood_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }

    [ServerRPC(RequireOwnership = false)]
    private int GetPlayerFood_Rpc(int id)
    {
        int value = 200;
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id)
            {
                value = activePlayers[i].food;
                break;
            }
        }
        if (value == 200)
        {
            DebugMessage("Unable to Get Food of Player ID: " + id, 1);
            value = 100;
        }
        return value;
    }


    //--Request Inventory items
    public void GetPlayerInventoryItems(int id, Action<Item[]> callback)
    {
        DebugMessage("Requesting Inventory Items.", 2);
        StartCoroutine(GetPlayerInventoryItems_Wait(id, returnValue => { callback(returnValue); }));
    }

    private IEnumerator GetPlayerInventoryItems_Wait(int id, Action<Item[]> callback)
    {
        RpcResponse<Item[]> response = InvokeServerRpc(GetPlayerInventoryItems_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }

    [ServerRPC(RequireOwnership = false)]
    private Item[] GetPlayerInventoryItems_Rpc(int id)
    {
        Item[] value = null;
        bool unable = true;
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id)
            {
                unable = false;
                value = activePlayers[i].items;
                break;
            }
        }
        if (unable)
        {
            DebugMessage("Unable to Get Items of Player ID: " + id, 1);
        }
        return value;
    }


    //--Request Inventory armor
    public void GetPlayerInventoryArmor(int id, Action<Item[]> callback)
    {
        DebugMessage("Requesting Inventory Armor.", 2);
        StartCoroutine(GetPlayerInventoryArmor_Wait(id, returnValue => { callback(returnValue); }));
    }

    private IEnumerator GetPlayerInventoryArmor_Wait(int id, Action<Item[]> callback)
    {
        RpcResponse<Item[]> response = InvokeServerRpc(GetPlayerInventoryArmor_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }

    [ServerRPC(RequireOwnership = false)]
    private Item[] GetPlayerInventoryArmor_Rpc(int id)
    {
        Item[] value = null;
        bool unable = true;
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id)
            {
                unable = false;
                value = activePlayers[i].armor;
                break;
            }
        }
        if (unable)
        {
            DebugMessage("Unable to Get Armor of Player ID: " + id, 1);
        }
        return value;
    }


    //--Request Inventory blueprints
    public void GetPlayerInventoryBlueprints(int id, Action<int[]> callback)
    {
        DebugMessage("Requesting Inventory Blueprints", 2);
        StartCoroutine(GetPlayerInventoryBlueprints_Wait(id, returnValue => { callback(returnValue); }));
    }

    private IEnumerator GetPlayerInventoryBlueprints_Wait(int id, Action<int[]> callback)
    {
        RpcResponse<int[]> response = InvokeServerRpc(GetPlayerInventoryBlueprints_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }

    [ServerRPC(RequireOwnership = false)]
    private int[] GetPlayerInventoryBlueprints_Rpc(int id)
    {
        int[] value = null;
        bool unable = true;
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id)
            {
                unable = false;
                value = activePlayers[i].blueprints;
                break;
            }
        }
        if (unable)
        {
            DebugMessage("Unable to Get Blueprints of Player ID: " + id, 1);
        }
        return value;
    }


    //-----------------------------------------------------------------//
    //             Player Request : Modify Own Values                  //
    //-----------------------------------------------------------------//

    //--Set Player Health
    public void SetPlayerHealth(int id, string authKey, int health)
    {
        DebugMessage("Requesting to Modify Health.", 2);
        InvokeServerRpc(SetPlayerHealth_Rpc, id, authKey, health);
    }
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerHealth_Rpc(int id, string authKey, int health)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                activePlayers[i].health += health;
                if(activePlayers[i].health < 0) 
                {
                    activePlayers[i].health = 0;
                    ServerRespawnPlayer(activePlayers[i].clientId);
                }
                else if(activePlayers[i].health == 0) 
                {
                    ServerRespawnPlayer(activePlayers[i].clientId);
                }
                DebugMessage("Setting Health of Player '" + activePlayers[i].name + "'.", 2);
                ForceRequestInfoById(activePlayers[i].clientId, 2);
                break;
            }
        }
    }


    //--Set Player Water
    public void SetPlayerWater(int id, string authKey, int water)
    {
        DebugMessage("Requesting to Modify Water.", 2);
        InvokeServerRpc(SetPlayerWater_Rpc, id, authKey, water);
    }

    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerWater_Rpc(int id, string authKey, int water)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                activePlayers[i].water += water;
                DebugMessage("Setting Water of Player '" + activePlayers[i].name + "'.", 2);
                ForceRequestInfoById(activePlayers[i].clientId, 4);
                break;
            }
        }
    }


    //--Set Player Food
    public void SetPlayerFood(int id, string authKey, int food)
    {
        DebugMessage("Requesting to Modify Food.", 2);
        InvokeServerRpc(SetPlayerFood_Rpc, id, authKey, food);
    }

    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerFood_Rpc(int id, string authKey, int food)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                activePlayers[i].food += food;
                DebugMessage("Setting Food of Player '" + activePlayers[i].name + "'.", 2);
                ForceRequestInfoById(activePlayers[i].clientId, 3);
                break;
            }
        }
    }


    //--Set Player Location
    public void SetPlayerLocation(int id, string authKey, Vector3 location)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(id);
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
            for (int i = 0; i < activePlayers.Count; i++)
            {
                if (activePlayers[i].id == reader.ReadInt32Packed() && activePlayers[i].authKey == reader.ReadStringPacked().ToString())
                {
                    activePlayers[i].location = new Vector3(reader.ReadSinglePacked(), reader.ReadSinglePacked(), reader.ReadSinglePacked());
                    DebugMessage("Setting Location of Player '" + activePlayers[i].name + "'.", 3);
                    break;
                }
            }
        }
    }


    //--Request To Die
    public void RequestToDie(int id, string authKey)
    {
        DebugMessage("Requesting to Modify Food.", 2);
        InvokeServerRpc(RequestToDie_Rpc, id, authKey);
    }

    [ServerRPC(RequireOwnership = false)]
    private void RequestToDie_Rpc(int id, string authKey)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey && activePlayers[i].health == 0)
            {
                ServerRespawnPlayer(activePlayers[i].clientId);
                break;
            }
        }
    }

    //-----------------------------------------------------------------//
    //         Player Request : Inventory Items Modification           //
    //-----------------------------------------------------------------//

    //--Move Player Inventory Item by Slot
    public void MovePlayerItemBySlot(int id, string authKey, int curSlot, int newSlot)
    {
        DebugMessage("Requesting to Modify Inventory.", 2);
        if (curSlot > 33 || newSlot > 33)
        {
            InvokeServerRpc(MovePlayerItemArmorBySlot_Rpc, id, authKey, curSlot, newSlot);
        }
        else
        {
            InvokeServerRpc(MovePlayerItemBySlot_Rpc, id, authKey, curSlot, newSlot);
        }

    }

    [ServerRPC(RequireOwnership = false)]
    private void MovePlayerItemBySlot_Rpc(int id, string authKey, int curSlot, int newSlot)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                activePlayers[i].items = inventorySystem.MoveItemInInventory(curSlot, newSlot, activePlayers[i].items);
                DebugMessage("Modifying Inventory of Player '" + activePlayers[i].name + "'.", 2);
                ForceRequestInfoById(activePlayers[i].clientId, 5);
                break;
            }
        }
    }

    [ServerRPC(RequireOwnership = false)]
    private void MovePlayerItemArmorBySlot_Rpc(int id, string authKey, int curSlot, int newSlot)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                ItemArmorArrays arrays = inventorySystem.MoveArmorInInventory(curSlot, newSlot, activePlayers[i].items, activePlayers[i].armor);
                activePlayers[i].items = arrays.items;
                activePlayers[i].items = arrays.items;
                ForceRequestInfoById(activePlayers[i].clientId);
                break;
            }
        }
    }


    //--Remove Player Inventory Item by Slot
    public void RemovePlayerItemBySlot(int id, string authKey, int curSlot)
    {
        DebugMessage("Requesting to Modify Inventory.", 2);
        InvokeServerRpc(RemovePlayerItemBySlot_Rpc, id, authKey, curSlot);
    }

    [ServerRPC(RequireOwnership = false)]
    private void RemovePlayerItemBySlot_Rpc(int id, string authKey, int curSlot)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                activePlayers[i].items = inventorySystem.RemoveItemFromInventoryBySlot(curSlot, activePlayers[i].items, returnValue =>
                {
                    if (returnValue != null)
                    {
                            //Instantiate Item by ID
                        }
                });
                DebugMessage("Modifying Inventory of Player '" + activePlayers[i].name + "'.", 2);
                ForceRequestInfoById(activePlayers[i].clientId, 5);
                break;
            }
        }
    }


    //--Remove Player Craft Item by ID
    public void CraftItemById(int id, string authKey, int itemId, int amount)
    {
        DebugMessage("Requesting to Modify Inventory.", 2);
        InvokeServerRpc(CraftItemById_Rpc, id, authKey, itemId, amount);
    }

    [ServerRPC(RequireOwnership = false)]
    private void CraftItemById_Rpc(int id, string authKey, int itemId, int amount)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                List<InventoryResource> invResource = new List<InventoryResource>();
                foreach (Item itemRes in activePlayers[i].items)
                {
                    bool placed = false;
                    foreach (InventoryResource invItem in invResource)
                    {
                        if (invItem.itemId == itemRes.itemID)
                        {
                            invItem.itemAmount += itemRes.itemStack;
                            placed = true;
                            break;
                        }
                        else
                        {
                            placed = false;
                        }
                    }
                    if (!placed)
                    {
                        InventoryResource newRes = new InventoryResource();
                        newRes.itemId = itemRes.itemID;
                        newRes.itemAmount = itemRes.itemStack;
                        invResource.Add(newRes);
                    }
                }
                ItemData item = GetItemDataById(itemId);
                int recipeAmount = item.recipe.Length;
                int recipeAvail = 0;
                foreach (string recipe in item.recipe)
                {
                    string[] data = recipe.Split('-');
                    int itemd = Convert.ToInt32(data[0]);
                    int itemAmount = Convert.ToInt32(data[1]);
                    if (CraftItemByIdCheck(itemd, itemAmount, invResource))
                    {
                        recipeAvail++;
                    }
                }
                if (recipeAvail == recipeAmount)
                {
                    Item[] newInventory = activePlayers[i].items;

                    if (amount * item.craftAmount > item.maxItemStack)
                    {
                        while (amount > 0)
                        {
                            if (amount > item.maxItemStack)
                            {
                                amount -= item.maxItemStack;
                                newInventory = inventorySystem.AddItemToInventory(CreateItemFromData(item, item.itemID, item.maxItemStack), newInventory, returnValue => { });
                            }
                            else
                            {
                                newInventory = inventorySystem.AddItemToInventory(CreateItemFromData(item, item.itemID, amount * item.craftAmount), newInventory, returnValue => { });
                                break;
                            }
                        }
                    }
                    else
                    {
                        newInventory = inventorySystem.AddItemToInventory(CreateItemFromData(item, item.itemID, amount * item.craftAmount), newInventory, returnValue => { });
                    }
                    activePlayers[i].items = inventorySystem.RemoveItemsByRecipe(item.recipe, amount, newInventory, allItems);
                    ForceRequestInfoById(activePlayers[i].clientId, 5);
                }
                else
                {
                    //Not enough?
                }
                break;
            }
        }
    }
    private bool CraftItemByIdCheck(int id, int amount, List<InventoryResource> invResource)
    {
        bool hasItem = false;
        foreach (InventoryResource item in invResource)
        {
            if (item.itemId == id && item.itemAmount >= amount)
            {
                hasItem = true;
                break;
            }
        }
        return hasItem;
    }

    //-----------------------------------------------------------------//
    //             Player Request : Raycast Hit                        //
    //-----------------------------------------------------------------//

    //Raycast Hit Request by rayType
    public void PlayerRayCastHit(NetworkedObject player, int type)
    {
        InvokeServerRpc(PlayerRayCastHitRpc, player, type);
    }

    [ServerRPC(RequireOwnership = false)]
    private void PlayerRayCastHitRpc(NetworkedObject player, int type)
    {

        float distance = 1F;
        int damage = 1;
        if (type == 1) { distance = 2f; }
        if (type == 2) { distance = 8f; }
        if (type == 3) { distance = 30f; }
        if (type == 4) { distance = 50f; }

        Transform playerTransform = player.GetComponent<Transform>();
        RaycastHit hit;
        LagCompensationManager.Simulate(player.NetworkId, () =>
        {
            if (Physics.Raycast(playerTransform.position, -Vector3.up, out hit, distance))
            {
                GameObject hitObject = hit.collider.gameObject;
                Vector3 hitPosition = hitObject.transform.position;
                string tag = hitObject.tag;

                if (tag == "Player")
                {
                        //ulong playerId = hitObject.GetComponent<NetworkedObject>().NetworkId;
                        //PlayerInfo playerInfo = GetActivePlayerById(playerId);
                        //int health = playerInfo.health;
                        //if (health - damage <= 0)
                        //{
                        //    //Kill Player
                        //}
                        //else
                        //{
                        //    health -= damage;
                        //    ForceRequestInfoById(playerId, 2);
                        //}
                    }
                if (tag == "Enemy")
                {

                }
                if (tag == "Friendly")
                {

                }
                if (tag == "Tree")
                {

                }

            }
        });
    }


    //-----------------------------------------------------------------//
    //         Player Request : World Interactions                     //
    //-----------------------------------------------------------------//


    //--Interact with Clickable
    public void InteractWithClickable(int id, string authKey, string uniqueId)
    {
        DebugMessage("Requesting to Interact with Clickable.", 2);
        InvokeServerRpc(InteractWithClickable_Rpc, id, authKey, uniqueId);
    }

    [ServerRPC(RequireOwnership = false)]
    private void InteractWithClickable_Rpc(int id, string authKey, string uniqueId)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                clickableSystem.InteractWithClickable(activePlayers[i], uniqueId);
                DebugMessage("Player '" + activePlayers[i].name + "' Interacting with Clickable: " + uniqueId + ".", 2);
                break;
            }
        }
    }

    //--Interact with Resource
    public void InteractWithResource(int id, string authKey, string uniqueId)
    {
        DebugMessage("Requesting to Interact with Resource.", 2);
        InvokeServerRpc(InteractWithResource_Rpc, id, authKey, uniqueId);
    }

    [ServerRPC(RequireOwnership = false)]
    private void InteractWithResource_Rpc(int id, string authKey, string uniqueId)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                ServerDepleteResource(activePlayers[i].clientId, uniqueId);
                DebugMessage("Player '" + activePlayers[i].name + "' Interacting with Resource: " + uniqueId + ".", 2);
                break;
            }
        }
    }


    //--Interact With Death Drop
    public void InteractWithDeathDrop(int id, string authKey, string uniqueId, int itemSlot, Action<Item[]> callback)
    {
        DebugMessage("Requesting to Interact with DeathDrop.", 2);
        StartCoroutine(InteractWithDeathDropWait(id, authKey, uniqueId, itemSlot, returnValue =>
        {
            callback(returnValue);
        }));
    }
    private IEnumerator InteractWithDeathDropWait(int id, string authKey, string uniqueId, int itemSlot, Action<Item[]> callback) 
    {
        RpcResponse<Item[]> response = InvokeServerRpc(InteractWithDeathDrop_Rpc, id, authKey, uniqueId, itemSlot);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }

    [ServerRPC(RequireOwnership = false)]
    private Item[] InteractWithDeathDrop_Rpc(int id, string authKey, string uniqueId, int itemSlot)
    {
        Item[] placed = null;
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                DeathDrop[] deathDrops = FindObjectsOfType<DeathDrop>();
                foreach (DeathDrop drop in deathDrops)
                {
                    if(drop.unique == uniqueId) 
                    {
                        if(itemSlot == 100) 
                        {
                            for (int e = 0; e < drop.dropItems.Count; e++)
                            {
                                if(ServerAddItemToInventory(activePlayers[i].clientId, drop.dropItems[e])) 
                                {
                                    drop.dropItems.RemoveAt(e--);
                                    
                                }
                            }
                        }
                        else 
                        {
                            for (int e = 0; e < drop.dropItems.Count; e++)
                            {
                                if (drop.dropItems[e].currSlot == itemSlot)
                                {
                                    if (ServerAddItemToInventory(activePlayers[i].clientId, drop.dropItems[e]))
                                    {
                                        drop.dropItems.RemoveAt(e--);
                                        break;
                                    }
                                }
                            }
                        }
                        drop.UpdateDropItems();
                        placed = drop.dropItems.ToArray();
                        break;
                    }
                }
                DebugMessage("Player '" + activePlayers[i].name + "' Interacting with DeathDrop: " + uniqueId + ".", 2);
                break;
            }
        }
        return placed;
    }

    //-----------------------------------------------------------------//
    //             Client RPC : Force Request                          //
    //-----------------------------------------------------------------//
    private void ForceRequestInfoById(ulong clientId, int infoDepth = 1)
    {
        List<ulong> idList = new List<ulong>();
        idList.Add(clientId);
        DebugMessage("Forcing Player '" + clientId + "' to Request Info. Depth: " + infoDepth, 2);
        if (infoDepth == 1)
        {
            InvokeClientRpc("ForceRequestInfoAll_Rpc", idList);
        }
        if (infoDepth == 2)
        {
            InvokeClientRpc("ForceRequestInfoHealth_Rpc", idList);
        }
        if (infoDepth == 3)
        {
            InvokeClientRpc("ForceRequestInfoFood_Rpc", idList);
        }
        if (infoDepth == 4)
        {
            InvokeClientRpc("ForceRequestInfoWater_Rpc", idList);
        }
        if (infoDepth == 5)
        {
            InvokeClientRpc("ForceRequestInfoItems_Rpc", idList);
        }
        if (infoDepth == 6)
        {
            InvokeClientRpc("ForceRequestInfoArmor_Rpc", idList);
        }
        if (infoDepth == 7)
        {
            InvokeClientRpc("ForceRequestInfoBlueprints_Rpc", idList);
        }
    }
    [ClientRPC]
    private void ForceRequestInfoAll_Rpc()
    {
        playerInfoManager.GetPlayer_AllInfo();
    }
    [ClientRPC]
    private void ForceRequestInfoHealth_Rpc()
    {
        playerInfoManager.GetPlayer_Health();
    }
    [ClientRPC]
    private void ForceRequestInfoFood_Rpc()
    {
        playerInfoManager.GetPlayer_Food();
    }
    [ClientRPC]
    private void ForceRequestInfoWater_Rpc()
    {
        playerInfoManager.GetPlayer_Water();
    }
    [ClientRPC]
    private void ForceRequestInfoItems_Rpc()
    {
        playerInfoManager.GetPlayer_InventoryItems();
    }
    [ClientRPC]
    private void ForceRequestInfoArmor_Rpc()
    {
        playerInfoManager.GetPlayer_InventoryArmor();
    }
    [ClientRPC]
    private void ForceRequestInfoBlueprints_Rpc()
    {
        playerInfoManager.GetPlayer_InventoryBlueprints();
    }



    private IEnumerator AutoSaveLoop()
    {
        int interval = storedProperties.autoSaveInterval;
        if (interval == 0)
        {
            interval = 5;
        }
        yield return new WaitForSeconds(interval * 60f);
        List<PlayerInfo> playerInfoTemp = activePlayers.ToList();
        foreach (PlayerInfo player in inactivePlayers)
        {
            playerInfoTemp.Add(player);
        }
        serverSaveData.playerData = playerInfoTemp;
        serverSaveData.objData = activeGameObjects;
        int playerCount = 0;
        int objCount = 0;
        if (serverSaveData.playerData != null)
        {
            playerCount = serverSaveData.playerData.Count;
        }
        if (serverSaveData.objData != null)
        {
            playerCount = serverSaveData.objData.Count;
        }
        DebugMessage("Auto-Save Complete. " + playerCount + " PlayerInfo's Saved. " + objCount + " Objects Saved.", 1);
        StartCoroutine(AutoSaveLoop());
    }


    private void DebugMessage(string message, int level)
    {
        if (level <= logLevel)
        {
            if (IsServer)
            {
                Debug.Log("[Server] GameServer : " + message);
            }
            else
            {
                Debug.Log("[Client] GameServer : " + message);
            }
        }
    }
}




//Player Info Object
public class PlayerInfo
{
    public string name;
    public string authKey;
    public int id;
    public int health;
    public int food;
    public int water;
    public Vector3 location;
    public ulong networkId;
    public Item[] items;
    public Item[] armor;
    public int[] blueprints;
    public ulong clientId;
}

//1156 6/20/20