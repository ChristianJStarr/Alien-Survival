using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public int space = 36;  // Amount of item spaces

    // Our current list of items in the inventory
    public List<Item> items = new List<Item>();

    // Add a new item if enough room
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

        return isPlaced;
        
    }

    // Remove an item
    public void Remove(Item item)
    {
        items.Remove(item);

        if (onItemChangedCallback != null)
            onItemChangedCallback.Invoke();
    }

}
