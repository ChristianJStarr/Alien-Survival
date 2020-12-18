using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class UI_Inventory : MonoBehaviour
{
    #region Singleton
    public static UI_Inventory Singleton;
    private void Awake() { Singleton = this; }
    #endregion
    public UI_Tooltip ui_tooltip;
    public GameObject inventoryUI, inventoryBkg, bounds, bounds2, bounds3, hotBarButtons, tint, tint2, playerViewCamera, storageCrateSlotContainer;
    public ControlsManager controls;
    public Transform itemsParent, armorSlotsContainer, hotBarParent;
    public Image buttonIcon;
    public Item toolTipItem;
    public UI_ItemSlot[] itemSlots, armorSlots, storageCrateSlots;
    public bool invOpen = false;
    public UI_CraftingMenu craftingMenu;

    //Inventory UI Slide Menus
    private Vector2 leftTarget = new Vector3(-354F, -41.99303F);
    private Vector2 rightTarget = new Vector3(-618, -41.99601F);

    public InventorySlideModule[] rightSlideMenus;
    public RectTransform leftSlideMenus;

    private int activeSlideMenu = 100;
    private bool rightMove = false;
    private bool leftMove = false;
    private bool leftActive = false;
    private bool rightActive = false;


    private bool checkingHover = false;
    //Extra UI Data
    private UIData storedUIData;

    public Sprite closeIcon;
    private Sprite normalIcon;


    //Configuration
    private float c_SlideLerpSpeed = 0.5F; //Seconds


    private void Start()
    {
        normalIcon = buttonIcon.sprite;
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
            MoveSideMenu(true, false);
            MoveSideMenu(false, false);
            buttonIcon.sprite = normalIcon;
            rightSlideMenus[activeSlideMenu].gameObject.SetActive(false);
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
        }
    }

    //Incoming Inventory Data from Server
    public void Incoming()
    {
        UpdateCraftingMenu();
        UpdateUI();
    }

    //Update Crafting Menu
    private void UpdateCraftingMenu()
    {
        if (craftingMenu == null)
        {
            craftingMenu = UI_CraftingMenu.Singleton;
        }
        craftingMenu.GetResources();
    }

    //Assign Slot Numbers To Slots
    private void FindAndRegisterSlots()
    {
        UI_ItemSlot[] hotbar = hotBarParent.GetComponentsInChildren<UI_ItemSlot>(true);
        UI_ItemSlot[] primary = itemsParent.GetComponentsInChildren<UI_ItemSlot>(true);
        UI_ItemSlot[] armor = armorSlotsContainer.GetComponentsInChildren<UI_ItemSlot>(true);
        int slotNumber = 1;
        List<UI_ItemSlot> temp = new List<UI_ItemSlot>();
        temp = hotbar.ToList();
        for (int i = 0; i < primary.Length; i++)
        {
            temp.Add(primary[i]);
        }
        for (int i = 0; i < armor.Length; i++)
        {
            temp.Add(armor[i]);
        }
        for (int i = 0; i < temp.Count; i++)
        {
            temp[i].slotNumber = slotNumber;
            slotNumber++;
        }
        itemSlots = temp.ToArray();
    }

    //Check the Hover Object
    public void CheckHoverObject()
    {
        if (HoveringOverLeftMenu())
        {
            if (!leftActive)
            {
                ToggleSideMenu(true);
            }
        }
        else if (HoveringOverRightMenu())
        {
            if (!rightActive)
            {
                ToggleSideMenu(false);
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
        yield return new WaitForSeconds(.2F);
        if (!HoveringOverLeftMenu())
        {
            if (leftActive)
            {
                ToggleSideMenu(true);
            }
        }
        if (!HoveringOverRightMenu())
        {
            if (rightActive)
            {
                ToggleSideMenu(false);
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

    
    //--------------------------//
    //  - Top Tool Tip          //
    //--------------------------//

    //Active Top Tip (item)
    public void ActivateToolTip(Item item)
    {
        ItemData data = ItemDataManager.Singleton.GetItemData(item.itemId);
        if (data != null)
        {
            ui_tooltip.SetData(data, item);
        }
        else
        {
            Debug.LogError("ItemDataManager has Failed. Data with Id: " + item.itemId + " doesn't exist.");
        }
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

    //Update the Standard Inventory UI
    private void UpdateUI()
    {
        Inventory inventory = PlayerInfoManager.singleton.storedPlayerInfo.inventory;
        int inv_length = inventory.items.Count;
        int sl_length = itemSlots.Length;
        List<int> modified_slots = new List<int>();

        //Find Item Slots
        if (sl_length == 0)
        {
            FindAndRegisterSlots();
            sl_length = itemSlots.Length;
        }

        //Apply Data to Slots
        for (int i = 0; i < inv_length; i++)
        {
            Item item = inventory.items[i];
            int index = item.currSlot - 1;
            if(itemSlots[index] != null) 
            {
                itemSlots[index].Incoming(item);
                modified_slots.Add(index);
            }
        }

        //Clear Unmodified Slots
        for (int i = 0; i < sl_length; i++)
        {
            if (!modified_slots.Contains(i)) 
            {
                itemSlots[i].Clear();
            }
        }
    }


    //--------------------------//
    //  - Side Menus            //
    //--------------------------//


    private void SetRightMenu(int menuId, bool active)
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
    public void ToggleSideMenu(bool left)
    {
        if (left)
        {
            MoveSideMenu(left, !leftActive);
            if (rightActive)
            {
                MoveSideMenu(!left, false);
            }
        }
        else
        {
            MoveSideMenu(left, !rightActive);
            if (leftActive)
            {
                MoveSideMenu(!left, false);
            }
        }
    }
    private void MoveSideMenu(bool left, bool state)
    {
        if (left && leftActive != state)
        {
            StartCoroutine(MoveSideLerp(left, state, c_SlideLerpSpeed));
        }
        else if (!left && rightActive != state)
        {
            StartCoroutine(MoveSideLerp(left, state, c_SlideLerpSpeed));
        }
    }
    private IEnumerator MoveSideLerp(bool left, bool state, float duration) 
    {
        Vector2 target;
        if (left)
        {
            leftActive = state;

            if (state)
            {
                target = new Vector2(507, -38);
            }
            else
            {
                target = new Vector2(-354.1534F, -38);
            }
            float time = 0;
            Vector2 startPosition = leftSlideMenus.anchoredPosition;
            while (time < duration && leftActive == state)
            {
                leftSlideMenus.anchoredPosition = Vector2.Lerp(startPosition, target, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            if (leftActive == state)
            {
                leftSlideMenus.anchoredPosition = target;
            }
        }
        else
        {
            rightActive = state;
            InventorySlideModule module = rightSlideMenus[activeSlideMenu];
            if (state)
            {
                target = module.onPos;
            }
            else
            {
                target = module.offPos;
            }
            float time = 0;
            Vector2 startPosition = module.rect.anchoredPosition;
            while (time < duration && rightActive == state)
            {
                module.rect.anchoredPosition = Vector2.Lerp(startPosition, target, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            if (rightActive == state) 
            {
                module.rect.anchoredPosition = target;
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


    //--------------------------//
    //  - Interface Hider       //
    //--------------------------//
    
    public void Show()
    {
        inventoryUI.SetActive(true);
    }
    public void Hide()
    {
        inventoryUI.SetActive(false);
    }
}



public class UIData 
{
    public int type = 0;
    public Item[] itemArray = null;
}