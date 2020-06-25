
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IDropHandler
{
    public RectTransform inventoryBounds;
    public RectTransform inventoryBounds2;
    public RectTransform inventoryBounds3;
    public RectTransform inventoryBounds4;
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
        GameObject drop = eventData.pointerDrag;
        ItemSlot parent = drop.GetComponentInParent<ItemSlot>();
        if (parent == null)
        {
            return;
        }
        Item item = parent.getItem();
        if (parent.dragType == 1)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds, Input.mousePosition)
            && !RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds2, Input.mousePosition)
            && !RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds3, Input.mousePosition)
            && !RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds4, Input.mousePosition))
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
                            ItemSlot itemSlot = obj.GetComponentInParent<ItemSlot>();
                            if (itemSlot != null)
                            {
                                obj = itemSlot.gameObject;
                            }
                        }
                        ItemSlot objSlot = obj.GetComponent<ItemSlot>();
                        ItemSlot dropSlot = drop.GetComponent<ItemSlot>();
                        if (objSlot != null && dropSlot != null)
                        {
                            int newSlot = objSlot.slotNumber;
                            int curSlot = dropSlot.slotNumber;
                            playerInfoManager.MoveItemBySlots(curSlot, newSlot);
                        }
                    }
                }
            }
        }
        else if(parent.dragType == 2) 
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(deathDropBounds, Input.mousePosition))
            {
                if (parent.getItem() != null)
                {
                    PlayerActionManager.singleton.TakeSingleDeathDrop(item.currSlot);
                }
            }
        }
    }
}