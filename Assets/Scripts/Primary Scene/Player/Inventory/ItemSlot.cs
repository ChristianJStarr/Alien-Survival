using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class ItemSlot : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerClickHandler, IEndDragHandler
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
    public UI_Inventory inventory;
    public Slider slider;
    public TempHoverSlot tempHoverIcon;
    public bool isTooltipped = false;

    public void OnBeginDrag(PointerEventData eventData) 
    {
        MusicManager.PlayUISound(1);
    }

    //Drag: On Drag
    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            MoveTempIcon();
     
            if (dragType == 1)
            {
                SetTooltip();
                inventory.CheckHoverObject();
            }
        }
    }
    //Drag: End Drag
    public void OnEndDrag(PointerEventData eventData)
    {
        ResetTempIcon();
        MusicManager.PlayUISound(2);
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
            if(itemData.maxDurability > 0 && itemData.durabilityId == 0) 
            {
                slider.gameObject.SetActive(false);
            }
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
            if (itemData.maxDurability > 0 && item.durability < 100 && itemData.durabilityId == 0)
            {
                slider.gameObject.SetActive(true);
            }
            if (item.itemStack > 1)
            {
                slot_Amount.text = item.itemStack.ToString();
            }
        }
    }

    //Update Item to Slot
    public void AddItem(Item newItem, ItemData data)
    {
        //Store Item
        item = newItem;
        itemData = data;

        //Handle Icon
        if (data.icon != null)
        {
            icon.sprite = data.icon;
            icon.enabled = true;
        }
        bool durability = false;
        //Handle Durability
        if(data.maxDurability > 0) 
        {
            durability = true;
            if (data.durabilityId == 0) 
            {
                //Normal Durability
                if (data.maxDurability > item.durability) 
                {
                    slider.gameObject.SetActive(true);
                    slider.maxValue = data.maxDurability;
                    slider.minValue = 0;
                    slider.value = item.durability;
                }
                else //Hide Slider, Durability Full 
                {
                    slider.gameObject.SetActive(false);
                }
            }
            else 
            {
                //Ammo Durability
                slider.gameObject.SetActive(false);
                numberItems = item.durability;
                if (numberItems > 0)
                {
                    slot_Amount.text = numberItems.ToString();
                }
                else if (numberItems == 0 && slot_Amount.text != "")
                {
                    slot_Amount.text = "";
                }
            }
        }

        //Handle Item Count Text
        if (!slider.gameObject.activeSelf && !durability) 
        {
            numberItems = item.itemStack;
            if (numberItems > 1) 
            {
                slot_Amount.text = numberItems.ToString();
            }
            else if(numberItems < 2 && slot_Amount.text != "") 
            {
                slot_Amount.text = "";
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
        if (isTooltipped) 
        {
            isTooltipped = false;
            inventory.HideTooltip(slotNumber);
        }
    }

    //Get this slots Item
    public Item getItem() 
    {
        return item;
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
            inventory.ActivateToolTip(item);
            isTooltipped = true;
        }
    }
}