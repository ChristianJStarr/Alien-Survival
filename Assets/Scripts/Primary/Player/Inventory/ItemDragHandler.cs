using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IDropHandler
{
    public RectTransform inventoryBounds;
    public RectTransform inventoryBounds2;
    public RectTransform inventoryBounds3;
    public RectTransform inventoryBounds4;
    private InventoryGfx inventoryGfx;
    private PlayerInfoManager playerInfoManager;

    private void Start()
    {
        inventoryGfx = GetComponent<InventoryGfx>();
        playerInfoManager = PlayerInfoManager.singleton;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject drop = eventData.pointerDrag;
        ItemSlot parent = drop.GetComponentInParent<ItemSlot>();
        if (parent == null) 
        {
            return;
        }
        Item item = parent.getItem();
        if (OutOfBounds())
        {
            if (parent.getItem() != null)
            {
                playerInfoManager.RemoveItemBySlot(item.currSlot);
            }
        }
        else
        {
            drop.gameObject.SetActive(false);
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            drop.gameObject.SetActive(true);
            if (raycastResults.Count > 0)
            {
                GameObject obj = raycastResults[0].gameObject;
                if (parent.getItem() != null)
                {
                    if (obj.name == "Image")
                    {
                        obj = obj.GetComponentInParent<ItemSlot>().gameObject;
                    }
                    int newSlot = obj.GetComponent<ItemSlot>().slotNumber;
                    int curSlot = drop.GetComponent<ItemSlot>().slotNumber;
                    playerInfoManager.MoveItemBySlots(curSlot, newSlot);
                }
            }
        }
    }

    private bool OutOfBounds() 
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds, Input.mousePosition)
            && !RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds2, Input.mousePosition)
            && !RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds3, Input.mousePosition)
            && !RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds4, Input.mousePosition)) 
        {
            return true;
        }
        else 
        {
            return false;
        }
    }
}