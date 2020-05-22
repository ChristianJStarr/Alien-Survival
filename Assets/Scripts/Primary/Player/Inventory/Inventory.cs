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
    public Item[] allItems;
    public PlayerStats playerStats;

    void Start() 
    {
        allItems = Resources.LoadAll("Items", typeof(Item)).Cast<Item>().ToArray();
    }

    public void GetInventory() 
    {
        if(playerStats != null) 
        {
            string inv = playerStats.playerInventory;
            if (inv.Length > 2)
            {
                if (inv.Contains('*')) 
                {
                    string[] invSplit = inv.Split('*');
                    foreach (string invItem in invSplit[0].Split('-'))
                    {
                        if (invItem != "")
                        {
                            string[] itemData = invItem.Split('#');
                            int itemID = Convert.ToInt32(itemData[0]);
                            int curSlot = Convert.ToInt32(itemData[1]);
                            int sitSlot = Convert.ToInt32(itemData[2]);
                            int itemStack = Convert.ToInt32(itemData[3]);
                            string special = itemData[4];
                            CreateItem(itemID, curSlot, sitSlot, itemStack, special);
                            //Debug.Log("Inventory - Added item #" + itemID);
                        }
                    }
                    foreach (string invItem in invSplit[1].Split('-'))
                    {
                        if (invItem != "")
                        {
                            string[] itemData = invItem.Split('#');
                            int itemID = Convert.ToInt32(itemData[0]);
                            int curSlot = Convert.ToInt32(itemData[1]);
                            int sitSlot = Convert.ToInt32(itemData[2]);
                            int itemStack = Convert.ToInt32(itemData[3]);
                            string special = itemData[4];
                            CreateArmor(itemID, curSlot, sitSlot, itemStack, special);
                            //Debug.Log("Inventory - Added item #" + itemID);
                        }
                    }
                }
                else 
                {
                    foreach (string invItem in inv.Split('-'))
                    {
                        if (invItem != "")
                        {
                            string[] itemData = invItem.Split('#');
                            int itemID = Convert.ToInt32(itemData[0]);
                            int curSlot = Convert.ToInt32(itemData[1]);
                            int sitSlot = Convert.ToInt32(itemData[2]);
                            int itemStack = Convert.ToInt32(itemData[3]);
                            string special = itemData[4];
                            CreateItem(itemID, curSlot, sitSlot, itemStack, special);
                            //Debug.Log("Inventory - Added item #" + itemID);
                        }
                    }
                }
            }
        }
        inventoryScript.UpdateUI();
    }

    private void SetInventory() 
    {
        if (playerStats != null)
        {
            string dataString = "";
            foreach (Item item in items)
            {
                int itemID = item.itemID;
                int curSlot = item.currSlot;
                int sitSlot = item.sitSlot;
                int itemStack = item.itemStack;
                string special = item.special;
                string data = "-" + itemID + "#" + curSlot + "#" + sitSlot + "#" + itemStack + "#" + special;
                dataString += data;
            }

            dataString += "*";
            foreach (Item item in armor)
            {
                int itemID = item.itemID;
                int curSlot = item.currSlot;
                int sitSlot = item.sitSlot;
                int itemStack = item.itemStack;
                string special = item.special;
                string data = "-" + itemID + "#" + curSlot + "#" + sitSlot + "#" + itemStack + "#" + special;
                dataString += data;
            }
            playerStats.playerInventory = dataString;
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
                SetInventory();
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
            SetInventory();
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
        }
        if (armor.Contains(item)) 
        {
            armor.Remove(item);
        }
        inventoryScript.GetAvailableCraft();
        SetInventory();
        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

}
