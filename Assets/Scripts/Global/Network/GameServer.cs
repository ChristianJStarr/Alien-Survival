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
    public bool logActions = true;
    void Awake()
    {
        singleton = this;
        DontDestroyOnLoad(gameObject);
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
    }

    #endregion



    //Player lists
    public List<PlayerInfo> activePlayers;
    public List<PlayerInfo> inactivePlayers;

    //Object lists
    private List<GameObject> activeGameObjects;
    private List<Resource> activeGameResources;

    //Systems
    private PlayerInfoManager playerInfoManager;
    private ServerSaveData serverSaveData;
    private InventorySystem inventorySystem;
    private ClickableSystem clickableSystem;
    //Item Datas
    private ItemData[] allItems;


    private void Start()
    {
        
        if (NetworkingManager.Singleton.IsServer)
        {
            StartGameServer();
        }
        

    }

    //-----------------------------------------------------------------//
    //             SERVER FUNCTIONS                                    //
    //-----------------------------------------------------------------//

    private void StartGameServer()
    {
        Debug.Log("Network - Server - Starting Game Server");
        activePlayers = new List<PlayerInfo>();
        inactivePlayers = new List<PlayerInfo>();
        allItems = Resources.LoadAll("Items", typeof(ItemData)).Cast<ItemData>().ToArray();
        serverSaveData = Resources.Load("Data/ServerSaveData") as ServerSaveData;
        if (serverSaveData.playerData != null)
        {
            inactivePlayers = serverSaveData.playerData;
            activeGameObjects = serverSaveData.objData;
        }
        activeGameResources = FindObjectsOfType<Resource>().ToList();
        playerInfoManager = PlayerInfoManager.singleton;
        inventorySystem = GetComponent<InventorySystem>();
        clickableSystem = GetComponent<ClickableSystem>();
        LoadGameObjects();
    }
    public void StopGameServer()
    {
        inactivePlayers.Concat(activePlayers).ToList();
        if (serverSaveData != null)
        {
            serverSaveData.playerData = inactivePlayers;
            serverSaveData.objData = activeGameObjects;
            NetworkingManager.Singleton.StopServer();
        }
    }
    public void ActiveManage(PlayerInfo info, bool add)
    {
        if (add)
        {
            activePlayers.Add(info);
        }
        else
        {
            activePlayers.Remove(info);
        }
    }
    public void InactiveManage(PlayerInfo info, bool add)
    {
        if (add)
        {
            inactivePlayers.Add(info);
        }
        else
        {
            inactivePlayers.Remove(info);
        }
    }

    //-----------------------------------------------------------------//
    //             CLIENT CALLBACKS                                    //
    //-----------------------------------------------------------------//

    public void PlayerConnected_Player(ulong networkId)
    {
        Debug.Log("Network - Game - Connected to Server.");
        InvokeServerRpc(HandoverNetworkId, PlayerPrefs.GetInt("id"), PlayerPrefs.GetString("authKey"), networkId);
    }
    [ServerRPC(RequireOwnership = false)]
    private void HandoverNetworkId(int id, string authKey, ulong networkId)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                activePlayers[i].networkId = networkId;
                break;
            }
        }
    }
    public void PlayerDisconnected_Player(ulong obj)
    {

    }

    //-----------------------------------------------------------------//
    //             SERVER SIDE TOOLS                                   //
    //-----------------------------------------------------------------//

    private PlayerInfo GetActivePlayerByAuth(int id, string authKey)
    {
        PlayerInfo info = null;
        foreach (PlayerInfo player in activePlayers.ToList())
        {
            if (player.id == id)
            {
                if (player.authKey == authKey)
                {
                    activePlayers.Remove(player);
                    info = player;
                    break;
                }
            }
        }
        return info;
    }
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
            else
            {
                if (logActions)
                {
                    Debug.Log("Server - TOOL - Unable to Move Player To Inactive: " + clientId);
                }
            }
        }
        
    }
    private ItemData GetItemDataById(int id) 
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
    
    //-----------------------------------------------------------------//
    //             Server Action : Change Player Info                  //
    //-----------------------------------------------------------------//

    //Health
    public void ServerSetHealth(ulong clientId, int amount) 
    {
        if (logActions) 
        {
            Debug.Log("Server - Action - Set Health of Client: " + clientId);
        }
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if(activePlayers[i].clientId == clientId) 
            {
                activePlayers[i].health += amount;
                ForceRequestInfoById(clientId, 2);
                break;
            }
            else 
            {
                if (logActions)
                {
                    Debug.Log("Server - Action - Unable to Set Health of Client: " + clientId);
                }
            }
        }
    }

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

    //Inventory Items Add by ID
    public void ServerAddNewItemToInventory(ulong clientId, int id, int amount) 
    {
        if (logActions)
        {
            Debug.Log("Server - Action - Add Item to Inventory of Client: " + clientId);
        }
        ItemData itemData = GetItemDataById(id);
        if(itemData != null) 
        {
            while (amount > 0)
            {
                if(amount > itemData.maxItemStack) 
                {
                    amount -= itemData.maxItemStack;
                    ServerAddItemToInventory(clientId, CreateItemFromData(itemData, id, itemData.maxItemStack));
                }
                else 
                {
                    ServerAddItemToInventory(clientId, CreateItemFromData(itemData, id, amount));
                    break;
                }
            }
        }
        else 
        {
            //ItemData returned null
        }
    }
    
    //Inventory Items Add by Item
    public void ServerAddItemToInventory(ulong clientId, Item item)
    {
        if (logActions)
        {
            Debug.Log("Server - Action - Added Item to Inventory of Client: " + clientId);
        }
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].clientId == clientId)
            {
                activePlayers[i].items = inventorySystem.AddItemToInventory(item, activePlayers[i].items);
                ForceRequestInfoById(clientId, 5);
                break;
            }
            else
            {
                if (logActions)
                {
                    Debug.Log("Server - Action - Unable to Add Item to Inventory of Client: " + clientId);
                }
            }
        }
    }
    //Inventory Items Add by Item
    public void ServerCraftItemToInventory(ulong clientId, ItemData item, int amount)
    {
        if (logActions)
        {
            Debug.Log("Server - Action - Add Item to Inventory of Client: " + clientId);
        }
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
                            newInventory = inventorySystem.AddItemToInventory(CreateItemFromData(item, item.itemID, item.maxItemStack), newInventory);
                        }
                        else
                        {
                            newInventory = inventorySystem.AddItemToInventory(CreateItemFromData(item, item.itemID, amount), newInventory);
                            break;
                        }
                    }
                    activePlayers[i].items = inventorySystem.RemoveItemsByRecipe(item.recipe, amount, newInventory, allItems);
                    ForceRequestInfoById(clientId, 5);
                    break;
                }
                else
                {
                    if (logActions)
                    {
                        Debug.Log("Server - Action - Unable to Add Item to Inventory of Client: " + clientId);
                    }
                }
            }
        }
        else
        {
            //ItemData returned null
        }
    }
    
   
    
    
    //Inventory Items Remove
    public void ServerRemoveItemFromInventory(ulong clientId, int itemId, int amount)
    {
        if (logActions)
        {
            Debug.Log("Server - Action - Removed Item from Inventory of Client: " + clientId);
        }
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
                if (logActions)
                {
                    Debug.Log("Server - Action - Unable to Add Item to Inventory of Client: " + clientId);
                }
            }
        }
    }
    
    //Inventory Blueprints Add
    public void ServerAddBlueprint(ulong clientId, Item newBp)
    {
        if (logActions)
        {
            Debug.Log("Server - Action - Added Blueprint to Inventory of Client: " + clientId);
        }
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
                if (logActions)
                {
                    Debug.Log("Server - Action - Unable to Add Blueprint to Inventory of Client: " + clientId);
                }
            }
        }
    }
    
    //Inventory Blueprints Whipe
    public void ServerWipeBlueprints(ulong clientId)
    {
        if (logActions)
        {
            Debug.Log("Server - Action - Wiped the Blueprints of Client: " + clientId);
        }
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
                if (logActions)
                {
                    Debug.Log("Server - Action - Unable to Wipe Blueprints of Client: " + clientId);
                }
            }
        }
    }


    //-----------------------------------------------------------------//
    //             Server Action : Handle GameObjs                     //
    //-----------------------------------------------------------------//

    private void LoadGameObjects() 
    {
        if(activeGameObjects != null) 
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

    private void DepleteResource(ulong clientId, GameObject obj) 
    {
        Resource resource = obj.GetComponent<Resource>();
        if (resource != null) 
        {
            int amount = resource.gatherAmount;
            int gather = resource.gatherPerAmount;

            if (amount - gather <= 0)
            {
                
            }
            else 
            {
            //    resource.gatherAmount -= gather;
            //    serverInventoryManager.AddItem(resource.gatherItem.itemID, gather);
            }

        }
    }

    //-----------------------------------------------------------------//
    //             Player Request : Retrieve Player Info               //
    //-----------------------------------------------------------------//

    //--Health
    public void GetPlayerHealth(int id, Action<int> callback) 
    {
        StartCoroutine(GetPlayerHealth_Wait(id, returnValue =>{callback(returnValue);}));
    }
    private IEnumerator GetPlayerHealth_Wait(int id, Action<int> callback) 
    {
        RpcResponse<int> response = InvokeServerRpc(GetPlayerHealth_Rpc, id);
        while (!response.IsDone){yield return null;}
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private int GetPlayerHealth_Rpc(int id)
    {
        int value = 0;
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                value = player.health;
                break;
            }
        }
        return value;
    }
    //--Water
    public void GetPlayerWater(int id, Action<int> callback)
    {
        StartCoroutine(GetPlayerWater_Wait(id, returnValue => { callback(returnValue); }));
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
        int value = 0;
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                value = player.water;
                break;
            }
        }
        return value;
    }
    //--Food
    public void GetPlayerFood(int id, Action<int> callback)
    {
        StartCoroutine(GetPlayerFood_Wait(id, returnValue => { callback(returnValue); }));
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
        int value = 0;
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                value = player.food;
                break;
            }
        }
        return value;
    }
    //--Inventory items
    public void GetPlayerInventoryItems(int id, Action<Item[]> callback)
    {
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
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                if (player.items != null) 
                {   
                    value = player.items;
                }
                break;
            }
        }
        return value;
    }
    //--Inventory armor
    public void GetPlayerInventoryArmor(int id, Action<Item[]> callback)
    {
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
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                if(player.armor != null) 
                {
                    value = player.armor;
                }
                break;
            }
        }
        return value;
    }
    //--Inventory blueprints
    public void GetPlayerInventoryBlueprints(int id, Action<int[]> callback)
    {
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
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                if (player.blueprints != null)
                {
                    value = player.blueprints;
                }
                break;
            }
        }
        return value;
    }


    //-----------------------------------------------------------------//
    //             Player Request : Modify Own Values                  //
    //-----------------------------------------------------------------//

    //--Set Player Health
    public void SetPlayerHealth(int id, string authKey, int health) 
    {
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
                ForceRequestInfoById(activePlayers[i].clientId, 2);
                break;
            }
        }
    }
    //--Set Player Water
    public void SetPlayerWater(int id, string authKey, int water)
    {
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
                ForceRequestInfoById(activePlayers[i].clientId, 2);
                break;
            }
        }
    }
    //--Set Player Food
    public void SetPlayerFood(int id, string authKey, int food)
    {
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
                ForceRequestInfoById(activePlayers[i].clientId, 2);
                break;
            }
        }
    }
    //--Set Player Location
    public void SetPlayerLocation(int id, string authKey, Vector3 location)
    {
        InvokeServerRpc(SetPlayerLocation_Rpc, id, authKey, location);
    }
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerLocation_Rpc(int id, string authKey, Vector3 location)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (activePlayers[i].id == id && activePlayers[i].authKey == authKey)
            {
                activePlayers[i].location = location;
                ForceRequestInfoById(activePlayers[i].clientId, 2);
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
        if(curSlot > 33 || newSlot > 33) 
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
                ForceRequestInfoById(activePlayers[i].clientId, 5);
                break;
            }
        }
    }
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
                        Debug.Log("Instantiate Item ID: " + returnValue.itemID);
                    }
                });
                ForceRequestInfoById(activePlayers[i].clientId, 5);
                break;
            }
        }       
    }
    //--Remove Player Craft Item by ID
    public void CraftItemById(int id, string authKey, int itemId, int amount)
    {
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

                    if(amount * item.craftAmount > item.maxItemStack) 
                    {
                        while (amount > 0)
                        {
                            if (amount > item.maxItemStack)
                            {
                                amount -= item.maxItemStack;
                                newInventory = inventorySystem.AddItemToInventory(CreateItemFromData(item, item.itemID, item.maxItemStack), newInventory);
                            }
                            else
                            {
                                newInventory = inventorySystem.AddItemToInventory(CreateItemFromData(item, item.itemID, amount * item.craftAmount), newInventory);
                                break;
                            }
                        }
                    }
                    else 
                    {
                        newInventory = inventorySystem.AddItemToInventory(CreateItemFromData(item, item.itemID, amount * item.craftAmount), newInventory);
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

    //public void PlayerRayCastHit(NetworkedObject player, int type) 
    //{
    //    InvokeServerRpc(PlayerRayCastHitRpc, player, type);
    //}
    //[ServerRPC(RequireOwnership = false)]
    //private void PlayerRayCastHitRpc(NetworkedObject player, int type) 
    //{

    //    float distance = 1F;
    //    int damage = 1;
    //    if(type == 1) { distance = 2f; }
    //    if (type == 2) { distance = 8f; }
    //    if (type == 3) { distance = 30f; }
    //    if (type == 4) { distance = 50f; }

    //    Transform playerTransform = player.GetComponent<Transform>();
    //    RaycastHit hit;
    //    LagCompensationManager.Simulate(player.NetworkId, () => 
    //    {
    //        if (Physics.Raycast(playerTransform.position, -Vector3.up, out hit, distance))
    //        {
    //            GameObject hitObject = hit.collider.gameObject;
    //            Vector3 hitPosition = hitObject.transform.position;
    //            string tag = hitObject.tag;

    //            if (tag == "Player") 
    //            {
    //                ulong playerId = hitObject.GetComponent<NetworkedObject>().NetworkId;
    //                PlayerInfo playerInfo = GetActivePlayerById(playerId);
    //                int health = playerInfo.health;
    //                if(health - damage <= 0) 
    //                {
    //                    //Kill Player
    //                }
    //                else 
    //                {
    //                    health -= damage;
    //                    ForceRequestInfoById(playerId, 2);
    //                }
    //            }
    //            if (tag == "Enemy")
    //            {

    //            }
    //            if (tag == "Friendly")
    //            {

    //            }
    //            if (tag == "Tree")
    //            {

    //            }

    //        }
    //    });
    //}


    //-----------------------------------------------------------------//
    //         Player Request : Inventory Items Modification           //
    //-----------------------------------------------------------------//
    //--Move Player Inventory Item by Slot
    public void InteractWithClickable(int id, string authKey, string uniqueId)
    {
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
                break;
            }
        }
    }



    //-----------------------------------------------------------------//
    //             Client RPC : Force Request                          //
    //-----------------------------------------------------------------//
    private void ForceRequestInfoById(ulong clientId, int infoDepth=1) 
    {
        List<ulong> idList = new List<ulong>();
        idList.Add(clientId);

        if(infoDepth == 1) 
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
        PlayerInfoManager.singleton.GetPlayer_AllInfo();
    }
    [ClientRPC]
    private void ForceRequestInfoHealth_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_Health();
    }
    [ClientRPC]
    private void ForceRequestInfoFood_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_Food();
    }
    [ClientRPC]
    private void ForceRequestInfoWater_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_Water();
    }
    [ClientRPC]
    private void ForceRequestInfoItems_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_InventoryItems();
    }
    [ClientRPC]
    private void ForceRequestInfoArmor_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_InventoryArmor();
    }
    [ClientRPC]
    private void ForceRequestInfoBlueprints_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_InventoryBlueprints();
    }

}





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

