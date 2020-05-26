using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Inventory : MonoBehaviour
{

    #region Singleton

    public static Inventory instance;

    void Awake()
    {
        instance = this;
    }

    #endregion

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;
    public InventoryScript inventoryScript;
    public int space = 33;
    public List<Item> items;
    public List<Item> armor;
    public List<Item> blueprints;
    public Item[] allItems;
    public PlayerStats playerStats;

    void Start() 
    {
        allItems = Resources.LoadAll("Items", typeof(Item)).Cast<Item>().ToArray();
    }

    public void Incoming(PlayerInfo playerInfo) 
    {
        //string items = playerInfo.items;
        //string armor = playerInfo.armor;
        //string blueprints = playerInfo.blueprints;
        //List<PartialItem> incomingItems = new List<PartialItem>();

        //if(items.Length > 0) 
        //{
        //    foreach (string item in items.Split('!'))
        //    {
        //        incomingItems.Add(GetPartialItemFromString(item));
        //    }
        //}
        //if (armor.Length > 0)
        //{
        //    foreach (string item in items.Split('!'))
        //    {
        //        incomingItems.Add(GetPartialItemFromString(item));
        //    }
        //}
        //if (blueprints.Length > 0)
        //{
        //    foreach (string item in items.Split('!'))
        //    {
        //        incomingItems.Add(GetPartialItemFromString(item));
        //    }
        //}

    }

    private PartialItem GetPartialItemFromString(string value) 
    {
        string[] data = value.Split(',');
        PartialItem newItem = new PartialItem();
        newItem.itemStack = Convert.ToInt32(data[0]);
        newItem.itemID = Convert.ToInt32(data[1]);
        newItem.currSlot = Convert.ToInt32(data[2]);
        newItem.sitSlot = Convert.ToInt32(data[3]);
        newItem.special = data[4];
        return newItem;
    }


    private void SetInventory(int type)
    {
        //Type 1 - Set Items
        //Type 2 - Set Armor
        //Type 3 - Set Blueprints

        if(type == 1) 
        {
            string itemsString = "";
            foreach (Item item in items)
            {
                string builder = "";
                builder += item.itemStack + ",";
                builder += item.itemID + ",";
                builder += item.currSlot + ",";
                builder += item.sitSlot + ",";
                builder += item.special + ",";
                itemsString += builder + "!";
            }
            PlayerInfoManager.singleton.SetPlayer_InventoryItems(itemsString);
        }
        if(type == 2) 
        {
            string armorString = "";
            foreach (Item item in armor)
            {
                string builder = "";
                builder += item.itemStack + ",";
                builder += item.itemID + ",";
                builder += item.currSlot + ",";
                builder += item.sitSlot + ",";
                builder += item.special + ",";
                armorString += builder + "!";
            }
            PlayerInfoManager.singleton.SetPlayer_InventoryArmor(armorString);
        }
        if(type == 3) 
        {
            string blueprintsString = "";
            foreach (Item item in blueprints)
            {
                string builder = "";
                builder += item.itemStack + ",";
                builder += item.itemID + ",";
                builder += item.currSlot + ",";
                builder += item.sitSlot + ",";
                builder += item.special + ",";
                blueprintsString += builder + "!";
            }
            PlayerInfoManager.singleton.SetPlayer_InventoryBlueprints(blueprintsString);
        }
    }


    private Item FindItem(int itemID) 
    {
        Item newItem = null;
        foreach (Item item in allItems)
        {
            if (item.itemID == itemID) 
            {
                newItem = item;
                break;
            }
        }
        return newItem;
    }
    public void CreateItem(int itemID, int curSlot, int sitSlot, int itemStack, string special) 
    {
        if (!FindItem(itemID))
            return;

        Item newItem = Instantiate(FindItem(itemID));
        newItem.currSlot = curSlot;
        newItem.sitSlot = sitSlot;
        newItem.itemStack = itemStack;
        newItem.special = special;
        items.Add(newItem);
    }
    public bool AddNewItem(int itemID, int curSlot, int sitSlot, int itemStack, string special)
    {
        Item newItem = Instantiate(FindItem(itemID));
        newItem.currSlot = curSlot;
        newItem.sitSlot = sitSlot;
        newItem.itemStack = itemStack;
        newItem.special = special;
        
        return Add(newItem);
    }

    private void CreateArmor(int itemID, int curSlot, int sitSlot, int itemStack, string special)
    {
        Item newItem = Instantiate(FindItem(itemID));
        newItem.currSlot = curSlot;
        newItem.sitSlot = sitSlot;
        newItem.itemStack = itemStack;
        newItem.special = special;
        armor.Add(newItem);
    }

    public bool Add(Item item)
    {
        bool isPlaced = false;
        if (item.showInInventory)
        {
            if (items.Count == 0)
            {
                items.Add(item);
                isPlaced = true;
            }
            else if (item.maxItemStack > 1)
            {
                foreach (Item stored in items)
                {
                    if (!isPlaced)
                    {
                        if (stored.name == item.name)
                        {
                            int stack = stored.maxItemStack - stored.itemStack;
                            if (stack != 0)
                            {
                                if (item.itemStack <= stack)
                                {
                                    stored.itemStack += item.itemStack;
                                    isPlaced = true;
                                    break;
                                }
                                else if (item.itemStack > stack && items.Count < space)
                                {
                                    stored.itemStack += stack;
                                    item.itemStack -= stack;
                                    item.currSlot = 44;
                                    items.Add(item);
                                    isPlaced = true;
                                    break;
                                }
                            }
                            else if (items.Count < space)
                            {
                                item.currSlot = 44;
                                items.Add(item);
                                isPlaced = true;
                                break;
                            }
                        }
                    }
                }
                if (!isPlaced && items.Count < space)
                {
                    item.currSlot = 44;
                    items.Add(item);
                    isPlaced = true;
                }
            }
            else if (items.Count < space)
            {
                item.currSlot = 44;
                items.Add(item);
                isPlaced = true;
            }
            else
            {
                isPlaced = false;
            }
            if (onItemChangedCallback != null)
            {
                onItemChangedCallback.Invoke();
            }
            if (isPlaced) 
            {
                inventoryScript.GetAvailableCraft();
                SetInventory(1);
                inventoryScript.UpdateUI();
            }
        }
        return isPlaced;
    }


    public bool AddSingle (Item item)
    {
        bool isPlaced = false;
        if (item.showInInventory)
        {
            if (items.Count < space)
            {
                items.Add(item);
                isPlaced = true;
            }
            else
            {
                isPlaced = false;
            }
            if (onItemChangedCallback != null)
            {
                onItemChangedCallback.Invoke();
            }
        }
        if (isPlaced)
        {
            SetInventory(1);
            inventoryScript.GetAvailableCraft();
            inventoryScript.UpdateUI();
        }
        return isPlaced;
    }

    public void Remove(Item item)
    {
        if (items.Contains(item)) 
        {
            items.Remove(item);
            SetInventory(1);
        }
        if (armor.Contains(item)) 
        {
            SetInventory(2);
            armor.Remove(item);
        }
        inventoryScript.GetAvailableCraft();
        SetInventory(1);
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

}
