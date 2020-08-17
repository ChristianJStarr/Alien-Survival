
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IDropHandler
{
    public RectTransform inventoryBounds;
    public RectTransform hotBarBounds;

    public RectTransform[] rightSlideMenu;
    public RectTransform leftSlideMenu;


    public RectTransform deathDropBounds;
    private InventoryGfx inventoryGfx;
    private PlayerInfoManager playerInfoManager;

    private void Start()
    {
        inventoryGfx = GetComponent<InventoryGfx>();
        playerInfoManager = PlayerInfoManager.singleton;
    }

    //On Item Drop
    public void OnDrop(PointerEventData eventData)
    {
        DebugMsg.Begin(230, "Drop Event Started", 4);

        GameObject drop = eventData.pointerDrag;
        ItemSlot parent = drop.GetComponentInParent<ItemSlot>();
        if (parent == null)
        {
            DebugMsg.End(230, "Drop Event Finished", 4);
            return;
        }
        Item item = parent.getItem();
        if (parent.dragType == 1)
        {
            if (HoveringOverSlots()) // Dragging Over Standard Inventory Slots
            {
                var raycastResults = new List<RaycastResult>();
                
                EventSystem.current.RaycastAll(eventData, raycastResults);
                
                if (raycastResults.Count > 0)
                {
                    GameObject obj = raycastResults[0].gameObject;
                    if (parent.getItem() != null)
                    {
                        ItemSlot newSlot = null;
                        
                        if (obj.name == "Image")
                        {
                            newSlot = obj.GetComponentInParent<ItemSlot>();
                        }
                        else if(newSlot == null) 
                        {
                            newSlot = obj.GetComponent<ItemSlot>();
                        }
                        if (newSlot != null)
                        {
                            playerInfoManager.MoveItemBySlots(parent.slotNumber, newSlot.slotNumber);
                        }
                        DebugMsg.End(230, "Drop Event Finished", 4);
                    }
                }
            }
            else //Drop Item
            {
                if (parent.getItem() != null)
                {
                    DebugMsg.End(230, "Drop Event Finished", 4);
                    playerInfoManager.RemoveItemBySlot(item.currSlot);
                }
            }
        }
        else if (parent.dragType == 2)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(deathDropBounds, Input.mousePosition))
            {
                if (parent.getItem() != null)
                {
                    DebugMsg.End(230, "Drop Event Finished", 4);
                    PlayerActionManager.singleton.TakeSingleDeathDrop(item.currSlot);
                }
            }
        }
    }

    private bool HoveringOverRightMenu() 
    {
        bool isHovering = false;
        if(rightSlideMenu != null && rightSlideMenu.Length > 0) 
        {
            foreach (RectTransform item in rightSlideMenu)
            {
                if (!isHovering)
                    isHovering = RectTransformUtility.RectangleContainsScreenPoint(leftSlideMenu, Input.mousePosition);
            }
        }
        return isHovering;
    }

    private bool HoveringOverLeftMenu() 
    {
        return (RectTransformUtility.RectangleContainsScreenPoint(leftSlideMenu, Input.mousePosition));
    }

    private bool HoveringOverInventory() 
    {
        return (RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds, Input.mousePosition)
            || RectTransformUtility.RectangleContainsScreenPoint(hotBarBounds, Input.mousePosition));
    }

    private bool HoveringOverSlots() 
    {
        bool isHovering = false;
        isHovering = HoveringOverRightMenu();
        if (!isHovering) 
        {
            isHovering = HoveringOverLeftMenu();   
        }
        if (!isHovering)
        {
            isHovering = HoveringOverInventory();
        }
        return isHovering;
    }

}