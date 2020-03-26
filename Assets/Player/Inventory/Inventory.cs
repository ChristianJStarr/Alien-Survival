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

    public int space = 36;
    public List<Item> items = new List<Item>();
    public Item[] allItems;
    public PlayerStats playerStats;

    void Start() 
    {
        allItems = Resources.LoadAll("Items", typeof(Item)).Cast<Item>().ToArray();
        GetInventory();
    }

    private void GetInventory() 
    {
        if(playerStats != null) 
        {
            string inv = playerStats.playerInventory;
            if (inv.Length > 2)
            {
                string[] invData = inv.Split('-');
                foreach (string invItem in invData)
                {
                    if (invItem == "") 
                    {
                    
                    }
                    else 
                    {
                        Debug.Log(invItem);
                        string[] itemData = invItem.Split('#');
                        int itemID = Convert.ToInt32(itemData[0]);
                        int curSlot = Convert.ToInt32(itemData[1]);
                        int sitSlot = Convert.ToInt32(itemData[2]);
                        int itemStack = Convert.ToInt32(itemData[3]);
                        string special = itemData[4];
                        CreateItem(itemID, curSlot, sitSlot, itemStack, special);
                        Debug.Log("Inventory - Added item #" + itemID);
                    }
                }
            }
        }
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
            playerStats.playerInventory = dataString;
            Debug.Log("Inventory - Set Data");
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
    private void CreateItem(int itemID, int curSlot, int sitSlot, int itemStack, string special) 
    {
        Item newItem = Instantiate(FindItem(itemID));
        newItem.currSlot = curSlot;
        newItem.sitSlot = sitSlot;
        newItem.itemStack = itemStack;
        newItem.special = special;
        Add(newItem);
    }

    public bool Add(Item item)
    {

        bool isPlaced = false;
        if (item.showInInventory)
        {
            if (items.Count == 0) 
            {
                item.itemStack = 1;
                items.Add(item);
                isPlaced = true;
            }
            else if (item.maxItemStack > 1)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if (isPlaced == false)
                    {
                        Item otem = items[i];
                        if (otem.name == item.name && otem.maxItemStack > otem.itemStack)
                        {

                            otem.itemStack = otem.itemStack + 1;
                            isPlaced = true;
                        }
                        else if (i == items.Count && items.Count < space)
                        {
                            items.Add(item);
                            isPlaced = true;
                        }
                    }
                }
            }
            else 
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
                
            }
            
            if (onItemChangedCallback != null) {
                onItemChangedCallback.Invoke();
            }
    
        }

        if (isPlaced) 
        {
            SetInventory();
        }

        return isPlaced;
        
    }

    public void Remove(Item item)
    {
        items.Remove(item);
        SetInventory();

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

}
