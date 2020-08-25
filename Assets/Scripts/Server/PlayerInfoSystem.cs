using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInfoSystem : MonoBehaviour
{
    public delegate void OnSystemStopped();
    public static event OnSystemStopped systemStopped;

    private bool systemEnabled = false;
    private GameServer gameServer;
    private Dictionary<ulong, PlayerInfo> active = new Dictionary<ulong, PlayerInfo>();
    private List<ulong> activeIds = new List<ulong>();
    private List<PlayerInfo> inactive = new List<PlayerInfo>();
    private ServerInventoryTool sit;
    //Configuration
    private bool confirmNetworkKey = true; // Authenticates ID and Authkey when updating player info. Increases CPU if TRUE


    private void Awake()
    {
        gameServer = GameServer.singleton;
    }



    //START SYSTEM
    public bool StartSystem()
    {
        systemEnabled = true;
        //Read temp list from json file.
        //return true if successfull
        StartCoroutine(ResourceDepletionLoop());

        return true;
    }

    public void StopSystem()
    {
        systemEnabled = false;
        SaveAllPlayerInfo();
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
    public void SaveAllPlayerInfo()
    {
        List<PlayerInfo> tempPlayerInfo = active.Values.ToList();
        for (int i = 0; i < inactive.Count; i++)
        {
            tempPlayerInfo.Add(inactive[i]);
        }

        //Save temp list to json file.
    }
    private void PlayerInfoHasChanged(ulong clientId, int depth = 0)
    {
        gameServer.ForceRequestInfoById(clientId, depth);
    }



    //-----------------------------------------------------------------//
    //         BASE-LEVEL PLAYERINFO MODIFICATION | GET & SET          //
    //-----------------------------------------------------------------//



    //Player Name
    public string GetPlayerName(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            return active[clientId].name;
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
            }
            else
            {
                if (add)
                {
                    active[clientId].blueprints = new int[] { itemId };
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
            if (subtract)
            {
                active[clientId].health += amount;
            }
            else
            {
                active[clientId].health = amount;
            }
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
            }
            else
            {
                active[clientId].food = amount;
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
            }
            else
            {
                active[clientId].water = amount;
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

    //Player Time
    public void SetPlayerTime(ulong clientId, DateTime time) 
    {
        if (Confirm(clientId)) 
        {
            active[clientId].time = time;
        }
    }

    //-----------------------------------------------------------------//
    //         BASE-LEVEL PLAYERINFO QUICK SET FUNCTIONS               //
    //-----------------------------------------------------------------//

    //Clear Player Items
    public void ClearPlayerInventory(ulong clientId)
    {
        if (active.ContainsKey(clientId))
        {
            active[clientId].items = null;
            active[clientId].armor = null;
        }
    }

    //Reset Info For Respawn
    public void ResetPlayerInfo(ulong clientId, Vector3 location) 
    {
        active[clientId].health = 100;
        active[clientId].food = 100;
        active[clientId].water = 100;
        active[clientId].location = location;
    }


    //-----------------------------------------------------------------//
    //         INVENTORY MODIFICATION | InventorySortTool              //
    //-----------------------------------------------------------------//

    //Add New Item
    public void Inventory_AddNew(ulong clientId, int itemId, int amount, Action<bool> callback) 
    {
        if (Confirm(clientId))
        {
            ItemData itemData = Inventory_GetItemData(itemId);
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
        }
    }

    //Move Item
    public void Inventory_MoveItem(ulong clientId, string authKey, int newSlot, int oldSlot)
    {
        if (Confirm(clientId, authKey))
        {
            Item[] inventory = GetPlayerItems(clientId);
            bool value = false;
            Item newItem = sit.GetItemBySlot(inventory, newSlot);
            Item oldItem = sit.GetItemBySlot(inventory, oldSlot);
            if (newItem != null && oldItem != null)
            {
                ItemData newItemData = sit.GetItemDataById(newItem.itemID);
                //Check if Item can add Durability
                if (newItem.durability + oldItem.itemStack <= newItemData.maxDurability && newItemData.durabilityId == oldItem.itemID)
                {
                    Item[] temp = sit.RemoveItemFromInventoryBySlot(oldSlot, inventory, callback => { });
                    inventory = sit.ChangeItemDurability(temp, oldItem.itemStack, newItemData.maxDurability, newSlot);

                    if (inventory != null)
                    {
                        SetPlayerItems(clientId, inventory);
                        value = true;
                    }
                }
                else if (newItemData.durabilityId == oldItem.itemID)
                {
                    Item[] temp = sit.RemoveItemFromInventoryBySlot(oldSlot, inventory, callback => { }, newItemData.maxDurability - newItem.durability);
                    inventory = sit.ChangeItemDurability(temp, oldItem.itemStack, newItemData.maxDurability, newSlot);
                    if (inventory != null)
                    {
                        SetPlayerItems(clientId, inventory);
                        value = true;
                    }
                }
                //Else Swap or Move the Item(s)
                if (!value)
                {
                    SetPlayerItems(clientId, sit.MoveItemInInventory(oldSlot, newSlot, inventory));
                }
            }
        }
    }

    //Remove Item
    public void Inventory_RemoveItem(ulong clientId, string authKey, int slot, Action<Item> droppedItem)
    {
        if (Confirm(clientId, authKey))
        {
            active[clientId].items = sit.RemoveItemFromInventoryBySlot(slot, active[clientId].items, returnValue => { droppedItem(returnValue); });
        }
    }

    //Craft Item
    public void Inventory_CraftItem(ulong clientId, string authKey, int itemId, int amount)
    {
        if (Confirm(clientId, authKey))
        {
            active[clientId].items = sit.CraftItem(active[clientId].items, itemId, amount);
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

    //Get ItemData from ID
    public ItemData Inventory_GetItemData(int itemId)
    {
        return sit.GetItemDataById(itemId);
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
                ItemData data = Inventory_GetItemData(item.itemID);
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
                return false;
            }
            if (active.ContainsKey(clientId))
            {
                if (authKey == "" || active[clientId].authKey == authKey)
                {
                    return true;
                }
                return false;
            }
        }
        return false;
    }



    //-----------------------------------------------------------------//
    //                           SYSTEM LOOPS                          //
    //-----------------------------------------------------------------//


    private IEnumerator ResourceDepletionLoop() 
    {
        yield return new WaitForSeconds(1f);
        DebugMsg.Begin(340, "Starting Deplete of All Players", 3);
        for (int i = 0; i < activeIds.Count; i++)
        {
            ResourceDeplete(activeIds[i]);
        }
        DebugMsg.End(340, "Finished Deplete of All Players", 3);
        if (systemEnabled) 
        {
            StartCoroutine(ResourceDepletionLoop());
        }
    }


    private void ResourceDeplete(ulong clientId) 
    {
        PlayerInfo player = active[clientId];
        if(player != null) 
        {
            if (player.food > 0)
            {
                active[clientId].food -= 1;
                ForceRequestInfoById(player.clientId, 3);
            }
            if (player.water > 0)
            {
                active[clientId].water -= 1;
                ForceRequestInfoById(player.clientId, 4);
            }
            if (player.water == 0 && player.food == 0)
            {
                if (player.health > 0)
                {
                    active[clientId].health -= 1;
                    ForceRequestInfoById(player.clientId, 2);
                }
                else if (player.health - 1 == 0)
                {
                    active[clientId].health = 0;
                    ForceRequestInfoById(player.clientId, 2);
                    gameServer.Server_RespawnPlayer(player.clientId);
                }
                else if (!player.isDead && player.health == 0)
                {
                    active[clientId].isDead = true;
                    gameServer.Server_RespawnPlayer(player.clientId);
                }
            }
        }
    }


    private void ForceRequestInfoById(ulong clientId, int depth = 1) 
    {
        gameServer.ForceRequestInfoById(clientId, depth);
    }

    public void AutoSave() 
    {
        SaveAllPlayerInfo();
    }
}


//Player Info Object
public class PlayerInfo
{
    //Authentication
    public string name;
    public string authKey;
    public int id;
    public ulong clientId;
    public ulong networkId;

    //Local Stats
    public int health;
    public int food;
    public int water;

    //Player Arrays
    public Item[] items;
    public Item[] armor;
    public int[] blueprints;
    
    //Bool Checks
    public bool isDead;

    //Acumulative Points
    public int coinsAdd;
    public int expAdd;
    public float hoursAdd;

    //Data for InfoChecks
    public System.DateTime time;
    public Vector3 location;
}