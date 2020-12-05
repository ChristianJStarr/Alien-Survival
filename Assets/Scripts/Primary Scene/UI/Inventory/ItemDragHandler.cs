
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
    public UI_Inventory inventory;
    private PlayerInfoManager playerInfoManager;

    private void Start()
    {
        playerInfoManager = PlayerInfoManager.singleton;
    }

    //On Item Drop
    public void OnDrop(PointerEventData eventData)
    {
        GameObject drop = eventData.pointerDrag;
        UI_ItemSlot parent = drop.GetComponentInParent<UI_ItemSlot>();
        if (parent == null)
        {
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
                        UI_ItemSlot newSlot = null;
                        
                        if (obj.name == "Image")
                        {
                            newSlot = obj.GetComponentInParent<UI_ItemSlot>();
                        }
                        else if(newSlot == null) 
                        {
                            newSlot = obj.GetComponent<UI_ItemSlot>();
                        }
                        if (newSlot != null)
                        {
                            playerInfoManager.MoveItemBySlots(parent.slotNumber, newSlot.slotNumber);
                        }
                    }
                }
            }
            else //Drop Item
            {
                if (parent.getItem() != null)
                {
                    playerInfoManager.RemoveItemBySlot(item.currSlot);
                }
            }
        }
        else if (parent.dragType == 2)
        {
            
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