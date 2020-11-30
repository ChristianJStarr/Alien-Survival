using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayerInfoSystem : MonoBehaviour
{
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
            inactive.Add(active[clientId]);
            returnedInfo = active[clientId];
            returnedInfo.hoursAdd = (float)DateTime.Now.Subtract(returnedInfo.time).TotalMinutes / 60;
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

    //Player Items
    public Item[] GetPlayerItems(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].items;
        }
        return null;
    }
    public void SetPlayerItems(ulong clientId, Item[] items)
    {
        if (active.ContainsKey(clientId))
        {
            active[clientId].items = items;
            ForceRequestInfoById(clientId, 5);
        }
    }

    //Player Armor
    public Item[] GetPlayerArmor(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].armor;
        }
        return null;
    }
    public void SetPlayerArmor(ulong clientId, Item[] armor)
    {
        if (active.ContainsKey(clientId))
        {
            active[clientId].armor = armor;

            ForceRequestInfoById(clientId, 6);
        }
    }

    //Player Blueprints
    public int[] GetPlayerBlueprints(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].blueprints;
        }
        return null;
    }
    public void SetPlayerBlueprints(ulong clientId, int itemId, bool add = true)
    {
        if (active.ContainsKey(clientId))
        {
            if (active[clientId].blueprints.Length > 0)
            {
                List<int> tempBp = active[clientId].blueprints.ToList();

                if (add)
                {
                    if (!tempBp.Contains(itemId))
                    {
                        tempBp.Add(itemId);
                    }
                }
                else
                {
                    if (tempBp.Contains(itemId))
                    {
                        tempBp.Remove(itemId);
                    }
                }
                active[clientId].blueprints = tempBp.ToArray();
                ForceRequestInfoById(clientId, 7);
            }
            else
            {
                if (add)
                {
                    active[clientId].blueprints = new int[] { itemId };
                    ForceRequestInfoById(clientId, 7);
                }
            }
        }
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
                gameServer.Server_PlayerDeath(clientId, info.items, info.armor, info.username);
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
    public void SetPlayerNetworkId(ulong clientId, int id, string authKey, ulong networkId)
    {
        if (active.ContainsKey(clientId))
        {
            if (Confirm(clientId, authKey))
            {
                active[clientId].networkId = networkId;
            }
        }
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
            DateTime lastRespawn = active[clientId].time;
            TimeSpan span = DateTime.Now - lastRespawn;
            active[clientId].time = DateTime.Now;
            active[clientId].hoursAdd += (float)span.TotalHours;
            return span;
        }
        return TimeSpan.FromSeconds(1);
    }


    //-----------------------------------------------------------------//
    //         INVENTORY MODIFICATION | InventorySortTool              //
    //-----------------------------------------------------------------//

    //Clear Player Items
    public void Inventory_Clear(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            active[clientId].items = null;
            active[clientId].armor = null;

            ForceRequestInfoById(clientId, 5);
            ForceRequestInfoById(clientId, 6);
        }
    }

    //Add New Item
    public void Inventory_AddNew(ulong clientId, int itemId, int amount, Action<bool> callback) 
    {
        if (Confirm(clientId))
        {
            ItemData itemData = ItemDataManager.Singleton.GetItemData(itemId);
            if (itemData != null)
            {
                while (amount > 0)
                {
                    if (amount > itemData.maxItemStack)
                    {
                        amount -= itemData.maxItemStack;
                        Inventory_Add(clientId, sit.CreateItemFromData(itemData, itemData.maxItemStack));
                    }
                    else
                    {
                        Inventory_Add(clientId, sit.CreateItemFromData(itemData, amount), returnValue => { callback(returnValue); });

                        break;
                    }
                }
            }
        }
        
    }

    //Add Item
    public void Inventory_Add(ulong clientId, Item item, Action<bool> success = null) 
    {
        if (Confirm(clientId)) 
        {
            item.currSlot = 44;
            active[clientId].items = sit.AddItemToInventory(item, active[clientId].items, returnValue => { success(returnValue); });
            ForceRequestInfoById(clientId, 5);
        }
    }

    //Move Item
    public void Inventory_MoveItem(ulong clientId, string authKey, int oldSlot, int newSlot)
    {
        if (Confirm(clientId, authKey))
        {
            Item[] inventory = GetPlayerItems(clientId);
            Item newItem = sit.GetItemBySlot(inventory, newSlot);
            Item oldItem = sit.GetItemBySlot(inventory, oldSlot);
            if (newItem != null && oldItem != null)
            {
                ItemData newItemData = ItemDataManager.Singleton.GetItemData(newItem.itemID);
                //Check if Item can add Durability
                if(newItemData.maxDurability != 0) 
                {
                    if (newItem.durability + oldItem.itemStack <= newItemData.maxDurability && newItemData.durabilityRefilId == oldItem.itemID)
                    {
                        Item[] temp = sit.RemoveItemFromInventoryBySlot(oldSlot, inventory, callback => { });
                        inventory = sit.ChangeItemDurability(temp, oldItem.itemStack, newItemData.maxDurability, newSlot);

                        if (inventory != null)
                        {
                            active[clientId].items = inventory;
                            ForceRequestInfoById(clientId, 5);
                        }
                    }
                    else if (newItemData.durabilityRefilId == oldItem.itemID)
                    {
                        Item[] temp = sit.RemoveItemFromInventoryBySlot(oldSlot, inventory, callback => { }, newItemData.maxDurability - newItem.durability);
                        inventory = sit.ChangeItemDurability(temp, oldItem.itemStack, newItemData.maxDurability, newSlot);
                        if (inventory != null)
                        {
                            active[clientId].items = inventory;
                            ForceRequestInfoById(clientId, 5);
                        }
                    }
                    else 
                    {
                        active[clientId].items = sit.MoveItemInInventory(oldSlot, newSlot, inventory);
                        ForceRequestInfoById(clientId, 5);
                    }
                }
                else 
                {
                    active[clientId].items = sit.MoveItemInInventory(oldSlot, newSlot, inventory);
                    ForceRequestInfoById(clientId, 5);
                }


                
            }
            else if(oldItem != null) 
            {
                active[clientId].items = sit.MoveItemInInventory(oldSlot, newSlot, inventory);
                ForceRequestInfoById(clientId, 5);
            }
        }
    }

    //Remove Item
    public void Inventory_RemoveItem(ulong clientId, string authKey, int slot, Action<Item> droppedItem)
    {
        if (Confirm(clientId, authKey))
        {
            active[clientId].items = sit.RemoveItemFromInventoryBySlot(slot, active[clientId].items, returnValue => { droppedItem(returnValue); });
            ForceRequestInfoById(clientId, 5);
        }
    }

    //Craft Item
    public void Inventory_CraftItem(ulong clientId, string authKey, int itemId, int amount)
    {
        if (Confirm(clientId, authKey))
        {
            active[clientId].items = sit.CraftItem(active[clientId].items, itemId, amount);
            ForceRequestInfoById(clientId, 5);
        }
    }

    //Get Item From Slot
    public Item Inventory_GetItemFromSlot(ulong clientId, int itemSlot)
    {
        if (Confirm(clientId))
        {
            Item[] inventory = active[clientId].items;
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].currSlot == itemSlot)
                {
                    return inventory[i];
                }
            }
        }
        return null;
    }

    //Change Item Durability
    public bool Inventory_ChangeItemDurability(ulong clientId, int amount, int maxDurability, int slot)
    {
        bool wasTaken = false;
        if (Confirm(clientId)) 
        {
            Item[] items = sit.ChangeItemDurability(active[clientId].items, amount, maxDurability, slot);
            if (items != null)
            {
                wasTaken = true;
                active[clientId].items = items;
                ForceRequestInfoById(clientId, 5);
            }
        }
        return wasTaken;
    }

    //Reload to Durability
    public void Inventory_ReloadToDurability(ulong clientId, string authKey, int slot) 
    {
        if(Confirm(clientId, authKey)) 
        {
            Item item = Inventory_GetItemFromSlot(clientId, slot);
            if (item != null)
            {
                ItemData data = ItemDataManager.Singleton.GetItemData(item.itemID);
                if (item.durability < data.maxDurability)
                {
                    int needed = data.maxDurability - item.durability;
                    int available = sit.GetMaxAvailableInventory(data.durabilityId, active[clientId].items);
                    int final = 0;
                    if (available <= needed)
                    {
                        final = available;
                    }
                    else if (available > needed)
                    {
                        final = needed;
                    }
                    Item[] inventory = sit.RemoveItemFromInventory(data.durabilityId, final, active[clientId].items);
                    if (inventory != null)
                    {
                        inventory = sit.ChangeItemDurability(inventory, final, data.maxDurability, slot);
                        if (inventory != null)
                        {
                            DebugMsg.Notify("Reloading for Player: " + clientId, 2);
                            active[clientId].items = inventory;
                            ForceRequestInfoById(clientId, 5);
                        }
                        else
                        {
                            DebugMsg.Notify("Reloading for Player Failed: " + clientId, 3);
                        }
                    }
                    else
                    {
                        DebugMsg.Notify("Reloading for Player Failed: " + clientId, 3);
                    }
                }
                else
                {
                    DebugMsg.Notify("Reloading for Player Failed: " + clientId, 3);
                }
            }
        }
    }

    //Split Item from Slot
    public void Inventory_SplitItem(ulong clientId, string authKey, int slot, int amount) 
    {
        if(Confirm(clientId, authKey)) 
        {
            active[clientId].items = sit.SplitItemStackById(active[clientId].items, slot, amount);
        }
    }


    //-----------------------------------------------------------------//
    //                           TOOLS .. TOOLS                        //
    //-----------------------------------------------------------------//

    //Confirm Player ID and AuthKey. Rerturns BOOL (Check Config)
    public bool Confirm(ulong clientId, string authKey = "")
    {
        if (systemEnabled) 
        {
            if (!confirmNetworkKey)
            {
                if (active.ContainsKey(clientId))
                {
                    return true;
                }
                DebugMsg.Notify("Client '" + clientId + "' Does Not Exist!", 1);
                return false;
            }
            if (active.ContainsKey(clientId))
            {
                if (authKey == "" || active[clientId].authKey == authKey)
                {
                    return true;
                }
                DebugMsg.Notify("Client '" + clientId + "' Failed Authentication!", 1);
                return false;
            }
        }
        return false;
    }

    //Force Request Info
    private void ForceRequestInfoById(ulong clientId, int depth = 1)
    {
        //------DEPTH KEY------//
        //   1 - ALL           //
        //   2 - HEALTH        //
        //   3 - FOOD          //
        //   4 - WATER         //
        //   5 - ITEMS         //
        //   6 - ARMOR         //
        //   7 - BLUEPRINTS    //
        //   8 - H/F/W         //
        gameServer.ForceRequestInfoById(clientId, depth);
    }


    //-----------------------------------------------------------------//
    //                           SYSTEM LOOPS                          //
    //-----------------------------------------------------------------//

    //Resource Depletion Loop
    private IEnumerator ResourceDepletionLoop() 
    {
        while (systemEnabled) 
        {
            yield return new WaitForSeconds(15);
            for (int i = 0; i < activeIds.Count; i++)
            {
                ulong clientId = activeIds[i];
                PlayerInfo info = active[clientId];
                if (!info.isDead)
                {
                    bool foodChanged = false;
                    bool waterChanged = false;
                    bool healthChanged = false;

                    //Deplete Food
                    if (info.food > 0)
                    {
                        active[clientId].food -= 1;
                        foodChanged = true;
                    }
                    //Deplete Water
                    if (info.water > 0)
                    {
                        active[clientId].water -= 2;
                        waterChanged = true;
                    }
                    //Deplete or Increase Health
                    if (info.water <= 0 && info.food <= 0)
                    {
                        if (info.health > 5)
                        {
                            active[clientId].health -= 5;
                            healthChanged = true;
                        }
                        else
                        {
                            active[clientId].health = 0;
                            active[clientId].isDead = true;
                            healthChanged = true;
                            gameServer.Server_PlayerDeath(clientId, info.items, info.armor, info.username);
                        }
                    }
                    else if (info.food > 1 && info.water > 3 && info.health < 100)
                    {
                        active[clientId].food -= 2;
                        active[clientId].water -= 4;
                        foodChanged = true;
                        waterChanged = true;
                        if (info.health > 98)
                        {
                            active[clientId].health = 100;
                            healthChanged = true;
                        }
                        else
                        {
                            active[clientId].health += 2;
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
    [SerializeField]public string username = "X";
    [SerializeField] public string authKey = "X";
    [SerializeField] public int id = 0;
    [SerializeField] public ulong clientId = 0;
    [SerializeField] public ulong networkId = 0;

    //Local Stats
    [SerializeField] public int health = 0;
    [SerializeField] public int food = 0;
    [SerializeField] public int water = 0;

    //Player Arrays
    [SerializeField] public Item[] items = new Item[0];
    [SerializeField] public Item[] armor = new Item[0];
    [SerializeField] public int[] blueprints = new int[0];

    //Bool Checks
    [SerializeField] public bool isDead = false;

    //Acumulative Points
    [SerializeField] public int coinsAdd = 0;
    [SerializeField] public int expAdd = 0;
    [SerializeField] public float hoursAdd = 0;

    //Data for InfoChecks
    [SerializeField] public DateTime time;
    [SerializeField] public Vector3 location;
}