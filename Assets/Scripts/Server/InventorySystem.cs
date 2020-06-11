using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private bool show_Stopwatch = true;
    private int space = 27;

    //-----------------------------------------------------------------//
    //                      Inventory Items                            //
    //-----------------------------------------------------------------//

    //Add Item to Inventory
    public Item[] AddItemToInventory(Item item, Item[] inventory) 
    {
        return SortItemSlotsInInventory(AddItemToInventoryTask(item, inventory));
    }
    
    //Remove Item from Inventory
    public Item[] RemoveItemFromInventory(int id, int amount, Item[] inventory) 
    {
        return SortItemSlotsInInventory(RemoveItemFromInventoryTask(id, amount, inventory));
    }

    //Move Item In Inventory
    public Item[] MoveItemInInventory(int curSlot, int newSlot, Item[] inventory)
    {
        foreach (Item item in inventory)
        {
            if(item.currSlot == curSlot) 
            {
                item.currSlot = newSlot;
            }
            else if(item.currSlot == newSlot) 
            {
                item.currSlot = curSlot;
            }
        }
        return inventory;
    }

    //Remove Item From Inventory By Slot
    public Item[] RemoveItemFromInventoryBySlot(int curSlot, Item[] inventory, Action<Item> callback) 
    {

        List<Item> newInventory = new List<Item>();
        foreach (Item item in inventory)
        {
            if(item.currSlot != curSlot) 
            {
                newInventory.Add(item);
            }
            else 
            {
                callback(item);
            }
        }
        return newInventory.ToArray();
    }

    //-----------------------------------------------------------------//
    //                      Blueprint Items                            //
    //-----------------------------------------------------------------//

    //Add Blueprint to Blueprint List
    public int[] AddBlueprintToBlueprints(int id, int[] blueprints) 
    {
        List<int> newBlueprints = blueprints.ToList();
        if (!newBlueprints.Contains(id)) 
        {
            newBlueprints.Add(id);
        }
        return newBlueprints.ToArray();
    }

    //-----------------------------------------------------------------//
    //                      Armor Items                                //
    //-----------------------------------------------------------------//

    public ItemArmorArrays MoveArmorInInventory(int curSlot, int newSlot, Item[] inventory, Item[] armor) 
    {
        bool isPlaced = false;
        if(curSlot > 33) 
        {
            //Armor move to inventory slot
            foreach(Item armorItem in armor) 
            {
                if(armorItem.currSlot == curSlot) 
                {
                    foreach(Item invItem in inventory) 
                    {
                        if(invItem.currSlot == newSlot) 
                        {
                            if (invItem.isArmor && invItem.armorType == armorItem.armorType) 
                            {
                                //Swap Armor with Inventory Armor
                                isPlaced = true;
                                invItem.currSlot = curSlot;
                                armorItem.currSlot = newSlot;
                                Item[] newInventory = AddItemToInventory(armorItem, inventory);
                                if (newInventory != null)
                                {
                                    inventory = newInventory;
                                    List<Item> invList = inventory.ToList();
                                    List<Item> newArmor = armor.ToList();
                                    newArmor.Remove(armorItem);
                                    invList.Remove(invItem);
                                    newArmor.Add(invItem);
                                }
                            }
                            else
                            {
                                //Cant hotswap so -- Place randomly
                                Item[] newInventory = AddItemToInventory(armorItem, inventory);
                                if(newInventory != null) 
                                {
                                    //Item was placed successfully.
                                    isPlaced = true;
                                    inventory = newInventory;
                                    List<Item> newArmor = armor.ToList();
                                    newArmor.Remove(armorItem);
                                    armor = newArmor.ToArray();
                                }
                            }
                            break;
                        }
                        if (!isPlaced)
                        {
                            //New Slot is Empty
                            List<Item> newArmor = armor.ToList();
                            newArmor.Remove(armorItem);
                            armor = newArmor.ToArray();
                            armorItem.currSlot = newSlot;
                            Item[] newInventory = AddItemToInventory(armorItem, inventory);
                            if(newInventory != null) 
                            {
                                inventory = newInventory;
                            }
                        }
                    }
                    break;
                }
            }
        }
        else 
        {
            //Inventory move to armor slot
            foreach(Item invItem in inventory) 
            {
                if(invItem.currSlot == curSlot) 
                {
                    foreach (Item armorItem in armor)
                    {
                        if(armorItem.currSlot == newSlot) 
                        {
                            //Trying to move inventory item to occupied slot 
                            if (armorItem.isArmor && armorItem.armorType == invItem.armorType)
                            {
                                isPlaced = true;
                                invItem.currSlot = newSlot;
                                armorItem.currSlot = curSlot;
                                List<Item> newInventory = inventory.ToList();
                                List<Item> newArmor = armor.ToList();
                                newArmor.Add(invItem);
                                newInventory.Remove(invItem);
                                newInventory.Add(armorItem);
                                newArmor.Remove(armorItem);
                                inventory = newInventory.ToArray();
                                armor = newArmor.ToArray();
                            }
                            break;
                        }
                    }
                    if (!isPlaced) 
                    {
                        //Armor slot is empty. Attempt to move item there.
                        if (invItem.armorType == 33 - newSlot) 
                        {
                            //Armor type matches slot. Place it there.
                            invItem.currSlot = newSlot;
                            List<Item> newInventory = inventory.ToList();
                            List<Item> newArmor = armor.ToList();
                            newArmor.Add(invItem);
                            newInventory.Remove(invItem);
                            inventory = newInventory.ToArray();
                            armor = newArmor.ToArray();
                        }
                    }
                    break;
                }
            }
        }


        ItemArmorArrays result = new ItemArmorArrays();
        result.items = inventory;
        result.armor = armor;
        return result;
    }

    //Tasks 

    //Add Item To Inventory
    private Item[] AddItemToInventoryTask(Item item, Item[] inventory) 
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        bool isPlaced = false;
        List<Item> items = new List<Item>();
        if(inventory != null) 
        {
            items = inventory.ToList();
        }
        if (item.showInInventory)
        {
            if (items.Count == 0)
            {
                item.currSlot = 44;
                items.Add(item);
                isPlaced = true;
            }
            else if (item.maxItemStack > 1)
            {
                foreach (Item stored in items)
                {
                    if (!isPlaced)
                    {
                        if (stored.itemID == item.itemID)
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
        }

        if (isPlaced) 
        {
            stopwatch.Stop();
            if (show_Stopwatch)
            {
                UnityEngine.Debug.Log("Added Item To Player Inventory. Time Taken: " + (stopwatch.Elapsed));
            }
            stopwatch.Reset();
        }
        else 
        {
            stopwatch.Stop();
            if (show_Stopwatch)
            {
                UnityEngine.Debug.Log("Unable To Add Item to Player Inventory. Time Taken: " + (stopwatch.Elapsed));
            }
            stopwatch.Reset();
        }
        return items.ToArray();
    }
    
    //Sort Item Slots in Inventory
    private Item[] SortItemSlotsInInventory(Item[] inventory) 
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        List<Item> unassigned = new List<Item>();
        List<int> assigned = new List<int>();
        List<Item> newInventory = new List<Item>();

        foreach(Item item in inventory) 
        {
            if(item.currSlot == 44 || item.currSlot == 0) 
            {
                unassigned.Add(item);
            }
            else 
            {
                assigned.Add(item.currSlot);
                newInventory.Add(item);
            }
        }
        for (int i = 0; i < space; i++)
        {
            int slot = i + 1;
            if(!assigned.Contains(slot) && unassigned.Count > 0) 
            {
                Item item = unassigned.First();
                UnityEngine.Debug.Log("Setting new slot" + slot);
                item.currSlot = slot;
                assigned.Add(slot);
                newInventory.Add(item);
                unassigned.Remove(item);
            }
        }
        stopwatch.Stop();
        if (show_Stopwatch)
        {
            UnityEngine.Debug.Log("Sorted Player Inventory. Time Taken: " + (stopwatch.Elapsed));
        }
        stopwatch.Reset();
        return newInventory.ToArray();
    }
    
    //Remove Item From Inventory
    private Item[] RemoveItemFromInventoryTask(int id, int amount, Item[] inventory)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        foreach (Item item in inventory) 
        {
            if(item.itemID == id) 
            {
                if (item.itemStack > amount) //More than enough items 
                {
                    item.itemStack -= amount;
                    amount = 0;
                }
                else if(item.itemStack == amount) //Just enough items
                {
                    item.itemStack = 0;
                    amount = 0;
                }
                else if(item.itemStack < amount) //Not enough items
                {
                    item.itemStack = 0;
                    amount -= item.itemStack;
                }
            }
        }
        if(amount == 0) 
        {
            List<Item> newInventory = new List<Item>();
            foreach (Item item in inventory)
            {
                if (item.itemStack != 0) 
                {
                    newInventory.Add(item);    
                }
            }
            stopwatch.Stop();
            if (show_Stopwatch)
            {
                UnityEngine.Debug.Log("Removed Items From Player Inventory. Time Taken: " + (stopwatch.Elapsed));
            }
            stopwatch.Reset();
            return newInventory.ToArray();
        }
        else 
        {
            stopwatch.Stop();
            if (show_Stopwatch)
            {
                UnityEngine.Debug.Log("Removed Items From Player Inventory. Time Taken: " + (stopwatch.Elapsed));
            }
            stopwatch.Reset();
            return null;
        }
    }
    
}

public class ItemArmorArrays 
{
    public Item[] items;
    public Item[] armor;
}