using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugHandler : MonoBehaviour
{
    Item[] allItems;
    public Inventory inventory;
    public GameObject content;
    public GameObject debugItem;

    void Start()
    {
        allItems = Resources.LoadAll("Items", typeof(Item)).Cast<Item>().ToArray();
        AddItemsToMenu();
    }

    public void Item(Item item) 
    {
        Item newItem = Instantiate(item);
        if(newItem != null) 
        {
            newItem.itemStack = 10;
            inventory.Add(newItem);
        }
    }
    public void AddItemsToMenu() 
    {
        foreach (Item item in allItems)
        {
            GameObject newItem = Instantiate(debugItem, content.transform);
            DebugItem newItemE = newItem.GetComponent<DebugItem>();
            newItemE.SetType(item.icon, item);
            newItemE.debugHandler = this;
        }    
    }
}
