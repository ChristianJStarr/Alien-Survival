using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingMenu : MonoBehaviour
{
    public GameObject slideContent, craftSlide, slideParent, typeContainer, typePrefab;
    public TextMeshProUGUI craftTip_name, craftTip_desc, craftAmount_1, craftAmount_2, craftAmount_3, craftAmount_4, craftMaxButton, craftSingleButton;
    public Image craftTip_image, craftImage_1, craftImage_2, craftImage_3, craftImage_4;
    private List<CraftSlide> slideCache;
    private List<InventoryResource> inv;
    private List<TopToolTypeSlide> typeCache;
    private ItemData[] craftDatas;
    private ItemData currentCraftItem;
    
    public Button craftMaxButton_b, craftSingleButton_b;

    private readonly Color onColor = new Color32(255, 255, 255, 255);
    private readonly Color offColor = new Color32(255, 255, 255, 200);
    private readonly Color nullColor = new Color32(255, 255, 255, 0);

    private int availableCraft;

    void Start()
    {
        craftDatas = Resources.LoadAll("Items", typeof(ItemData)).Cast<ItemData>().ToArray();
        List<ItemData> tempDatas = new List<ItemData>();
        inv = new List<InventoryResource>();
        typeCache = new List<TopToolTypeSlide>();
        slideCache = new List<CraftSlide>();
        foreach (ItemData itemData in craftDatas)
        {
            if (itemData.isCraftable) 
            {
                CraftSlide slide = Instantiate(craftSlide, slideContent.transform).GetComponent<CraftSlide>();
                slide.Craftable(false, itemData, this);
                slideCache.Add(slide);
            }
        }
        ShowTooltip(slideCache.First().item);
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
                if (recipeAvail == recipeAmount)
                {
                    if(blueprints == null) 
                    {
                        FindSlideAndSet(true, item);
                    }
                    else if(blueprints.ToList().Contains(item.itemID)) 
                    {
                        FindSlideAndSet(true, item);
                    }
                }
                else
                {
                    FindSlideAndSet(false, item);
                }
            }
            if (currentCraftItem != null)
            {
                ShowTooltip(currentCraftItem);
            }
            else
            {
                ShowTooltip(slideCache.First().item);
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
        int lowest = 6000;
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
        craftTip_name.text = item.name;
        craftTip_desc.text = item.description;
        craftTip_image.sprite = item.icon;
        craftAmount_1.text = craftAmount_2.text = craftAmount_3.text = craftAmount_4.text = "";
        craftImage_1.sprite = craftImage_2.sprite = craftImage_3.sprite = craftImage_4.sprite = null;
        craftImage_1.color = craftImage_2.color = craftImage_3.color = craftImage_4.color = nullColor;
        if(item.craftAmount > 1) 
        {
            craftSingleButton.text = "CRAFT " + item.craftAmount;
        }
        else 
        {
            craftSingleButton.text = "CRAFT ONE";
        }
        int craftAmount = item.recipe.Length;
        List<int> canAmount = new List<int>();
        for (int i = 0; i < item.recipe.Length; i++)
        {
            string[] recipe = item.recipe[i].Split('-');
            int itemId = Convert.ToInt32(recipe[0]);
            int itemAmount = Convert.ToInt32(recipe[1]);
            UpdateSlot(i, itemId, itemAmount, isCraftable);
            canAmount.Add(HowManyItem(itemId, itemAmount));
        }
        BuildTypes(item.itemUse);
        foreach (int amount in canAmount)
        {
            if (amount < lowest)
            {
                lowest = amount;
            }
        }
        availableCraft = lowest;
        if (lowest > 1)
        {
            craftSingleButton_b.interactable = true;
            craftMaxButton_b.interactable = true;
            craftMaxButton.text = "CRAFT " + lowest * item.craftAmount;
        }
        else
        {
            craftMaxButton.text = "CRAFT MAX";
            craftSingleButton_b.interactable = true;
            craftMaxButton_b.interactable = false;
            if (lowest == 0) 
            {
                craftSingleButton_b.interactable = false;
            }
        }
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
        foreach (InventoryResource item in inv.ToList())
        {
            if (item.itemId == itemId && item.itemAmount >= itemAmount)
            {
                howMany = item.itemAmount / itemAmount;
                break;
            }
        }
        return howMany;
    }

    private void BuildTypes(string[] types)
    {
        if (typeCache != null)
        {
            if (typeCache.Count > 0)
            {
                foreach (TopToolTypeSlide slide in typeCache.ToList())
                {
                    Destroy(slide.gameObject);
                    typeCache.Remove(slide);
                }
            }
        }
        if (types != null)
        {
            foreach (string item in types)
            {
                string[] type = item.Split('-');
                int item_type = Convert.ToInt32(type[0]);
                int type_amount = Convert.ToInt32(type[1]);
                TopToolTypeSlide slide = Instantiate(typePrefab, typeContainer.transform).GetComponent<TopToolTypeSlide>();
                slide.UpdateType(item_type, type_amount);
                typeCache.Add(slide);
            }
        }

    }

    public void CraftMax()
    {
        PlayerInfoManager.singleton.CraftItemById(currentCraftItem.itemID, availableCraft);
    }

    public void CraftOne()
    {
        PlayerInfoManager.singleton.CraftItemById(currentCraftItem.itemID, 1);
    }

}
