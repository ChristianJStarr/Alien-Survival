using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerInventoryTool
{
    private int space = 33;
    private ItemData[] itemDatas;

    private void GatherAllItemDatas() 
    {
        if(itemDatas != null)
        {
            itemDatas = Resources.LoadAll("Items", typeof(ItemData)).Cast<ItemData>().ToArray();
        }
    }


    //-----------------------------------------------------------------//
    //                      Inventory Items                            //
    //-----------------------------------------------------------------//

    //Add Item to Inventory
    public Item[] AddItemToInventory(Item item, Item[] inventory, Action<bool> callback)
    {
        return SortItemSlotsInInventory(AddItemToInventoryTask(item, inventory, returnValue => 
        {
            callback(returnValue);
        }));
    }

    public Item[] AddNewItemToInventory(Item[] inventory, ItemData data, int amount, Action<bool> callback) 
    {
        foreach (ItemData datas in itemDatas)
        {
            if(datas.itemID == data.itemID) 
            {
                if (amount > datas.maxItemStack)
                {
                    while (amount > 0)
                    {
                        if (amount > datas.maxItemStack)
                        {
                            amount -= datas.maxItemStack;
                            inventory = AddItemToInventory(CreateItemFromData(datas, datas.maxItemStack), inventory, returnValue => { });
                        }
                        else
                        {
                            inventory = AddItemToInventory(CreateItemFromData(datas, amount), inventory, returnValue => { });
                            break;
                        }
                    }
                }
                else
                {
                    inventory = AddItemToInventory(CreateItemFromData(datas, amount), inventory, returnValue => { });
                }
            }
        }
        return inventory;
    }

    //Remove Item from Inventory
    public Item[] RemoveItemFromInventory(int id, int amount, Item[] inventory)
    {
        return SortItemSlotsInInventory(RemoveItemFromInventoryTask(id, amount, inventory));
    }

    //Remove Item from Inventory Max
    public int GetMaxAvailableInventory(int id, Item[] inventory)
    {
        int stored = 0;
        foreach (Item item in inventory)
        {
            if(item.itemID == id) 
            {
                stored += item.itemStack;
            }
        }
        return stored;
    }

    //Move Item In Inventory
    public Item[] MoveItemInInventory(int curSlot, int newSlot, Item[] inventory)
    {
        int changed = 0;
        foreach (Item item in inventory)
        {
            if(changed == 2) { break; }
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
        return inventory;
    }

    //Remove Item From Inventory By Slot
    public Item[] RemoveItemFromInventoryBySlot(int curSlot, Item[] inventory, Action<Item> callback,int amount = 0)
    {
        List<Item> inv = inventory.ToList();
        Item item = inv.Single(r => r.currSlot == curSlot);
        if(item != null) 
        {
            if(amount != 0) 
            {
                item.itemStack -= amount;
            }
            else 
            {
                inv.Remove(item);
            }
            callback(item);
        }
        else 
        {
            return null;
        }
        return inv.ToArray();
    }

    //Change Item Durability
    public Item[] ChangeItemDurability(Item[] inventory, int amount, int maxDurability, int slot) 
    {
        
        foreach (Item item in inventory)
        {
            if(item.currSlot == slot) 
            {
                if (item.durability + amount > 0 && item.durability + amount <= maxDurability) 
                {
                    item.durability += amount;
                }
                else if (item.durability + amount <= 0) 
                {
                    return null;
                }
                break;
            }
        }
        return inventory;
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
                                Item[] newInventory = AddItemToInventory(armorItem, inventory, returnValue => { });
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
                                Item[] newInventory = AddItemToInventory(armorItem, inventory, returnValue => { });
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
                            Item[] newInventory = AddItemToInventory(armorItem, inventory, returnValue => { });
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




    //-----------------------------------------------------------------//
    //                      BIG INVENTORY TASKS                        //
    //-----------------------------------------------------------------//
    
    //Add Item To Inventory
    private Item[] AddItemToInventoryTask(Item item, Item[] inventory, Action<bool> callback) 
    {
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
        callback(isPlaced);
        return items.ToArray();
    }
    
    //Sort Item Slots in Inventory
    private Item[] SortItemSlotsInInventory(Item[] inventory) 
    {
        if(inventory == null) 
        {
            return null;
        }
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
                item.currSlot = slot;
                assigned.Add(slot);
                newInventory.Add(item);
                unassigned.Remove(item);
            }
        }
        return newInventory.ToArray();
    }
    
    //Remove Item From Inventory
    private Item[] RemoveItemFromInventoryTask(int id, int amount, Item[] inventory)
    {
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
            return newInventory.ToArray();
        }
        else 
        {
            return null;
        }
    }




    //-----------------------------------------------------------------//
    //                      CRAFTING FUNCTIONS                         //
    //-----------------------------------------------------------------//

    public Item GetItemBySlot(Item[] inventory, int slot) 
    {
        foreach (Item item in inventory)
        {
            if(item.currSlot == slot) 
            {
                return item;
            }
        }
        return null;
    }

    public ItemData GetItemDataById(int itemId) 
    {
        return new ItemData();
    }
 
    public bool SlotBelongsToItems(int slot) 
    {
        if(slot > 0 && slot < 34)
        {
            return true;
        }
        return false;
    }
 
    public bool SlotBelongsToArmor(int slot)
    {
        if(slot > 33 && slot < 48) 
        {
            return true; 
        }
        return false;
    }
    
    public bool SlotBelonsToStorage(int slot)
    {
        if(slot > 49) 
        {
            return true;
        }
        return false;
    }




    //-----------------------------------------------------------------//
    //                      CRAFTING FUNCTIONS                         //
    //-----------------------------------------------------------------//

    //Initiate Craft Sequence
    public Item[] CraftItem(Item[] inventory, int itemId, int amount) 
    {
        ItemData craftItem = GetItemDataById(itemId);
        if (HasRecipe(craftItem.recipe, inventory) && CanAddItem(amount * craftItem.craftAmount, itemId, inventory))
        {
            inventory = AddNewItemToInventory(inventory, craftItem, amount, returnValue => { });
            return RemoveItemsByRecipe(craftItem.recipe, amount, inventory);
        }
        return inventory;
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
        Item item = new Item();
        item.itemID = itemData.itemID;
        item.itemStack = amount;
        item.maxItemStack = itemData.maxItemStack;
        item.currSlot = 44;
        item.armorType = itemData.armorType;
        item.isPlaceable = itemData.isPlaceable;
        item.isCraftable = itemData.isCraftable;
        item.isHoldable = itemData.isHoldable;
        item.isArmor = itemData.isArmor;
        item.showInInventory = itemData.showInInventory;
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
    public Item[] RemoveItemsByRecipe(string[] recipe, int amount, Item[] inventory)
    {
        GatherAllItemDatas();
        foreach (string recipeItem in recipe)
        {
            string[] recipeData = recipeItem.Split('-');
            int itemId = Convert.ToInt32(recipeData[0]);
            int itemAmount = Convert.ToInt32(recipeData[1]);

            foreach (ItemData itemData in itemDatas)
            {
                if (itemData.itemID == itemId)
                {
                    inventory = RemoveItemFromInventoryTask(itemData.itemID, itemAmount * amount, inventory);
                    break;
                }
            }
        }
        return inventory;
    }

}

public class ItemArmorArrays 
{
    public Item[] items;
    public Item[] armor;
}