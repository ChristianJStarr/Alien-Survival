using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftingMenu : InventorySlideModule
{
    #region Singleton
    public static UI_CraftingMenu Singleton;
    private void Awake() { Singleton = this; }
    #endregion

    public GameObject slideContent, craftSlide, slideParent, typeContainer, typePrefab;
    public TextMeshProUGUI craftTip_name, craftTip_desc, craftAmount_1, craftAmount_2, craftAmount_3, craftAmount_4, craftMaxButton, craftSingleButton;
    public Image craftTip_image, craftImage_1, craftImage_2, craftImage_3, craftImage_4;
    private List<UI_CraftSlide> slides = new List<UI_CraftSlide>();
    private List<TopToolTypeSlide> typeCache = new List<TopToolTypeSlide>();
    private ItemData currentCraftItem;
    public Button craftMaxButton_b, craftSingleButton_b;
    public ItemData[] itemDatas;
    private int[] blueprints;
    private int availableCraft;
    private readonly Color onColor = new Color32(255, 255, 255, 255);
    private readonly Color offColor = new Color32(255, 255, 255, 200);
    private readonly Color nullColor = new Color32(255, 255, 255, 0);
    private int currentCraftId;

    //Button: Craft Max
    public void CraftMax()
    {
        if (currentCraftId != 0) 
        {
            PlayerInfoManager.singleton.CraftItemById(currentCraftId, availableCraft);
        }
    }

    //Button: Craft Single
    public void CraftOne()
    {
        if(currentCraftId != 0) 
        {
            PlayerInfoManager.singleton.CraftItemById(currentCraftId, 1);
        }
    }

    //Get Resources
    public void GetResources()
    {
        Inventory inventory = PlayerInfoManager.singleton.storedPlayerInfo.inventory;
        blueprints = inventory.blueprints;
        for (int i = 0; i < inventory.blueprints.Length; i++)
        {
            int itemId = inventory.blueprints[i];
            ItemData data = ItemDataManager.Singleton.GetItemData(itemId);
            if (inventory.HasRecipe(itemId, data.recipe, true))
            {
                UpdateCraftSlide(itemId, data, true);
            }
            else
            {
                UpdateCraftSlide(itemId, data, false);
            }
        }
        if(slides.Count > 0) 
        {
            if (currentCraftItem != null)
            {
                ShowTooltip(currentCraftItem);
            }
            else
            {
                ShowTooltip(slides.First().item);
            }
        }
    }
    
    //Show craft tip
    public void ShowTooltip(ItemData item)
    {
        int lowest = 6000;
        currentCraftItem = item;
        currentCraftId = item.itemId;
        craftTip_name.text = item.itemName;
        craftTip_desc.text = item.description;
        craftTip_image.sprite = item.icon;
        craftAmount_1.text = craftAmount_2.text = craftAmount_3.text = craftAmount_4.text = "";
        craftImage_1.sprite = craftImage_2.sprite = craftImage_3.sprite = craftImage_4.sprite = null;
        craftImage_1.color = craftImage_2.color = craftImage_3.color = craftImage_4.color = nullColor;
        if (item.craftAmount > 1)
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
            UpdateSlot(i, itemId, itemAmount, blueprints.Contains(item.itemId));
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





    //Check if has item
    private bool HasItem(int itemId, int itemAmount)
    {
        int available = PlayerInfoManager.singleton.storedPlayerInfo.inventory.AvailableItems(itemId);
        if (available >= itemAmount)
        {
            return true;
        }
        return false;
    }

    //Update Crafting Slide
    private void UpdateCraftSlide(int itemId, ItemData data, bool focused) 
    {
        UI_CraftSlide slide = null;
        for (int i = 0; i < slides.Count; i++)
        {
            if(slides[i].itemId == itemId) 
            {
                slide = slides[i];
                break;
            }
        }
        if (slide == null) 
        {
            slide = Instantiate(craftSlide, slideContent.transform).GetComponent<UI_CraftSlide>();
            slides.Add(slide);
        }
        slide.Craftable(focused, data);
    }
    
    //Update slot
    private void UpdateSlot(int pos, int itemId, int itemAmount, bool isCraftable)
    {

        if (pos == 0)
        {
            craftAmount_1.text = itemAmount + "";
            craftImage_1.sprite = ItemDataManager.Singleton.GetIcon(itemId);
            if (HasItem(itemId, itemAmount))
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
            craftImage_2.sprite = ItemDataManager.Singleton.GetIcon(itemId);
            if (HasItem(itemId, itemAmount))
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
            craftImage_3.sprite = ItemDataManager.Singleton.GetIcon(itemId);
            if (HasItem(itemId, itemAmount))
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
            craftImage_4.sprite = ItemDataManager.Singleton.GetIcon(itemId);
            if (HasItem(itemId, itemAmount))
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

    //How many Items
    private int HowManyItem(int itemId, int itemAmount)
    {
        int available = PlayerInfoManager.singleton.storedPlayerInfo.inventory.AvailableItems(itemId);
        if(available < itemAmount) 
        {
            return 0;
        }
        else 
        {
            return available / itemAmount;
        }
    }

    //Build Item Types
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
}
