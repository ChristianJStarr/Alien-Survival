﻿using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

public class InventoryGfx : MonoBehaviour
{
    public TopToolTipHandler toolTipHandler;
    public GameObject inventoryUI, inventoryBkg, bounds, bounds2, bounds3, hotBarButtons, tint, tint2, playerViewCamera, storageCrateSlotContainer;
    public ControlControl controls;
    public Transform itemsParent, armorSlotsContainer, hotBarParent;
    public Slider splitSlider;
    public TextMeshProUGUI splitText;
    public Item toolTipItem;
    public ItemSlot[] itemSlots, armorSlots, storageCrateSlots;
    public bool invOpen = false;
    FirstPersonController fps;
    CraftingMenu craftingMenu;
    Transform[] holds;

    private SelectedItemHandler selectedHandler;

    private Item[] items;
    private Item[] armor;
    private int[] blueprints;
    private ItemData[] allItems;


    //Inventory UI Slide Menus
    private Vector2 leftTarget = new Vector3(-354F, -41.99303F);
    private Vector2 rightTarget = new Vector3(-618, -41.99601F);

    public InventorySlideModule[] rightSlideMenus;
    public RectTransform leftSlideMenus;

    private int activeSlideMenu;
    private bool rightMove = false;
    private bool leftMove = false;
    private bool leftActive = false;
    private bool rightActive = false;


    private bool checkingHover = false;
    //Extra UI Data
    private UIData storedUIData;

    private int uiType = 0;
    
    
    private void Start()
    {

        allItems = Resources.LoadAll("Items", typeof(ItemData)).Cast<ItemData>().ToArray();
        craftingMenu = GetComponent<CraftingMenu>();
        ItemSlot[] itemSlotsTemp = itemsParent.GetComponentsInChildren<ItemSlot>(true);
        List<ItemSlot> hotBarSlotsTemp = hotBarParent.GetComponentsInChildren<ItemSlot>(true).ToList();
        armorSlots = armorSlotsContainer.GetComponentsInChildren<ItemSlot>(true);
        storageCrateSlots = storageCrateSlotContainer.GetComponentsInChildren<ItemSlot>(true);
        foreach (ItemSlot slot in itemSlotsTemp)
        {
            hotBarSlotsTemp.Add(slot);
        }
        itemSlots = hotBarSlotsTemp.ToArray();

        //Assign Slot_Numbers to Item Slots
        AssignSlotNumbersToSlots();
    }
    private void Update()
    {
        UpdateMenus();
    }


    public void Incoming(Item[] _items, Item[] _armor, int[] _blueprints) 
    {
        items = _items;
        armor = _armor;
        blueprints = _blueprints;




        if (craftingMenu == null)
        {
            craftingMenu = GetComponent<CraftingMenu>();
        }
        craftingMenu.GetResources(items, blueprints);
        UpdateUI();
        if (selectedHandler != null)
        {
            selectedHandler.UpdateSelectedSlot();
        }

    }

    //Assign Slot Numbers To Slots
    private void AssignSlotNumbersToSlots() 
    {
        int slotNumber = 1;
        for (int i = 0; i < itemSlots.Length; i++) //33 0-32
        {
            itemSlots[i].slotNumber = slotNumber; // 1-33
            slotNumber++;
        }
        for (int i = 0; i < armorSlots.Length; i++) //5 0-4
        {
            armorSlots[i].slotNumber = slotNumber; //34-38
            slotNumber++;
        }
        for (int i = 0; i < storageCrateSlots.Length; i++) //18 0-17
        {
            storageCrateSlots[i].slotNumber = slotNumber; //39-56
            slotNumber++;
        }
    }

    //Check the Hover Object
    public void CheckHoverObject()
    {
        if (HoveringOverLeftMenu())
        {
            if (!leftActive) 
            {
                ToggleSlideMenu(true);
            }
        }
        else if (HoveringOverRightMenu())
        {
            if (!rightActive) 
            {
                ToggleSlideMenu(false);
            }
        }
        else if (leftActive)
        {
            if (!checkingHover)
            {
                StartCoroutine(CheckHoverObjectWait());
            }
        }
        else if (rightActive) 
        {
            if (!checkingHover)
            {
                StartCoroutine(CheckHoverObjectWait());
            }
        }
    }

    //Wait for Check Hover Objecet
    private IEnumerator CheckHoverObjectWait() 
    {
        checkingHover = true;
        yield return new WaitForSeconds(.4F);
        if (!HoveringOverLeftMenu())
        {
            if (leftActive)
            {
                ToggleSlideMenu(true);
            }
        }
        if (!HoveringOverRightMenu())
        {
            if (rightActive)
            {
                ToggleSlideMenu(false);
            }
        }
        checkingHover = false;
    }

    //If Hovering Over Right Menu
    private bool HoveringOverRightMenu()
    {
        bool isHovering = false;
        if (rightSlideMenus != null && rightSlideMenus.Length > 0)
        {
            foreach (InventorySlideModule item in rightSlideMenus)
            {
                if (!isHovering && item.gameObject.activeSelf && item.allowActivateOnHover)
                {
                    isHovering = RectTransformUtility.RectangleContainsScreenPoint(item.rect, Input.mousePosition);
                }
            }
        }
        return isHovering;
    }

    //If Hovering Over Left Menu
    private bool HoveringOverLeftMenu()
    {
        return (RectTransformUtility.RectangleContainsScreenPoint(leftSlideMenus, Input.mousePosition));
    }

    //Button: Slide Menu
    public void ToggleSlideMenu(bool left)
    {
        if (left) 
        {
            SlideMenu(left, !leftActive);
            if (rightActive) 
            {
                SlideMenu(!left, false);
            }
        }
        else //right 
        {
            SlideMenu(left, !rightActive);
            if (leftActive)
            {
                SlideMenu(!left, false);
            }
        }
    }

    //Handover Selected Item
    public void SelectedItemHandover(SelectedItemHandler handler) 
    {
        selectedHandler = handler;
    }

    //Update Menu Positions
    private void UpdateMenus() 
    {
            if (leftMove)
            {
                if (leftSlideMenus.anchoredPosition != leftTarget)
                {
                    leftSlideMenus.anchoredPosition = Vector2.MoveTowards(leftSlideMenus.anchoredPosition, leftTarget, 6000 * Time.deltaTime);
                }
                else
                {
                    leftMove = false;
                }
            }
            if (rightMove)
            {
                RectTransform rect = rightSlideMenus[activeSlideMenu].rect;
                if (rect.anchoredPosition != rightTarget)
                {
                    rect.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition, rightTarget, 6000 * Time.deltaTime);
                }
                else
                {
                    rightMove = false;
                }
            }
            if (leftActive || rightActive)
            {
                tint2.SetActive(true);
            }
            else
            {
                tint2.SetActive(false);
            }
    }

    //Set Right Menu
    private void SetRightMenu(int menuId, bool active) 
    {
        activeSlideMenu = menuId;
        foreach (InventorySlideModule module in rightSlideMenus)
        {
            if (module.gameObject.activeSelf) 
            {
                module.gameObject.SetActive(false);
            }
        }
        rightSlideMenus[menuId].gameObject.SetActive(active);
    }

    //Slide Menus 
    private void SlideMenu(bool left, bool state) 
    {
        if (!left) //Right
        {
            InventorySlideModule module = rightSlideMenus[uiType];
            if (state) 
            {
                rightTarget = module.onPos;
            }
            else 
            {
                rightTarget = module.offPos;
            }
            rightMove = true;
            rightActive = state;
        }
        else //Left
        {
            
            if (state)
            {
                leftTarget = new Vector2(507, -38);
            }
            else
            {
                leftTarget = new Vector2(-354.1534F, -38);
            }
            leftMove = true;
            leftActive = state;
        }
    }

    //Active Top Tip (item)
    public void ActivateToolTip(Item item) 
    {
        if (!toolTipHandler.gameObject.activeSelf)
        {
            toolTipHandler.gameObject.SetActive(true);
        }
        toolTipHandler.SetData(InvUI.FindItemDataById(item.itemID), item);
    }

    //Select a slot
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

    //Button: Inventory Open/Close
    public void InvButton(string data = null) 
    {
        uiType = 0;
        UIData uiData = null;
        if (!String.IsNullOrEmpty(data))
        {
            uiData = JsonUtility.FromJson<UIData>(data);
            if (uiData != null)
            {
                uiType = uiData.type;
                invOpen = false;
            }
        }



        SetRightMenu(uiType, !invOpen);
        activeSlideMenu = uiType;
        if (invOpen)
        {
            invOpen = false;
            controls.Show();
            toolTipHandler.gameObject.SetActive(false);
            SlideMenu(true, false);
            SlideMenu(false, false);
            tint.SetActive(false);
            if(playerViewCamera != null) 
            {
                playerViewCamera.SetActive(false);
            }
        }
        else
        {
            controls.Hide();
            invOpen = true;
            
            if (playerViewCamera != null)
            {
                playerViewCamera.SetActive(true);
            }
        }
        tint.SetActive(invOpen);
        hotBarButtons.SetActive(!invOpen);
        inventoryBkg.SetActive(invOpen);
        leftSlideMenus.gameObject.SetActive(invOpen);
        
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

        //Show StorageCrate
        if(uiType == 1) 
        {
            OpenStorageCrateUI(uiData);   
        }
        //Show Smelting
        if(uiType == 2)
        {
            OpenSmeltUI(uiData);
        }
    }

    //Close the Inventory if OPEN
    public void CloseInventory() 
    {
        if (invOpen) 
        {
            InvButton();
        }
    }

    //Open the Inventory if CLOSED
    public void OpenInventory() 
    {
        if (!invOpen) 
        {
            InvButton();
        }
    }


    //Update Extra UI Data
    public void UpdateExtraUIData(string data)
    {
        UIData uiData = JsonUtility.FromJson<UIData>(data);

        if(uiData.type == 1) //Storage
        {
            UpdateStorageCrateUI(uiData.itemArray);
        }
        else if (uiData.type == 2) //Smelt
        {
            UpdateSmeltUI(uiData);
        }
        else if (uiData.type == 3)
        {

        }
    }

    //Open UI Storage Crate
    public void OpenStorageCrateUI(UIData crateData) 
    {
        ToggleSlideMenu(false);
        UpdateStorageCrateUI(crateData.itemArray);
    }

    //Update UI Storage Crate
    private void UpdateStorageCrateUI(Item[] datas)
    {
        InvUI.ClearSlots(storageCrateSlots);
        if (datas != null)
        {
            InvUI.SortSlots(datas, storageCrateSlots);
        }
    }

    //Open UI Smelting
    private void OpenSmeltUI(UIData smeltData) 
    {
        
    }
   
    //Update UI Smelting
    private void UpdateSmeltUI(UIData smeltData) 
    {
    
    }

    //Update the Standard Inventory UI
    private void UpdateUI()
    {
        //Inventory Slots
        InvUI.ClearSlots(itemSlots);
        if (items != null)
        {
            InvUI.SortSlots(items, itemSlots);
        }

        //Armor Slots
        InvUI.ClearSlots(armorSlots);
        if (armor != null) 
        {
            InvUI.SortSlots(armor, armorSlots);
        }
    }

}


public class InvUI
{
    private static ItemData[] itemDatas;

    //TOOL: Sort Slots
    public static void SortSlots(Item[] items, ItemSlot[] slots)
    {
        foreach (Item item in items)
        {
            foreach (ItemSlot slot in slots)
            {
                if (item.currSlot == slot.slotNumber)
                {
                    ItemData data = FindItemDataById(item.itemID);
                    if (data != null)
                    {
                        slot.AddItem(item, data);
                    }
                    break;
                }
            }
        }
    }


    //TOOL: Get Item Data from ID
    public static ItemData FindItemDataById(int id)
    {
        if (CheckItemDataList())
        {
            ItemData itemData = null;
            foreach (ItemData data in itemDatas)
            {
                if (data.itemID == id)
                {
                    itemData = data;
                    break;
                }
            }
            return itemData;
        }
        else
        {
            return null;
        }
    }

    //TOOL: Get All Item Datas
    public static ItemData[] GetAllItemDatas() 
    {
        if (CheckItemDataList())
        {
            return itemDatas;
        }
        return null;
    }

    //Check and Initialize Item Data List
    private static bool CheckItemDataList() 
    {
        if (itemDatas == null)
        {
            itemDatas = Resources.LoadAll("Items", typeof(ItemData)).Cast<ItemData>().ToArray();
            if(itemDatas != null) 
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        else 
        {
            return true;
        }
    }


    //TOOL: Clear Slots
    public static void ClearSlots(ItemSlot[] slots) 
    {
        if(slots != null && slots.Length > 0) 
        {
            foreach (ItemSlot slot in slots)
            {
                slot.ClearSlot();
            }
        }
    }

}






/// <summary>
/// *
/// </summary>

public class UIData 
{
    public int type = 0;
    public Item[] itemArray = null;
}