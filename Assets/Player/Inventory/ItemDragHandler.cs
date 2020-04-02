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
    public InventoryScript inventoryScript;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject drop = eventData.pointerDrag;
        ItemSlot parent = drop.GetComponentInParent<ItemSlot>();
        if (parent == null) 
        {
            return;
        }
        Item item = parent.getItem();
        if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds, Input.mousePosition) 
            && !RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds2, Input.mousePosition) 
            && !RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds3, Input.mousePosition) 
            && !RectTransformUtility.RectangleContainsScreenPoint(inventoryBounds4, Input.mousePosition))
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
                    if (obj.name == "Image")
                    {
                        obj = obj.GetComponentInParent<ItemSlot>().gameObject;
                    }
                    for (int i = 0; i < inventoryScript.slots.Length; i++)
                    {
                        ItemSlot slot = inventoryScript.slots[i];
                        if (slot.gameObject == obj)
                        {
                            //Debug.Log("Inventory - Item dropped on slot.");
                            if (slot.item != null)
                            {
                                if (item.isArmor) 
                                {
                                    foreach (ItemSlot armorSlot in inventoryScript.armor)
                                    {
                                        if (armorSlot.item != null)
                                        {
                                            if (armorSlot.item = item)
                                            {
                                                inventoryScript.RemoveFromArmor(item);
                                                item.currSlot = i;
                                                if (slot.item.armorType == item.armorType)
                                                {
                                                    inventoryScript.AddToArmor(slot.item);
                                                    slot.item.currSlot = 32 + slot.item.armorType;
                                                }
                                                else 
                                                {
                                                    slot.item.currSlot = 44;
                                                }
                                            }
                                        }
                                    }
                                }
                                else 
                                {
                                    if (slot.item.itemID == item.itemID && slot.item.maxItemStack > 1) 
                                    {
                                        //Debug.Log("Inventory - Item dragged on same item type stackable");
                                        if(item.itemStack <= slot.item.maxItemStack - slot.item.itemStack) 
                                        {
                                            //Debug.Log("Inventory - Dragged Item fits in stack");
                                            slot.item.itemStack += item.itemStack;
                                            inventoryScript.RemoveItem(item);
                                            slot.UpdateText();
                                        }
                                        else if(slot.item.maxItemStack - slot.item.itemStack > 0) 
                                        {
                                            //Debug.Log("Inventory - Dragged Item kinda fits");
                                            item.itemStack -= slot.item.maxItemStack - slot.item.itemStack;
                                            slot.item.itemStack = slot.item.maxItemStack;
                                            slot.UpdateText();
                                        }
                                    }
                                    else 
                                    {
                                        slot.item.currSlot = item.currSlot;
                                        item.currSlot = i;
                                    }
                                }
                                inventoryScript.UpdateUI();
                                inventoryScript.UpdateUI();
                            }
                            else
                            {
                                if (item.isArmor)
                                {
                                    bool equiped = false;
                                    foreach (ItemSlot armorSlot in inventoryScript.armor)
                                    {
                                        if (armorSlot.item != null)
                                        {
                                            if (armorSlot.item = item)
                                            {
                                                inventoryScript.RemoveFromArmor(item);
                                                item.currSlot = i;
                                                equiped = true;
                                            }
                                        }
                                    }
                                    if(equiped == false) 
                                    {
                                        item.currSlot = i;
                                    }
                                }
                                else
                                {
                                    item.currSlot = i;
                                }
                                inventoryScript.UpdateUI();
                            }
                        }
                    }
                    for (int i = 0; i < inventoryScript.armor.Length; i++)
                    {
                        ItemSlot slot = inventoryScript.armor[i];
                        if (slot.gameObject == obj && item.isArmor == true && item.armorType == slot.armorType)
                        {
                            if (slot.item != null)
                            {
                                slot.item.currSlot = item.currSlot;
                                item.currSlot = 33 + i;
                                inventoryScript.RemoveFromArmor(slot.item);
                                slot.item.currSlot = 44;
                                inventoryScript.AddToArmor(item);
                                inventoryScript.UpdateUI();
                            }
                            else
                            {
                                item.currSlot = item.armorType;
                                inventoryScript.AddToArmor(item);
                                inventoryScript.UpdateUI();
                            }
                        }
                    }
                }
            }
        }
    }
}