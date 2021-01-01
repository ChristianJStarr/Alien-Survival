using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayerInfoSystem : MonoBehaviour
{
#if (UNITY_SERVER || UNITY_EDITOR)
    private string savedPlayerInfoPath;
    private bool systemEnabled = false;
    private GameServer gameServer;
    private Dictionary<ulong, PlayerInfo> active = new Dictionary<ulong, PlayerInfo>();
    private List<ulong> activeIds = new List<ulong>();
    private List<PlayerInfo> inactive = new List<PlayerInfo>();
    public ServerInventoryTool sit;
    private bool confirmNetworkKey = true;


    public bool StartSystem()
    {
        systemEnabled = true;
        savedPlayerInfoPath = Application.dataPath + "/inventory-data.alien";
        LoadSaveFile();
        //Read temp list from json file.
        //return true if successfull

        StartCoroutine(ResourceDepletionLoop());

        gameServer = GameServer.singleton;
        return true;
    }
    public void StopSystem()
    {
        SaveAllInfo();
        systemEnabled = false;
    }


    //-----------------------------------------------------------------//
    //               PLAYER DICTIONARY CONTROL                         //
    //-----------------------------------------------------------------//

    public bool CreatePlayer(PlayerInfo playerInfo)
    {
        foreach (PlayerInfo info in inactive)
        {
            if (info.id == playerInfo.id && info.authKey == playerInfo.authKey) 
            {
                return false;
            }
        }
        active.Add(playerInfo.clientId, playerInfo);
        activeIds.Add(playerInfo.clientId);
        return true;
    }
    public bool MovePlayerToActive(ulong clientId, int userId, string authKey)
    {
        for (int i = 0; i < inactive.Count; i++)
        {
            if (inactive[i].clientId == clientId && inactive[i].authKey == authKey && inactive[i].id == userId)
            {
                if (!active.ContainsKey(clientId))
                {
                    active.Add(clientId, inactive[i]);
                    active[clientId].time = DateTime.Now;
                    if (active[clientId].inventory.blueprints.Length == 0) 
                    {
                        active[clientId].inventory.blueprints = new int[5] {1,2,3,4,5}; 
                    }
                    inactive.RemoveAt(i);
                    activeIds.Add(clientId);
                    return true;
                }
                break;
            }
        }
        return false;
    }
    public PlayerInfo MovePlayerToInactive(ulong clientId)
    {
        PlayerInfo returnedInfo = null;
        if (active.ContainsKey(clientId))
        {
            
            returnedInfo = active[clientId];
            active[clientId].hoursAdd = 0;
            active[clientId].expAdd = 0;
            active[clientId].coinsAdd = 0;
            inactive.Add(active[clientId]);
            active.Remove(clientId);
            activeIds.Remove(clientId);
        }
        return returnedInfo;
    }
    public void SaveAllInfo() 
    {
        File.WriteAllText(savedPlayerInfoPath, JsonUtility.ToJson(new PackedPlayerInfo(){players = active.Values.ToArray()}));
    }
    public void LoadSaveFile() 
    {
        if (File.Exists(savedPlayerInfoPath))
        {
            PackedPlayerInfo packed = JsonUtility.FromJson<PackedPlayerInfo>(File.ReadAllText(savedPlayerInfoPath));
            if (packed != null && packed.players.Length > 0)
            {
                inactive = packed.players.ToList();
            }
        }
    }


    //-----------------------------------------------------------------//
    //         BASE-LEVEL PLAYERINFO MODIFICATION | GET & SET          //
    //-----------------------------------------------------------------//

    //Full PlayerInfo
    public PlayerInfo GetPlayerInfo(ulong clientId) 
    {
        if (Confirm(clientId)) 
        {
            return active[clientId];
        }
        return null;
    }

    //Player Name
    public string GetPlayerName(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].username;
        }
        return "";
    }

    //Player Is New
    public bool GetPlayerNew(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            if (active[clientId].isNew) 
            {
                active[clientId].isNew = false;
                return true;
            }
        }
        return false;
    }

    //Player Items
    public Inventory GetInventory(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].inventory;
        }
        return null;
    }

    //Player Blueprints
    public int[] GetPlayerBlueprints(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].inventory.blueprints;
        }
        return null;
    }
    
    //Player Health
    public int GetPlayerHealth(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].health;
        }
        return 0;
    }
    public void SetPlayerHealth(ulong clientId, int amount, bool subtract = true)
    {
        if (active.ContainsKey(clientId))
        {
            PlayerInfo info = active[clientId];
            if (subtract)
            {
                active[clientId].health += amount;
            }
            else
            {
                active[clientId].health = amount;
            }
            if(active[clientId].health == 0) 
            {
                active[clientId].isDead = true;
                gameServer.Server_PlayerDeath(clientId, info.inventory, info.username);
            }

            ForceRequestInfoById(clientId, 2);
        }
    }

    //Player Food
    public int GetPlayerFood(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].food;
        }
        return 0;
    }
    public void SetPlayerFood(ulong clientId, int amount, bool subtract = true)
    {
        if (active.ContainsKey(clientId))
        {
            if (subtract)
            {
                active[clientId].food += amount;
                ForceRequestInfoById(clientId, 3);
            }
            else
            {
                active[clientId].food = amount;
                ForceRequestInfoById(clientId, 3);
            }
        }
    }

    //Player Water
    public int GetPlayerWater(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].water;
        }
        return 0;
    }
    public void SetPlayerWater(ulong clientId, int amount, bool subtract = true)
    {
        if (active.ContainsKey(clientId))
        {
            if (subtract)
            {
                active[clientId].water += amount;
                ForceRequestInfoById(clientId, 4);
            }
            else
            {
                active[clientId].water = amount;
                ForceRequestInfoById(clientId, 4);
            }
        }
    }

    //Player Coins
    public int GetPlayerCoins(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].coinsAdd;
        }
        return 0;
    }
    public void AddPlayerCoins(ulong clientId, int amount)
    {
        if (active.ContainsKey(clientId))
        {
            active[clientId].coinsAdd += amount;
        }
    }

    //Player Hours
    public float GetPlayerHours(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].hoursAdd;
        }
        return 0;
    }
    public void AddPlayerHours(ulong clientId, int amount)
    {
        if (active.ContainsKey(clientId))
        {
            active[clientId].hoursAdd += amount;
        }
    }

    //Player Experience
    public int GetPlayerExp(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].expAdd;
        }
        return 0;
    }
    public void AddPlayerExp(ulong clientId, int amount)
    {
        if (active.ContainsKey(clientId))
        {
            active[clientId].expAdd += amount;
        }
    }

    //Player isDead
    public bool GetPlayerDead(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].isDead;
        }
        return false;
    }
    public void SetPlayerDead(ulong clientId, bool isDead)
    {
        if (active.ContainsKey(clientId))
        {
            active[clientId].isDead = isDead;
        }
    }

    //Player Location
    public Vector3 GetPlayerLocation(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].location;
        }
        return Vector3.zero;
    }
    public void SetPlayerLocation(ulong clientId, Vector3 position)
    {
        if (Confirm(clientId))
        {
            active[clientId].location = position;
        }
    }

    //Player NetworkID
    public void SetPlayerNetworkId(ulong clientId, ulong networkId)
    {
        if (Confirm(clientId))
        {
            active[clientId].networkId = networkId;
        }
    }

    //Get Drop Item (Duplicate)
    public Item Inventory_GetDropItem(ulong clientId, int slot) 
    {
        if (Confirm(clientId)) 
        {
            return active[clientId].inventory.GetDropDuplicate(slot);
        }
        return null;
    }
    
    //Get Item from Slot
    public Item Inventory_GetItemBySlot(ulong clientId, int slot) 
    {
        if (Confirm(clientId)) 
        {
            Item item = active[clientId].inventory.GetItemBySlot(slot);
            if(item != null) 
            {
                return item;
            }
        }
        return null;
    }

    //-----------------------------------------------------------------//
    //         BASE-LEVEL PLAYERINFO QUICK SET FUNCTIONS               //
    //-----------------------------------------------------------------//

    //Reset Info For Respawn
    public void ResetPlayerInfo(ulong clientId, Vector3 location) 
    {
        if (Confirm(clientId)) 
        {
            active[clientId].health = 100;
            active[clientId].food = 100;
            active[clientId].water = 100;
            active[clientId].location = location;
            active[clientId].isDead = false;
            ForceRequestInfoById(clientId);
        }
    }

    //Reset the Time Survived for Player (Called On Death) Adds to HoursAdd Stat
    public TimeSpan ResetTimeSurvived(ulong clientId) 
    {
        if (Confirm(clientId)) 
        {
            PlayerInfo player = active[clientId];
            DateTime lastRespawn = player.time;
            TimeSpan span = DateTime.Now - lastRespawn;
            player.time = DateTime.Now;
            player.hoursAdd += (float)span.TotalHours;
            player.deaths++;
            return span;
        }
        return TimeSpan.FromSeconds(1);
    }

    public void IncPlayerKills(ulong clientId) 
    {
        if (Confirm(clientId)) 
        {
            active[clientId].kills++;   
        }
    }


    //-----------------------------------------------------------------//
    //         INVENTORY MODIFICATION | InventorySortTool              //
    //-----------------------------------------------------------------//

    //Clear Player Items
    public void Inventory_Clear(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            active[clientId].inventory.Clear();
            ForceRequestInfoById(clientId, 5);
        }
    }

    //Add New Item
    public bool Inventory_AddNew(ulong clientId, int itemId, int amount) 
    {
        if (Confirm(clientId))
        {
            if (active[clientId].inventory.AddItems(itemId, amount)) 
            {
                ForceRequestInfoById(clientId, 5);
                return true;
            }
            return false;
        }
        return false;
    }

    //Move Item
    public void Inventory_MoveItem(ulong clientId, string authKey, int current, int future)
    {
        if (Confirm(clientId, authKey))
        {
            active[clientId].inventory.MoveItem(current, future);
            ForceRequestInfoById(clientId, 5);
        }
    }

    //Remove Item
    public bool Inventory_RemoveItem(ulong clientId, string authKey, int slot)
    {
        if (Confirm(clientId, authKey))
        { 
            if(active[clientId].inventory.RemoveItem(true, slot)) 
            {
                ForceRequestInfoById(clientId, 5);
                return true;
            }
        }
        return false;
    }

    //Craft Item
    public void Inventory_CraftItem(ulong clientId, string authKey, int itemId, int amount)
    {
        if (Confirm(clientId, authKey))
        {
            active[clientId].inventory.Craft(itemId, amount);
            ForceRequestInfoById(clientId, 5);
        }
    }

    //Change Item Durability
    public bool Inventory_ChangeItemDurability(ulong clientId, int slot, int amount)
    {
        if (Confirm(clientId)) 
        {
            if(active[clientId].inventory.Durability(slot, amount)) 
            {
                ForceRequestInfoById(clientId, 5);
                return true;
            }
        }
        return false;
    }

    //Reload to Durability
    public void Inventory_ReloadToDurability(ulong clientId, int slot) 
    {
        if(Confirm(clientId)) 
        {
            active[clientId].inventory.Reload(slot);
            ForceRequestInfoById(clientId, 5);
        }
    }

    //Split Item from Slot
    public void Inventory_SplitItem(ulong clientId, string authKey, int slot, int amount) 
    {
        if(Confirm(clientId, authKey)) 
        {
            active[clientId].inventory.SplitItem(slot, amount);
            ForceRequestInfoById(clientId, 5);
        }
    }


    //-----------------------------------------------------------------//
    //                           TOOLS .. TOOLS                        //
    //-----------------------------------------------------------------//

    //Confirm Player ID and AuthKey. Rerturns BOOL (Check Config)
    public bool Confirm(ulong clientId)
    {
        if (!systemEnabled) return false;
        if (active.ContainsKey(clientId)) return true;
        return false;
    }
    public bool Confirm(ulong clientId, string authKey)
    {
        if (!systemEnabled) return false;
        if (active.ContainsKey(clientId))
        {
            if (confirmNetworkKey)
            {
                if (active[clientId].authKey == authKey)
                {
                    return true;
                }
                else 
                {
                    Debug.Log("Client: " + clientId + " has submitted an invalid authentication key. User will be kicked, if continued");
                }
                return false;
            }
            return true;
        }
        return false;
    }

    //Force Request Info
    public void ForceRequestInfoById(ulong clientId, int depth = 1)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(depth);
                if (depth == 1) //All
                {
                    PlayerInfo player = GetPlayerInfo(clientId);
                    if (player != null)
                    {
                        writer.WriteInt32Packed(player.health);
                        writer.WriteInt32Packed(player.food);
                        writer.WriteInt32Packed(player.water);

                        //INVENTORY ITEMS
                        int inventoryLength = player.inventory.items.Count;
                        if (inventoryLength > 0)
                        {
                            writer.WriteInt32Packed(inventoryLength);
                            for (int i = 0; i < inventoryLength; i++)
                            {
                                if (player.inventory.items[i] != null)
                                {
                                    Item instance = player.inventory.items[i];
                                    writer.WriteInt32Packed(instance.itemId);
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

                        //BLUEPRINTS
                        writer.WriteIntArrayPacked(player.inventory.blueprints);
                    }
                    gameServer.ServerSendPlayerInfo(clientId, writeStream);
                }
                else if (depth == 2) //Health
                {
                    writer.WriteInt32Packed(GetPlayerHealth(clientId));
                    gameServer.ServerSendPlayerInfo(clientId, writeStream);
                }
                else if (depth == 3) //Food
                {
                    writer.WriteInt32Packed(GetPlayerFood(clientId));
                    gameServer.ServerSendPlayerInfo(clientId, writeStream);
                }
                else if (depth == 4) //Water
                {
                    writer.WriteInt32Packed(GetPlayerWater(clientId));
                    gameServer.ServerSendPlayerInfo(clientId, writeStream);
                }
                else if (depth == 5) //Items
                {
                    //INVENTORY ITEMS
                    Inventory inventory = GetInventory(clientId);
                    int inventoryLength = inventory.items.Count;
                    if (inventoryLength > 0)
                    {
                        writer.WriteInt32Packed(inventoryLength);
                        for (int i = 0; i < inventoryLength; i++)
                        {
                            if (inventory.items[i] != null)
                            {
                                Item instance = inventory.items[i];
                                writer.WriteInt32Packed(instance.itemId);
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
                    gameServer.ServerSendPlayerInfo(clientId, writeStream);
                }
                else if (depth == 6) //Armor
                {

                    //Depreciated
                }
                else if (depth == 7) //Blueprints
                {
                    writer.WriteIntArrayPacked(GetPlayerBlueprints(clientId));
                    gameServer.ServerSendPlayerInfo(clientId, writeStream);
                }
                else if (depth == 8) //Health / Food / Water
                {
                    PlayerInfo player = GetPlayerInfo(clientId);
                    if (player != null)
                    {
                        writer.WriteInt32Packed(player.health);
                        writer.WriteInt32Packed(player.food);
                        writer.WriteInt32Packed(player.water);
                    }
                    gameServer.ServerSendPlayerInfo(clientId, writeStream);
                }
            }
        }
    }


    //-----------------------------------------------------------------//
    //                           SYSTEM LOOPS                          //
    //-----------------------------------------------------------------//

    //Resource Depletion Loop
    private IEnumerator ResourceDepletionLoop() 
    {
        int interval = 15; //15
        int water_deplete = 2;//2 
        int food_deplete = 1; //1
        int water_damage = 3;//3
        int food_damge = 2;//2
        int heal_food = 2;
        int heal_water = 4;
        int heal_health = 2;
        while (systemEnabled) 
        {
            yield return new WaitForSeconds(interval);
            for (int i = 0; i < activeIds.Count; i++)
            {
                ulong clientId = activeIds[i];
                PlayerInfo info = active[clientId];
                if (!info.isDead && !info.isNew)
                {
                    bool foodChanged = false;
                    bool waterChanged = false;
                    bool healthChanged = false;
                    if (info.food > 0)
                    {
                        active[clientId].food -= food_deplete;
                        foodChanged = true;
                    }
                    if (info.water > 0)
                    {
                        active[clientId].water -= water_deplete;
                        waterChanged = true;
                    }
                    if(info.water <= 0) 
                    {
                        if (info.health > water_damage)
                        {
                            active[clientId].health -= water_damage;
                            healthChanged = true;
                        }
                        else
                        {
                            active[clientId].health = 0;
                            active[clientId].isDead = true;
                            healthChanged = true;
                            gameServer.Server_PlayerDeath(clientId, info.inventory, info.username);
                        }
                    }
                    if (info.food <= 0)
                    {
                        if (info.health > food_damge)
                        {
                            active[clientId].health -= food_damge;
                            healthChanged = true;
                        }
                        else
                        {
                            active[clientId].health = 0;
                            active[clientId].isDead = true;
                            healthChanged = true;
                            gameServer.Server_PlayerDeath(clientId, info.inventory, info.username);
                        }
                    }
                    else if (info.food > heal_food - 1 && info.water > heal_water - 1 && info.health < 100)
                    {
                        active[clientId].food -= heal_food;
                        active[clientId].water -= heal_water;
                        foodChanged = true;
                        waterChanged = true;
                        if (info.health > 100 - heal_health)
                        {
                            active[clientId].health = 100;
                            healthChanged = true;
                        }
                        else
                        {
                            active[clientId].health += heal_health;
                            healthChanged = true;
                        }
                    }

                    if ((foodChanged && waterChanged) || (waterChanged && healthChanged) || (foodChanged && healthChanged))
                    {
                        ForceRequestInfoById(clientId, 8);
                    }
                    else if (healthChanged)
                    {
                        ForceRequestInfoById(clientId, 2);
                    }
                    else if (foodChanged)
                    {
                        ForceRequestInfoById(clientId, 3);
                    }
                    else if (waterChanged)
                    {
                        ForceRequestInfoById(clientId, 4);
                    }
                }
            }
        }
    }
    
    //Auto-Save Called by Primary GameServer Loop
    public void AutoSave()
    {
        SaveAllInfo();
    }

#endif
}


[Serializable]
public class PackedPlayerInfo
{
    public PlayerInfo[] players;
}


//Player Info Object
[Serializable]
public class PlayerInfo
{
    //Authentication
    [SerializeField] public string username = "X";
    [SerializeField] public string authKey = "X";
    [SerializeField] public int id = 0;
    [SerializeField] public ulong clientId = 0;
    [SerializeField] public ulong networkId = 0;

    //Local Stats
    [SerializeField] public int health = 0;
    [SerializeField] public int food = 0;
    [SerializeField] public int water = 0;

    //New Inventory
    [SerializeField] public Inventory inventory;

    //Bool Checks
    [SerializeField] public bool isDead = false;
    [SerializeField] public bool isNew = false;

    //Acumulative Points
    [SerializeField] public int coinsAdd = 0;
    [SerializeField] public int expAdd = 0;
    [SerializeField] public float hoursAdd = 0;
    [SerializeField] public int kills = 0;
    [SerializeField] public int deaths = 0;

    //Data for InfoChecks
    [SerializeField] public DateTime time;
    [SerializeField] public Vector3 location;
}



[Serializable]
public class Inventory
{
    [SerializeField] public int[] blueprints = new int[0];
    [SerializeField] public List<Item> items = new List<Item>();

    //Clear Inventory
    public void Clear() 
    {
        items.Clear();
    }

    //Sort Inventory
    public void Sort()
    {
        //Gives Items a slot if they dont have one.
        List<Item> unassigned = new List<Item>();
        List<int> assigned = new List<int>();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].currSlot == 44 || items[i].currSlot == 0)
            {
                unassigned.Add(items[i]);
            }
            else
            {
                assigned.Add(items[i].currSlot);
            }
        }
        for (int i = 0; i < 33; i++) //33 Inventory Space
        {
            int slot = i + 1;
            if (!assigned.Contains(slot) && unassigned.Count > 0)
            {
                Item item = unassigned.First();
                item.currSlot = slot;
                assigned.Add(slot);
                unassigned.Remove(item);
            }
        }
    }

    //Add Item
    public bool AddItems(int itemId, int amount)
    {
        bool success = false;
        ItemData data = ItemDataManager.Singleton.GetItemData(itemId);
        if (data == null) return false;
        int maxItemStack = data.maxItemStack;
        if (amount > maxItemStack)
        {
            while (amount > 0)
            {
                if (amount > maxItemStack)
                {
                    if (AddItem(maxItemStack, CreateItem(data, maxItemStack)))
                    {
                        amount -= maxItemStack;
                    }
                }
                else
                {
                    if (AddItem(maxItemStack, CreateItem(data, amount)))
                    {
                        success = true;
                    }
                }
            }
        }
        else
        {
            if (AddItem(maxItemStack, CreateItem(data, amount)))
            {
                success = true;
            }
        }
        if (success) Sort();
        return success;
    }

    //Remove Item
    public bool RemoveItem(int itemId, int amount)
    {
        bool success = false;
        if (AvailableItems(itemId) < amount) return false;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId == itemId)
            {
                if (items[i].itemStack > amount) //More than enough items 
                {
                    items[i].itemStack -= amount;
                    amount = 0;
                }
                else if (items[i].itemStack == amount) //Just enough items
                {
                    items[i].itemStack = 0;
                    amount = 0;
                }
                else if (items[i].itemStack < amount) //Not enough items
                {
                    items[i].itemStack = 0;
                    amount -= items[i].itemStack;
                }
            }
        }
        if (amount == 0)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].itemStack == 0)
                {
                    items.RemoveAt(i);
                }
            }
        }
        return success;
    }
    public bool RemoveItem(bool bySlot, int slot, int amount = 0)
    {
        if (!bySlot) return false;
        Item item = GetItemBySlot(slot);
        if (amount != 0)
        {
            if (item != null)
            {
                if (item.itemStack > amount)
                {
                    item.itemStack -= amount;
                    return true;
                }
                else if (item.itemStack == amount)
                {
                    items.Remove(item);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        else
        {
            if (item != null)
            {
                items.Remove(item);
                return true;
            }
        }
        return false;
    }

    //Get Available 
    public int AvailableItems(int itemId)
    {
        int amount = 0;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId == itemId)
            {
                amount += items[i].itemStack;
            }
        }
        return amount;
    }

    //Move Item
    public void MoveItem(int current, int future)
    {
        Item current_item = GetItemBySlot(current);
        Item future_item = GetItemBySlot(future);

        if (current_item != null)
        {
            if (SlotBelongsToItems(current))
            {
                if (SlotBelongsToItems(future))
                {
                    //Items to Items
                    current_item.currSlot = future;
                    if (future_item != null)
                    {
                        bool swap = true;
                        ItemData armorData = ItemDataManager.Singleton.GetItemData(future_item.itemId);
                        if (armorData.maxDurability != 0)
                        {
                            if (future_item.durability + current_item.itemStack <= armorData.maxDurability && armorData.durabilityRefilId == current_item.itemId)
                            {
                                if(RemoveItem(true, current)) 
                                {
                                    Durability(future, current_item.itemStack);
                                    swap = false;
                                }
                            }
                            else if (armorData.durabilityRefilId == current_item.itemId)
                            {
                                if(RemoveItem(true, current, armorData.maxDurability - future_item.durability)) 
                                {
                                    Durability(future, armorData.maxDurability);
                                    swap = false;
                                }
                            }
                        }
                        if (swap) 
                        {
                            future_item.currSlot = current;
                        }
                    }
                }
                else if (SlotBelongsToArmor(future))
                {
                    //Items to Armor
                    ItemData itemData = ItemDataManager.Singleton.GetItemData(current_item.itemId);
                    if (itemData.isArmor)
                    {
                        if (itemData.armorType == GetArmorTypeFromSlot(future))
                        {
                            current_item.currSlot = future;
                        }
                        if (future_item != null)
                        {
                            ItemData armorData = ItemDataManager.Singleton.GetItemData(future_item.itemId);
                            if (itemData.armorType == armorData.armorType)
                            {
                                future_item.currSlot = current;
                            }
                        }
                    }
                }
            }
            else if (SlotBelongsToArmor(current))
            {
                if (SlotBelongsToItems(future))
                {
                    //Armor to Items
                    if (future_item == null)
                    {
                        current_item.currSlot = future;
                    }
                    else
                    {
                        ItemData futureData = ItemDataManager.Singleton.GetItemData(future_item.itemId);
                        if (futureData.isArmor && futureData.armorType == GetArmorTypeFromSlot(future))
                        {
                            current_item.currSlot = future;
                            future_item.currSlot = current;
                        }
                    }
                }
                else if (SlotBelongsToArmor(future))
                {
                    //Armor to Armor
                }
            }
        }
    }

    //Change Durability
    public bool Durability(int slot, int amount)
    {
        Item item = GetItemBySlot(slot);
        if (item != null)
        {
            ItemData data = ItemDataManager.Singleton.GetItemData(item.itemId);
            if (item.durability + amount > 0 && item.durability + amount <= data.maxDurability)
            {
                item.durability += amount;
                return true;
            }
            return false;
        }
        return false;
    }
    public bool Reload(int slot) 
    {
        Item item = GetItemBySlot(slot);
        if (item == null) return false;
        ItemData data = ItemDataManager.Singleton.GetItemData(item.itemId);
        if (item.durability < data.maxDurability)
        {
            int needed = data.maxDurability - item.durability;
            int available = AvailableItems(item.itemId);
            int final = 0;
            if (available <= needed)
            {
                final = available;
            }
            else if (available > needed)
            {
                final = needed;
            }
            if (RemoveItem(data.durabilityId, final))
            {
                if(Durability(slot, final)) 
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    //Split Item
    public void SplitItem(int slot, int amount)
    {
        Item item = GetItemBySlot(slot);
        if (item != null && item.itemStack > amount && items.Count < 33)
        {
            item.itemStack -= amount;
            items.Add(CreateItem(item, amount));
            Sort();
        }
    }


    //Blueprints
    public void Blueprints(int itemId)
    {
        int length = blueprints.Length;
        int[] temp = new int[length + 1];
        for (int i = 0; i < length + 1; i++)
        {
            if (i != length)
            {
                temp[i] = blueprints[i];
            }
            else
            {
                temp[i] = itemId;
            }
        }
    }
    public void ClearBlueprints()
    {
        blueprints = new int[0];
    }

    //Craft
    public void Craft(int itemId, int amount)
    {
        ItemData craftItem = ItemDataManager.Singleton.GetItemData(itemId);
        if (HasRecipe(itemId, craftItem.recipe, true) && AddItems(itemId, amount * craftItem.craftAmount))
        {
            for (int i = 0; i < craftItem.recipe.Length; i++)
            {
                string[] recipeData = craftItem.recipe[i].Split('-');
                int r_itemId = Convert.ToInt32(recipeData[0]);
                int r_amount = Convert.ToInt32(recipeData[1]);
                RemoveItem(r_itemId, r_amount);
            }
        }
    }

    //Drop Duplicate
    public Item GetDropDuplicate(int slot) 
    {
        Item item = GetItemBySlot(slot);
        if(item != null) 
        {
            return CreateItem(item, item.itemStack);
        }
        return null;
    }

    //Backends
    public Item GetItemBySlot(int slot)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].currSlot == slot)
            {
                return items[i];
            }
        }
        return null;
    }
    private Item CreateItem(ItemData data, int amount)
    {
        Item item = new Item()
        {
            itemId = data.itemId,
            itemStack = amount,
            currSlot = 44
        };
        if (data.startMaxDurability)
        {
            item.durability = data.maxDurability;
        }
        return item;
    }
    private Item CreateItem(Item item, int amount)
    {
        return new Item()
        {
            itemId = item.itemId,
            currSlot = 44,
            itemStack = amount,
        };
    }
    private bool AddItem(int maxItemStack, Item item)
    {
        if (item.itemStack <= 0) return false;
        int count = items.Count;
        if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                if (item.itemStack == 0) break;
                if (item.itemId == items[i].itemId)
                {
                    int stackRoom = maxItemStack - items[i].itemStack;
                    if (stackRoom > 0)
                    {
                        if (item.itemStack <= stackRoom)
                        {
                            items[i].itemStack += item.itemStack;
                        }
                        else
                        {
                            items[i].itemStack += stackRoom;
                            item.itemStack -= stackRoom;
                        }
                    }
                }
            }
            if (count < 33 && item.itemStack != 0)
            {
                item.currSlot = 44;
                items.Add(item);
                return true;
            }
            else if (item.itemStack == 0)
            {
                return true;
            }
        }
        else
        {
            item.currSlot = 44;
            items.Add(item);
            return true;
        }
        return false;
    }
    public bool HasRecipe(int itemId, string[] recipe, bool blueprint)
    {
        if (blueprint && !blueprints.Contains(itemId)) return false;
        int recipeAmount = recipe.Length;
        int recipeAvail = 0;
        foreach (string recipeData in recipe)
        {
            string[] data = recipeData.Split('-');
            int item = Convert.ToInt32(data[0]);
            int amount = Convert.ToInt32(data[1]);
            bool hasResources = false;

            foreach (InventoryResource resource in CalculateResources())
            {
                if (resource.itemId == item && resource.itemAmount >= amount)
                {
                    hasResources = true;
                    break;
                }
            }
            if (hasResources)
            {
                recipeAvail++;
            }
        }
        if (recipeAvail == recipeAmount)
        {
            return true;
        }
        return false;
    }
    public List<InventoryResource> CalculateResources()
    {
        Dictionary<int, InventoryResource> resources = new Dictionary<int, InventoryResource>();
        for (int i = 0; i < items.Count; i++)
        {
            if (resources.ContainsKey(items[i].itemId))
            {
                resources[items[i].itemId].itemAmount += items[i].itemStack;
            }
            else
            {
                resources.Add(items[i].itemId, new InventoryResource()
                {
                    itemId = items[i].itemId,
                    itemAmount = items[i].itemStack
                });
            }
        }
        return resources.Values.ToList();
    }



    //What Slots Are for What
    private bool SlotBelongsToItems(int slot)
    {
        if (slot > 0 && slot < 34)
        {
            return true;
        }
        return false;
    }
    private bool SlotBelongsToArmor(int slot)
    {
        if (slot > 33 && slot < 48)
        {
            return true;
        }
        return false;
    }
    private int GetArmorTypeFromSlot(int slot)
    {
        return 33 - slot;
    }

}