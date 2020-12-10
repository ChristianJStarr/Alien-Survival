using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerInventoryTool : MonoBehaviour
{
    private int space = 33;


    //-----------------------------------------------------------------//
    //                      Inventory Items                            //
    //-----------------------------------------------------------------//

    //Add Item to Inventory
    public void AddItemToInventory(Item item, Item[] inventory, Action<bool> callback)
    {
        AddItemToInventoryTask(item, inventory, returnValue =>
        {
            callback(returnValue);
        });
        SortItemSlotsInInventory(inventory);
    }

    //Add New Item To Inventory
    public void AddNewItemToInventory(Item[] inventory, ItemData data, int amount, Action<bool> callback)
    {
        if (amount > data.maxItemStack)
        {
            while (amount > 0)
            {
                if (amount > data.maxItemStack)
                {
                    amount -= data.maxItemStack;
                    AddItemToInventory(CreateItemFromData(data, data.maxItemStack), inventory, returnValue => { });
                }
                else
                {
                    AddItemToInventory(CreateItemFromData(data, amount), inventory, returnValue => { });
                    break;
                }
            }
        }
        else
        {
            AddItemToInventory(CreateItemFromData(data, amount), inventory, returnValue => { });
        }
    }

    //Remove Item from Inventory
    public bool RemoveItemFromInventory(int id, int amount, Item[] inventory)
    {
        bool wasRemoved = RemoveItemFromInventoryTask(id, amount, inventory);
        SortItemSlotsInInventory(inventory);
        return wasRemoved;
    }

    //Remove Item from Inventory Max
    public int GetMaxAvailableInventory(int id, Item[] inventory)
    {
        int stored = 0;
        foreach (Item item in inventory)
        {
            if (item.itemID == id)
            {
                stored += item.itemStack;
            }
        }
        return stored;
    }

    //Move Item In Inventory
    public void MoveItemInInventory(int curSlot, int newSlot, Item[] inventory)
    {
        int changed = 0;
        foreach (Item item in inventory)
        {
            if (changed == 2) { break; }
            if (item.currSlot == curSlot)
            {
                item.currSlot = newSlot;
                changed++;
            }
            else if (item.currSlot == newSlot)
            {
                item.currSlot = curSlot;
                changed++;
            }
        }
    }

    //Remove Item From Inventory By Slot
    public bool RemoveItemFromInventoryBySlot(int slot, Item[] inventory, Action<Item> callback, int amount = 0)
    {
        if (amount != 0)
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].currSlot == slot)
                {
                    if (inventory[i].itemStack > amount)
                    {
                        inventory[i].itemStack -= amount;
                        callback(inventory[i]);
                        return true;
                    }
                    else
                    {
                        callback(inventory[i]);
                        List<Item> temp = inventory.ToList();
                        temp.RemoveAt(i);
                        inventory = temp.ToArray();
                        return true;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].currSlot == slot)
                {
                    callback(inventory[i]);
                    List<Item> temp = inventory.ToList();
                    temp.RemoveAt(i);
                    inventory = temp.ToArray();
                    return true;
                }
            }
        }
        return false;
    }

    //Change Item Durability
    public bool ChangeItemDurability(Item[] inventory, int amount, int maxDurability, int slot)
    {
        foreach (Item item in inventory)
        {
            if (item.currSlot == slot)
            {
                if (item.durability + amount > 0 && item.durability + amount <= maxDurability)
                {
                    item.durability += amount;
                    return true;
                }
                break;
            }
        }
        return false;
    }

    //Split Item Stack
    public void SplitItemStackById(Item[] inventory, int slot, int amount)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].currSlot == slot)
            {
                if (inventory[i].itemStack > amount && inventory.Length < space)
                {
                    inventory[i].itemStack -= amount;
                    Item newItem = inventory[i];
                    newItem.currSlot = 44;
                    newItem.itemStack = amount;
                    List<Item> temp = inventory.ToList();
                    temp.Add(newItem);
                    inventory = temp.ToArray();
                    SortItemSlotsInInventory(inventory);
                }
                break;
            }
        }
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

    public void MoveArmorInInventory(int curSlot, int newSlot, Item[] inventory, Item[] armor)
    {
        bool isPlaced = false;
        if (curSlot > 33)
        {
            //Armor move to inventory slot
            foreach (Item armorItem in armor)
            {
                if (armorItem.currSlot == curSlot)
                {
                    foreach (Item invItem in inventory)
                    {
                        if (invItem.currSlot == newSlot)
                        {
                            ItemData itemData = ItemDataManager.Singleton.GetItemData(invItem.itemID);
                            ItemData armorData = ItemDataManager.Singleton.GetItemData(armorItem.itemID);
                            if (itemData.isArmor && itemData.armorType == armorData.armorType)
                            {
                                //Swap Armor with Inventory Armor
                                isPlaced = true;
                                invItem.currSlot = curSlot;
                                armorItem.currSlot = newSlot;
                                AddItemToInventory(armorItem, inventory, returnValue => { });
                                List<Item> invList = inventory.ToList();
                                List<Item> newArmor = armor.ToList();
                                newArmor.Remove(armorItem);
                                invList.Remove(invItem);
                                newArmor.Add(invItem);
                            }
                            else
                            {
                                //Cant hotswap so -- Place randomly
                                AddItemToInventory(armorItem, inventory, returnValue => { });
                                //Item was placed successfully.
                                isPlaced = true;
                                List<Item> newArmor = armor.ToList();
                                newArmor.Remove(armorItem);
                                armor = newArmor.ToArray();
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
                            AddItemToInventory(armorItem, inventory, returnValue => { });
                        }
                    }
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].currSlot == curSlot)
                {
                    Item item = inventory[i];
                    ItemData itemData = ItemDataManager.Singleton.GetItemData(item.itemID);
                    if (itemData.isArmor)
                    {
                        for (int e = 0; e < armor.Length; e++)
                        {
                            if (armor[e].currSlot == newSlot)
                            {
                                Item armorItem = armor[e];
                                ItemData armorData = ItemDataManager.Singleton.GetItemData(armorItem.itemID);
                                if (itemData.armorType == armorData.armorType)
                                {
                                    item.currSlot = newSlot;
                                    armorItem.currSlot = curSlot;
                                    inventory[i] = armorItem;
                                    armor[e] = item;
                                }
                            }
                        }
                        //Armor slot is empty. Attempt to move item there.
                        if (itemData.armorType == 33 - newSlot)
                        {
                            //Armor type matches slot. Place it there.
                            item.currSlot = newSlot;

                            //Moving of item from inventory array to armor array
                            List<Item> newInventory = inventory.ToList();
                            List<Item> newArmor = armor.ToList();
                            newArmor.Add(item);
                            newInventory.Remove(item);
                            inventory = newInventory.ToArray();
                            armor = newArmor.ToArray();
                        }
                    }
                    break;
                }
            }
        }
    }




    //-----------------------------------------------------------------//
    //                      BIG INVENTORY TASKS                        //
    //-----------------------------------------------------------------//

    //Add Item To Inventory
    private void AddItemToInventoryTask(Item item, Item[] inventory, Action<bool> callback)
    {
        if (inventory == null) inventory = new Item[0];
        if (inventory.Length > 0) //Inventory has Items, can we stack?
        {
            int inventoryLength = inventory.Length;
            Item[] tempItems = inventory;
            for (int i = 0; i < inventoryLength; i++)
            {
                if (item.itemStack == 0) break;
                if (inventory[i].itemID == item.itemID) //Same Item Exists 
                {
                    ItemData itemData = ItemDataManager.Singleton.GetItemData(item.itemID);
                    int stackRoom = itemData.maxItemStack - inventory[i].itemStack;
                    if (stackRoom > 0) //Room to stack on this item
                    {
                        if (item.itemStack <= stackRoom)
                        {
                            inventory[i].itemStack += item.itemStack;
                        }
                        else
                        {
                            tempItems[i].itemStack += stackRoom;
                            item.itemStack -= stackRoom;
                        }
                    }
                }
            }
            if (inventoryLength < space && item.itemStack != 0)
            {
                item.currSlot = 44;
                AddItemDirectTask(tempItems, item);
            }
            else if (item.itemStack == 0)
            {
                inventory = tempItems;
            }
        }
        else // Item is Empty, just throw it in
        {
            item.currSlot = 44; // Slot 44 - Auto-Sort
            AddItemDirectTask(inventory, item);
        }
    }

    //Add Item directly to Array
    private void AddItemDirectTask(Item[] inventory, Item item) 
    {
        if (item.itemStack == 0) return;
        if(inventory.Length > 0) 
        {
            List<Item> tempList = new List<Item>();
            for (int i = 0; i < inventory.Length; i++)
            {
                tempList.Add(inventory[i]);
            }
            tempList.Add(item);
            inventory = tempList.ToArray();
        }
        else 
        {
            Item[] tempArray = new Item[1];
            tempArray[0] = item;
            inventory = tempArray;
        }
    }


    //Sort Item Slots in Inventory
    private void SortItemSlotsInInventory(Item[] inventory)
    {
        List<Item> unassigned = new List<Item>();
        List<int> assigned = new List<int>();
        List<Item> newInventory = new List<Item>();

        foreach (Item item in inventory)
        {
            if (item.currSlot == 44 || item.currSlot == 0)
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
            if (!assigned.Contains(slot) && unassigned.Count > 0)
            {
                Item item = unassigned.First();
                item.currSlot = slot;
                assigned.Add(slot);
                newInventory.Add(item);
                unassigned.Remove(item);
            }
        }
        inventory = newInventory.ToArray();
    }

    //Remove Item From Inventory
    private bool RemoveItemFromInventoryTask(int id, int amount, Item[] inventory)
    {
        foreach (Item item in inventory)
        {
            if (item.itemID == id)
            {
                if (item.itemStack > amount) //More than enough items 
                {
                    item.itemStack -= amount;
                    amount = 0;
                }
                else if (item.itemStack == amount) //Just enough items
                {
                    item.itemStack = 0;
                    amount = 0;
                }
                else if (item.itemStack < amount) //Not enough items
                {
                    item.itemStack = 0;
                    amount -= item.itemStack;
                }
            }
        }
        if (amount == 0)
        {
            List<Item> newInventory = new List<Item>();
            foreach (Item item in inventory)
            {
                if (item.itemStack != 0)
                {
                    newInventory.Add(item);
                }
            }
            inventory = newInventory.ToArray();
            return true;
        }
        return false;
    }


    //-----------------------------------------------------------------//
    //                      CRAFTING FUNCTIONS                         //
    //-----------------------------------------------------------------//

    public Item GetItemBySlot(Item[] inventory, int slot)
    {
        foreach (Item item in inventory)
        {
            if (item.currSlot == slot)
            {
                return item;
            }
        }
        return null;
    }

    public bool SlotBelongsToItems(int slot)
    {
        if (slot > 0 && slot < 34)
        {
            return true;
        }
        return false;
    }

    public bool SlotBelongsToArmor(int slot)
    {
        if (slot > 33 && slot < 48)
        {
            return true;
        }
        return false;
    }

    public bool SlotBelonsToStorage(int slot)
    {
        if (slot > 49)
        {
            return true;
        }
        return false;
    }


    //-----------------------------------------------------------------//
    //                      CRAFTING FUNCTIONS                         //
    //-----------------------------------------------------------------//

    //Initiate Craft Sequence
    public void CraftItem(Item[] inventory, int itemId, int amount)
    {
        ItemData craftItem = ItemDataManager.Singleton.GetItemData(itemId);
        if (HasRecipe(craftItem.recipe, inventory) && CanAddItem(amount * craftItem.craftAmount, itemId, inventory))
        {
            AddNewItemToInventory(inventory, craftItem, amount, returnValue => { });
            RemoveItemsByRecipe(craftItem.recipe, amount, inventory);
        }
    }

    //Calculate Resources in Inventory
    private List<InventoryResource> CalculateResources(Item[] inventory)
    {
        List<InventoryResource> resources = new List<InventoryResource>();
        foreach (Item itemRes in inventory)
        {
            bool placed = false;
            foreach (InventoryResource invItem in resources)
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
                resources.Add(newRes);
            }
        }
        return resources;
    }

    //Check if enough for crafting
    public bool HasRecipe(string[] recipe, Item[] inventory)
    {
        int recipeAmount = recipe.Length;
        int recipeAvail = 0;
        foreach (string recipeData in recipe)
        {
            string[] data = recipeData.Split('-');
            int item = Convert.ToInt32(data[0]);
            int amount = Convert.ToInt32(data[1]);
            bool hasResources = false;

            foreach (InventoryResource resource in CalculateResources(inventory))
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

    //Create Item from Item Data
    public Item CreateItemFromData(ItemData itemData, int amount)
    {
        Item item = new Item() 
        {
            itemID = itemData.itemID,
            itemStack = amount,
            currSlot = 44
        };
        if (itemData.startMaxDurability)
        {
            item.durability = itemData.maxDurability;
        }
        return item;
    }

    //Check if you can add the item to inventory
    public bool CanAddItem(int amount, int itemId, Item[] inventory)
    {
        return true;
    }

    //Remove Items by Recipe
    public void RemoveItemsByRecipe(string[] recipe, int amount, Item[] inventory)
    {
        foreach (string recipeItem in recipe)
        {
            string[] recipeData = recipeItem.Split('-');
            int r_itemId = Convert.ToInt32(recipeData[0]);
            int r_amount = Convert.ToInt32(recipeData[1]);
            RemoveItemFromInventoryTask(r_itemId, r_amount * amount, inventory);
        }
    }
}