using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IDropHandler
{
    public RectTransform inventoryBounds;
    public RectTransform inventoryBounds2;
    public InventoryScript inventoryScript;

    public void Start()
    {
        inventoryScript = inventoryScript.GetComponent<InventoryScript>();
    }
    public void OnDrop(PointerEventData eventData)
    {
        GameObject drop = eventData.pointerDrag;
        ItemSlot parent = drop.GetComponentInParent<ItemSlot>();
        Item item = parent.getItem();
        if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds, Input.mousePosition) && !RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds2, Input.mousePosition))
        {


            if (parent.getItem() != null)
            {
                int slot = item.currSlot;
                inventoryScript.DropItem(slot);
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
                    for (int i = 0; i < inventoryScript.slots.Length; i++)
                    {
                        if (obj.name == "Image")
                        {
                            obj = obj.GetComponentInParent<ItemSlot>().gameObject;
                        }
                        if (inventoryScript.slots[i].gameObject == obj || inventoryScript.slots[i].gameObject == obj)
                        {
                            if (inventoryScript.slots[i].item != null)
                            {
                                inventoryScript.slots[i].item.currSlot = item.currSlot;
                                item.currSlot = i;
                                inventoryScript.UpdateUI();
                                inventoryScript.UpdateUI();
                            }
                            else
                            {
                                item.currSlot = i;
                                inventoryScript.UpdateUI();
                            }
                        }
                    }
                }
            }
        }
    }
}