using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ItemSlot : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public TextMeshProUGUI slot_Amount;
    public int numberItems = 0;
    public Image icon;
    public GameObject Hover;
    public int slotNumber = 0;
    public int armorType = 0;
    public int dragType = 1;
    public Item item;
    public ItemData itemData;
    public InventoryGfx inventoryGfx;
    public Slider slider;
    public TempHoverSlot tempHoverIcon;


    //Drag: On Drag
    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            MoveTempIcon();
     
            if (dragType == 1)
            {
                SetTooltip();
                inventoryGfx.CheckHoverObject();
            }
        }
    }
    //Drag: End Drag
    public void OnEndDrag(PointerEventData eventData)
    {
        ResetTempIcon();
    }
    //Drag: PointerClick
    public void OnPointerClick(PointerEventData eventData)
    {
        if (item != null)
        {
            SetTooltip();
        }
    }

    private void MoveTempIcon() 
    {
        if (!tempHoverIcon.image.enabled) 
        {
            tempHoverIcon.image.enabled = true;
            icon.enabled = false;
            slider.gameObject.SetActive(false);
            slot_Amount.text = "";
        }
        if (tempHoverIcon.image.sprite != icon.sprite)
        {
            tempHoverIcon.image.sprite = icon.sprite;
        }
        tempHoverIcon.gameObject.transform.position = Input.mousePosition; 
    }

    private void ResetTempIcon()
    {
        tempHoverIcon.image.enabled = false;
        if (item != null) 
        {
            StartCoroutine(EnableIconDelay());
        }
    }

    //Enable Icon Delay
    private IEnumerator EnableIconDelay() 
    {
        yield return new WaitForSeconds(.5F);
        if(item != null) 
        {
            icon.enabled = true;
            slider.gameObject.SetActive(true);
            if (item.itemStack > 1)
            {
                slot_Amount.text = item.itemStack.ToString();
            }
        }
    }

    //Update Item to Slot
    public void AddItem(Item newItem, ItemData data)
    {
        item = newItem;
        itemData = data;
        if (data.icon != null)
        {
            icon.sprite = data.icon;
            icon.enabled = true;
            if(data.maxDurability > 0) 
            {
                if(data.durabilityId == 0) 
                {
                    numberItems = 0;
                    slider.value = item.durability;
                    slider.gameObject.SetActive(true);
                }
                else 
                {
                    numberItems = item.durability;
                    slider.gameObject.SetActive(false);
                }
            }
            else 
            {
                numberItems = item.itemStack;
            }
            if (numberItems > 1)
            {
                slot_Amount.text = numberItems.ToString();
            }
            else if (numberItems == 0)
            {
                if (slot_Amount.text != "")
                {
                    slot_Amount.text = "";
                }
            }
        }
    }

    //Select this Slot (bool)
    public void Selected(bool value)
    {
        Hover.SetActive(value);
    }

    //Get Item Object
    public GameObject GetImage() 
    {
        return icon.gameObject;
    }

    //Clear this Slot
    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
        if (slot_Amount.text != "")
        {
            slot_Amount.text = "";
        }
        slider.gameObject.SetActive(false);
        numberItems = 0;
    }

    //Get this slots Item
    public Item getItem() 
    {
        if (item != null) 
        {
            return item;
        }
        else
        {
            return null;
        }
    }
    
    //Toggle this Slot
    public void Toggle() 
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
    
    //Set Tooltip
    public void SetTooltip() 
    {
        if(item != null) 
        {
            inventoryGfx.ActivateToolTip(item);
        }
    }
}