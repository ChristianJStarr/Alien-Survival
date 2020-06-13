using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;
using System.Linq;

public class InventoryGfx : MonoBehaviour
{
    public TopToolTipHandler toolTipHandler;
    public GameObject inventoryUI, inventoryBkg, playerMenu, craftMenu, bounds, bounds2, bounds3, hotBarButtons, tint;
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

    private bool craftingActive;
    private bool armorActive;

    public RectTransform armorRect;
    public RectTransform craftingRect;

    private Vector2 armorTarget = new Vector3(-354F, -41.99303F);
    private Vector2 craftingTarget = new Vector3(2997, -41.99601F);
    private bool armorMove;
    private bool craftingMove;

    private void Update()
    {
        UpdateMenus();
    }


    public void Incoming(PlayerInfo playerInfo)
    {
        items = playerInfo.items;
        armor = playerInfo.armor;
        blueprints = playerInfo.blueprints;
        if(craftingMenu == null) 
        {
            craftingMenu = GetComponent<CraftingMenu>();
        }
        craftingMenu.GetResources(items, blueprints);
        UpdateUI();
    }

    public void ButtonCraftingMenu()
    {
        craftingActive = !craftingActive;
        SlideMenu(true, craftingActive);
        if (armorActive) 
        {
            armorActive = false;
            SlideMenu(false, false);
        }
    }
    public void ButtonArmorMenu() 
    {
        armorActive = !armorActive;
        SlideMenu(false, armorActive);
        if (craftingActive)
        {
            craftingActive = false;
            SlideMenu(true, false);
        }
    }

    private void UpdateMenus() 
    {
        if(armorMove) 
        {
            if (armorRect.anchoredPosition != armorTarget)
            {
                armorRect.anchoredPosition = Vector2.MoveTowards(armorRect.anchoredPosition, armorTarget, 6000 * Time.deltaTime);
            }
            else 
            {
                armorMove = false;
                if (armorActive) 
                {
                    tint.SetActive(true);
                }
                else
                {
                    tint.SetActive(false);
                }
            }
        }
        if(craftingMove) 
        {
            if (craftingRect.anchoredPosition != craftingTarget)
            {
                craftingRect.anchoredPosition = Vector2.MoveTowards(craftingRect.anchoredPosition, craftingTarget, 6000 * Time.deltaTime);
            }
            else 
            {
                craftingMove = false;
                if (craftingActive)
                {
                    tint.SetActive(true);
                }
                else 
                {
                    tint.SetActive(false);
                }
            }
        }
    }

    private void SlideMenu(bool craftingMenu, bool state) 
    {
        if (craftingMenu) 
        {
            Vector2 target;
            if (state) 
            {
                target = new Vector2(1943, -41.99601F);
            }
            else 
            {
                target = new Vector2(2997, -41.99601F);
            }
            craftingTarget = target;
            craftingMove = true;
        }
        else 
        {
            Vector2 target;
            if (state)
            {
                target = new Vector2(489.6F, -41.99303F);
            }
            else
            {
                target = new Vector2(-354F, -41.99303F);
            }
            armorTarget = target;
            armorMove = true;
        }
    }

    void Start()
    {

        allItems = Resources.LoadAll("Items", typeof(ItemData)).Cast<ItemData>().ToArray();
        craftingMenu = GetComponent<CraftingMenu>();
        itemSlots = itemsParent.GetComponentsInChildren<ItemSlot>(true);
        armorSlots = armorSlotsContainer.GetComponentsInChildren<ItemSlot>(true);
        
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].slotNumber = i + 1;
        }
        for (int i = 0; i < armorSlots.Length; i++)
        {
            armorSlots[i].slotNumber = i + 34;
        }
    }



    public void ActivateToolTip(Item item) 
    {
        if (!toolTipHandler.gameObject.activeSelf)
        {
            toolTipHandler.gameObject.SetActive(true);
        }
        toolTipHandler.SetData(FindItemData(item.itemID), item);
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
            controls.Show();
            toolTipHandler.gameObject.SetActive(false);
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