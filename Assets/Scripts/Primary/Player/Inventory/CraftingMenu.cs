using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenu : MonoBehaviour
{
    public GameObject itemToolTip, splitOptions, craftOptions, slideContent, craftSlide, slideParent;
    public TextMeshProUGUI craftTip_name, craftTip_desc, craftAmount_1, craftAmount_2, craftAmount_3, craftAmount_4, craftAmount_5, craftMaxButton;
    public Image craftTip_image, craftImage_1, craftImage_2, craftImage_3, craftImage_4, craftImage_5;

    private List<CraftSlide> slideCache;
    private List<InventoryResource> inv;
    private ItemData[] craftDatas;
    private ItemData currentCraftItem;
    private int lowest = 6000;
    

    private readonly Color onColor = new Color32(255, 255, 255, 255);
    private readonly Color offColor = new Color32(255, 255, 255, 200);
    private readonly Color nullColor = new Color32(255, 255, 255, 0);


    void Start()
    {
        craftDatas = Resources.LoadAll("Items", typeof(ItemData)).Cast<ItemData>().ToArray();
        List<ItemData> tempDatas = new List<ItemData>();
        inv = new List<InventoryResource>();
        slideCache = new List<CraftSlide>();
        foreach (ItemData itemData in craftDatas)
        {
            if (itemData.isCraftable) 
            {
                tempDatas.Add(itemData);
                CraftSlide slide = Instantiate(craftSlide, slideContent.transform).GetComponent<CraftSlide>();
                slide.Craftable(false, itemData, this);
                slideCache.Add(slide);
            }
        }
        craftDatas = tempDatas.ToArray();
    }


    public void GetResources(Item[] inventory, int[] blueprints)
    {
        if(inventory != null) 
        {
            foreach (Item item in inventory)
            {
                bool placed = false;
                foreach (InventoryResource invItem in inv)
                {
                    if (invItem.itemId == item.itemID)
                    {
                        invItem.itemAmount += item.itemStack;
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
                    newRes.itemId = item.itemID;
                    newRes.itemAmount = item.itemStack;
                    inv.Add(newRes);
                }
            }
            foreach (ItemData item in craftDatas)
            {
                int recipeAmount = item.recipe.Length;
                int recipeAvail = 0;
                foreach (string recipe in item.recipe)
                {
                    string[] data = recipe.Split('-');
                    int itemId = Convert.ToInt32(data[0]);
                    int itemAmount = Convert.ToInt32(data[1]);
                    if (HasItem(itemId, itemAmount, inv))
                    {
                        recipeAvail++;
                    }
                }
                if (recipeAvail == recipeAmount && blueprints.ToList().Contains(item.itemID))
                {
                    FindSlideAndSet(true, item);
                }
                else
                {
                    FindSlideAndSet(false, item);
                }
            }
        }
    }


    private void FindSlideAndSet(bool value, ItemData itemData) 
    {
        foreach (CraftSlide slide in slideCache)
        {
            if(slide.item == itemData && slide.craftable != value) 
            {
                slide.Craftable(value, itemData, this);
                break;
            }
        }
    }


    public bool HasItem(int itemId, int itemAmount, List<InventoryResource> inv)
    {
        bool hasItem = false;
        foreach (InventoryResource item in inv)
        {
            if (item.itemId == itemId && item.itemAmount >= itemAmount)
            {
                hasItem = true;
                break;
            }
        }
        return hasItem;
    }


    public void ShowTooltip(ItemData item)
    {
        currentCraftItem = item;
        bool isCraftable = false;
        foreach (CraftSlide slide in slideCache)
        {
            if (item.itemID == slide.item.itemID)
            {
                isCraftable = true;
                break;
            }
        }
        itemToolTip.SetActive(true);
        craftOptions.SetActive(true);
        splitOptions.SetActive(false);
        craftTip_name.text = item.name;
        craftTip_desc.text = item.description;
        craftTip_image.sprite = item.icon;
        craftAmount_1.text = craftAmount_2.text = craftAmount_3.text = craftAmount_4.text = craftAmount_5.text = "";
        craftImage_1.sprite = craftImage_2.sprite = craftImage_3.sprite = craftImage_4.sprite = craftImage_5.sprite = null;
        craftImage_1.color = craftImage_2.color = craftImage_3.color = craftImage_4.color = craftImage_5.color = nullColor;

        int craftAmount = item.recipe.Length;
        List<int> canAmount = new List<int>();
        for (int i = 0; i < item.recipe.Length; i++)
        {
            String[] recipe = item.recipe[i].Split('-');
            int itemId = Convert.ToInt32(recipe[0]);
            int itemAmount = Convert.ToInt32(recipe[1]);
            UpdateSlot(i, itemId, itemAmount, isCraftable);
            canAmount.Add(HowManyItem(itemId, itemAmount));
        }

        foreach (int amount in canAmount)
        {
            if (amount < lowest)
            {
                lowest = amount;
            }
        }
        craftMaxButton.text = "CRAFT " + lowest;
    }
    

    public void UpdateSlot(int pos, int itemId, int itemAmount, bool isCraftable)
    {

        if (pos == 0)
        {
            craftAmount_1.text = itemAmount + "";
            craftImage_1.sprite = GetImage(itemId);
            if (HasItem(itemId, itemAmount, inv))
            {
                craftImage_1.color = onColor;
                craftAmount_1.color = onColor;
            }
            else
            {
                craftImage_1.color = offColor;
                craftAmount_1.color = offColor;
            }
        }
        if (pos == 1)
        {
            craftAmount_2.text = itemAmount + "";
            craftImage_2.sprite = GetImage(itemId);
            if (HasItem(itemId, itemAmount, inv))
            {
                craftImage_2.color = onColor;
                craftAmount_2.color = onColor;
            }
            else
            {
                craftImage_2.color = offColor;
                craftAmount_2.color = offColor;
            }
        }
        if (pos == 2)
        {
            craftAmount_3.text = itemAmount + "";
            craftImage_3.sprite = GetImage(itemId);
            if (HasItem(itemId, itemAmount, inv))
            {
                craftImage_3.color = onColor;
                craftAmount_3.color = onColor;
            }
            else
            {
                craftImage_3.color = offColor;
                craftAmount_3.color = offColor;
            }
        }
        if (pos == 3)
        {
            craftAmount_4.text = itemAmount + "";
            craftImage_4.sprite = GetImage(itemId);
            if (HasItem(itemId, itemAmount, inv))
            {
                craftImage_4.color = onColor;
                craftAmount_4.color = onColor;
            }
            else
            {
                craftImage_4.color = offColor;
                craftAmount_4.color = offColor;
            }
        }
        if (pos == 4)
        {
            craftAmount_5.text = itemAmount + "";
            craftImage_5.sprite = GetImage(itemId);
            if (HasItem(itemId, itemAmount, inv))
            {
                craftImage_5.color = onColor;
                craftAmount_5.color = onColor;
            }
            else
            {
                craftImage_5.color = offColor;
                craftAmount_5.color = offColor;
            }
        }
    }


    public Sprite GetImage(int id)
    {
        Sprite image = null;
        foreach (ItemData item in craftDatas)
        {
            if (item.itemID == id)
            {
                image = item.icon;
                break;
            }
        }
        return image;
    }
    

    public int HowManyItem(int itemId, int itemAmount)
    {
        int howMany = 0;
        foreach (InventoryResource item in inv)
        {
            if (item.itemId == itemId && item.itemAmount >= itemAmount)
            {
                howMany = item.itemAmount / itemAmount;
                break;
            }
        }
        return howMany;
    }





    //public void CraftMax()
    //{
    //    int space = inventory.space - inventory.items.Count;
    //    if (inventory.space >= lowest / currentCraftItem.maxItemStack)
    //    {
    //        if (lowest != 0 && lowest != 6000)
    //        {
    //            for (int i = 0; i < currentCraftItem.recipe.Length; i++)
    //            {
    //                String[] recipe = currentCraftItem.recipe[i].Split('-');
    //                int itemId = Convert.ToInt32(recipe[0]);
    //                int itemAmount = Convert.ToInt32(recipe[1]);
    //                RemoveItem(itemId, itemAmount * lowest);
    //            }
    //            lowest *= currentCraftItem.craftAmount;
    //            if (lowest > currentCraftItem.maxItemStack)
    //            {
    //                lowest -= currentCraftItem.maxItemStack;
    //                AddItem(currentCraftItem, currentCraftItem.maxItemStack);
    //                if (lowest > currentCraftItem.maxItemStack)
    //                {
    //                    lowest -= currentCraftItem.maxItemStack;
    //                    AddItem(currentCraftItem, currentCraftItem.maxItemStack);
    //                    AddItem(currentCraftItem, lowest);
    //                }
    //                else
    //                {
    //                    AddItem(currentCraftItem, lowest);
    //                }
    //            }
    //            else
    //            {
    //                AddItem(currentCraftItem, lowest);
    //            }
    //        }
    //    }
    //}

    //public void CraftOne()
    //{
    //    int space = inventory.space - inventory.items.Count;
    //    if (inventory.space >= lowest / currentCraftItem.maxItemStack)
    //    {
    //        if (lowest != 0 && lowest != 6000)
    //        {
    //            for (int i = 0; i < currentCraftItem.recipe.Length; i++)
    //            {
    //                String[] recipe = currentCraftItem.recipe[i].Split('-');
    //                int itemId = Convert.ToInt32(recipe[0]);
    //                int itemAmount = Convert.ToInt32(recipe[1]);
    //                RemoveItem(itemId, itemAmount);
    //            }
    //            AddItem(currentCraftItem, currentCraftItem.craftAmount);
    //        }
    //    }
    //}

    //public void AddItem(Item item, int amount)
    //{
    //    Item newItem = Instantiate(item);
    //    newItem.itemStack = amount;
    //    newItem.currSlot = 44;
    //    inventory.Add(newItem);
    //}

    //public bool RemoveItem(int itemID, int amount)
    //{
    //    if (HasItem(itemID, amount))
    //    {
    //        foreach (Item stored in inventory.items.ToList())
    //        {
    //            if (itemID == stored.itemID && amount != 0)
    //            {
    //                if (stored.itemStack > amount)
    //                {
    //                    stored.itemStack -= amount;
    //                    amount = 0;
    //                    break;
    //                }
    //                else if (stored.itemStack == amount)
    //                {
    //                    inventory.Remove(stored);
    //                    amount = 0;
    //                    break;
    //                }
    //                else if (stored.itemStack < amount)
    //                {
    //                    amount -= stored.itemStack;
    //                    inventory.Remove(stored);
    //                }
    //            }
    //        }
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }

    //}

}
