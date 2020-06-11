using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;
using System.IO;
using System;
using System.Linq;

public class InventoryGfx : MonoBehaviour
{

    public GameObject inventoryUI, inventoryBkg, playerMenu, toolTipMenu, craftMenu, bounds, bounds2, bounds3, hotBarButtons;
    public ControlControl controls;
    public Transform itemsParent, armorSlotsContainer;
    public Slider splitSlider;
    public TextMeshProUGUI splitText;
    public Item toolTipItem;
    public ItemSlot[] itemSlots, armorSlots;
    public bool invOpen = false;
    FirstPersonController fps;
    CraftingMenu craftingMenu;
    Transform[] holds;

    private Item[] items;
    private Item[] armor;
    private int[] blueprints;
    private ItemData[] allItems;

    public void Incoming(PlayerInfo playerInfo)
    {
        items = playerInfo.items;
        armor = playerInfo.armor;
        blueprints = playerInfo.blueprints;
        craftingMenu.GetResources(items, blueprints);
        UpdateUI();
    }

    void Start()
    {
        allItems = Resources.LoadAll("Items", typeof(ItemData)).Cast<ItemData>().ToArray();
        craftingMenu = GetComponent<CraftingMenu>();
        itemSlots = itemsParent.GetComponentsInChildren<ItemSlot>(true);
        armorSlots = armorSlotsContainer.GetComponentsInChildren<ItemSlot>(true);
        craftingMenu = GetComponent<CraftingMenu>();
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].slotNumber = i + 1;
        }
        for (int i = 0; i < armorSlots.Length; i++)
        {
            armorSlots[i].slotNumber = i + 34;
        }
    }

    public Item SelectSlot(int slot) 
    {
        Item item = null;
        for (int i = 0; i < 6; i++)
        {
            if (itemSlots[i].slotNumber == slot) 
            {
                itemSlots[i].Selected(true);
                item = itemSlots[i].item;
            }
            else 
            {
                itemSlots[i].Selected(false);
            }
        }
        return item;
    }

    public void InvButton() 
    {
        if (invOpen)
        {
            invOpen = false;
            toolTipMenu.SetActive(false);
            controls.Show();
        }
        else
        {
            controls.Hide();
            invOpen = true;
        }
        hotBarButtons.SetActive(!invOpen);
        inventoryBkg.SetActive(invOpen);
        playerMenu.SetActive(invOpen);
        craftMenu.SetActive(invOpen);
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i > 5)
            {
                itemSlots[i].Toggle();
            }
        }
        for (int i = 0; i < armorSlots.Length; i++)
        {
            armorSlots[i].Toggle();  
        }
    }

    private void UpdateUI()
    {
        foreach (ItemSlot slot in itemSlots)
        {
            slot.ClearSlot();
        }
        foreach (ItemSlot slot in armorSlots)
        {
            slot.ClearSlot();
        }

        if (items != null)
        {
            foreach (Item item in items)
            {
                foreach (ItemSlot slot in itemSlots)
                {
                    if (item.currSlot == slot.slotNumber)
                    {
                        ItemData data = FindItemData(item.itemID);
                        if (data != null)
                        {
                            slot.AddItem(item, FindItemData(item.itemID));
                        }
                        break;
                    }
                }
            }
        }
        if (armor != null) 
        {
            foreach (Item item in armor)
            {
                foreach (ItemSlot slot in armorSlots)
                {
                    if (item.currSlot == slot.slotNumber)
                    {
                        ItemData data = FindItemData(item.itemID);
                        if(data != null)
                        {
                            slot.AddItem(item, FindItemData(item.itemID));
                        }
                        break;
                    }
                }
            }
        }
    }

    public ItemData FindItemData(int id) 
    {
        ItemData itemData = null;
        foreach (ItemData data in allItems) 
        {
            if(data.itemID == id) 
            {
                itemData = data;
                break;
            }   
        }
        return itemData;
    }
}