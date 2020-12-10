using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Collections;

public class UI_Inventory : MonoBehaviour
{
    public UI_Tooltip ui_tooltip;
    public GameObject inventoryUI, inventoryBkg, bounds, bounds2, bounds3, hotBarButtons, tint, tint2, playerViewCamera, storageCrateSlotContainer;
    public ControlControl controls;
    public Transform itemsParent, armorSlotsContainer, hotBarParent;
    public Image buttonIcon;
    public Item toolTipItem;
    public UI_ItemSlot[] itemSlots, armorSlots, storageCrateSlots;
    public bool invOpen = false;
    public UI_CraftingMenu craftingMenu;
    
    private Item[] items;
    private Item[] armor;
    private int[] blueprints;


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

    public Sprite closeIcon;
    private Sprite normalIcon;


    private void Start()
    {
        normalIcon = buttonIcon.sprite;
        craftingMenu = GetComponent<UI_CraftingMenu>();
        UI_ItemSlot[] itemSlotsTemp = itemsParent.GetComponentsInChildren<UI_ItemSlot>(true);
        List<UI_ItemSlot> hotBarSlotsTemp = hotBarParent.GetComponentsInChildren<UI_ItemSlot>(true).ToList();
        armorSlots = armorSlotsContainer.GetComponentsInChildren<UI_ItemSlot>(true);
        storageCrateSlots = storageCrateSlotContainer.GetComponentsInChildren<UI_ItemSlot>(true);
        foreach (UI_ItemSlot slot in itemSlotsTemp)
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



    //Button: Inventory Open/Close
    public void InvButton()
    {
        invOpen = !invOpen;
        if (playerViewCamera != null) { playerViewCamera.SetActive(invOpen); }
        if (!invOpen)
        {
            controls.Show();
            ui_tooltip.gameObject.SetActive(false);
            SlideMenu(true, false);
            SlideMenu(false, false);
            buttonIcon.sprite = normalIcon;
        }
        else
        {
            controls.Hide();
            buttonIcon.sprite = closeIcon;
            SetRightMenu(0, true);
        }
        tint.SetActive(invOpen);
        hotBarButtons.SetActive(!invOpen);
        inventoryBkg.SetActive(invOpen);
        leftSlideMenus.gameObject.SetActive(invOpen);
        //Turn off item slots, but not hotbar slots
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i > 5)
            {
                itemSlots[i].Toggle();
            }
        }
        //Turn off armor slots
        for (int i = 0; i < armorSlots.Length; i++)
        {
            armorSlots[i].Toggle();
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
    public void OpenInventory(int uiType, UIData data = null)
    {
        if (data != null)
        {
            storedUIData = data;
        }
        SetRightMenu(uiType, true);
        if (!invOpen)
        {
            invOpen = !invOpen;
            if (playerViewCamera != null) { playerViewCamera.SetActive(invOpen); }
            controls.Hide();
            buttonIcon.sprite = closeIcon;
            SetRightMenu(activeSlideMenu, true);
            tint.SetActive(invOpen);
            hotBarButtons.SetActive(!invOpen);
            inventoryBkg.SetActive(invOpen);
            leftSlideMenus.gameObject.SetActive(invOpen);
            //Turn off item slots, but not hotbar slots
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (i > 5)
                {
                    itemSlots[i].Toggle();
                }
            }
            //Turn off armor slots
            for (int i = 0; i < armorSlots.Length; i++)
            {
                armorSlots[i].Toggle();
            }
        }
    }




    //Incoming Inventory Data from Server
    public void Incoming(Item[] _items, Item[] _armor, int[] _blueprints) 
    {
        items = _items;
        armor = _armor;
        blueprints = _blueprints;
        if (craftingMenu == null)
        {
            craftingMenu = GetComponent<UI_CraftingMenu>();
        }
        craftingMenu.GetResources(items, blueprints);
        UpdateUI();
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
        if(activeSlideMenu != menuId) 
        {
            activeSlideMenu = menuId;
            for (int i = 0; i < rightSlideMenus.Length; i++)
            {
                if (i != menuId)
                {
                    rightSlideMenus[menuId].gameObject.SetActive(false);
                }
            }
            rightSlideMenus[menuId].gameObject.SetActive(active);
            rightSlideMenus[menuId].storedUIData = storedUIData;
        }
        else if (active && rightSlideMenus[menuId].needsUIData)
        {
            rightSlideMenus[menuId].storedUIData = storedUIData;
        }
    }

    //Slide Menus 
    private void SlideMenu(bool left, bool state) 
    {
        if (!left) //Right
        {
            InventorySlideModule module = rightSlideMenus[activeSlideMenu];
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
        ui_tooltip.SetData(ItemDataManager.Singleton.GetItemData(item.itemID), item);
    }
    
    //Hide Top Tip
    public void HideTooltip(int slot) 
    {
        ui_tooltip.Hide(slot);
    }


    //Set Slot to Focused
    public void SetSlotFocused(int slot) 
    {
        for (int i = 0; i < 6; i++)
        {
            itemSlots[i].Selected(itemSlots[i].slotNumber == slot);
        }
    }

    //Get Item from Slot
    public Item GetItemFromSlot(int slot) 
    {
        for (int i = 0; i < items.Length; i++)
        {
            if(items[i].currSlot == slot) 
            {
                return items[i];
            }
        }
        return null;
    }

    //Update the Standard Inventory UI
    private void UpdateUI()
    {
        //Inventory Slots
        ClientInventoryTool.ClearSlots(itemSlots);
        if (items != null)
        {
            ClientInventoryTool.SortSlots(items, itemSlots);
        }

        //Armor Slots
        ClientInventoryTool.ClearSlots(armorSlots);
        if (armor != null) 
        {
            ClientInventoryTool.SortSlots(armor, armorSlots);
        }
    }




    //Interface Hider Show/Hide
    public void Show() 
    {
        inventoryUI.SetActive(true);
    }
    public void Hide() 
    {
        inventoryUI.SetActive(false);
    }
}


public class ClientInventoryTool
{
    //TOOL: Sort Slots
    public static void SortSlots(Item[] items, UI_ItemSlot[] slots)
    {
        for (int i = 0; i < items.Length; i++)
        {
            for (int e = 0; e < slots.Length; e++)
            {
                if (items[i].currSlot == slots[e].slotNumber)
                {
                    ItemData data = ItemDataManager.Singleton.GetItemData(items[i].itemID);
                    if (data != null)
                    {
                        slots[e].AddItem(items[i], data);
                    }
                    break;
                }
            }
        }
    }

    //TOOL: Clear Slots
    public static void ClearSlots(UI_ItemSlot[] slots) 
    {
        if(slots != null && slots.Length > 0) 
        {
            foreach (UI_ItemSlot slot in slots)
            {
                slot.ClearSlot();
            }
        }
    }

}


public class UIData 
{
    public int type = 0;
    public Item[] itemArray = null;
}