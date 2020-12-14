using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class UI_ItemSlot : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerClickHandler, IEndDragHandler
{
    public UI_Inventory inventory;
    public UI_TempSlot tempHoverIcon;
    
    public TextMeshProUGUI slot_Amount;
    public Image icon;
    public GameObject Hover;
    public Slider slider;

    private Item stored_item;
    private ItemData stored_data;

    public int numberItems = 0;
    public int slotNumber = 0;
    public int armorType = 0;
    public int dragType = 1;

    public bool isTooltipped = false;
    public bool itemInSlot = false;




    private void Start() 
    {
        inventory = UI_Inventory.Singleton;
    }



    //Event: On Begin Drag
    public void OnBeginDrag(PointerEventData eventData) 
    {
        MusicManager.PlayUISound(1);
    }
    //Event: On Draging
    public void OnDrag(PointerEventData eventData)
    {
        if (itemInSlot)
        {
            MoveTempIcon();
            if (dragType == 1)
            {
                SetTooltip();
                inventory.CheckHoverObject();
            }
        }
    }
    //Event: On End Drag
    public void OnEndDrag(PointerEventData eventData)
    {
        ResetTempIcon();
        MusicManager.PlayUISound(2);
    }
    //Event: On Pointer Click
    public void OnPointerClick(PointerEventData eventData)
    {
        if(itemInSlot) 
        {
            SetTooltip();
        }
    }
    


    //Add Item to Slot
    public void Incoming(Item item) 
    {
        if (item.itemId == 0 || item.itemStack == 0) return;
        stored_item = item;
        stored_data = ItemDataManager.Singleton.GetItemData(item.itemId);
        UpdateIcon();
        UpdateCount();
        itemInSlot = true;
    }

    //Clear this Slot
    public void Clear() 
    {
        itemInSlot = false;
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

    //Toggle this Slot
    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    //Get Item from this Slot
    public Item GetItem()
    {
        return stored_item;
    }

    //Set Tooltip
    public void SetTooltip()
    {
        if (itemInSlot) 
        {
            inventory.ActivateToolTip(stored_item);
            isTooltipped = true;
        }
    }

    //Select this Slot (bool)
    public void Selected(bool value)
    {
        Hover.SetActive(value);
    }



    //Update the Icon for the Slot
    private void UpdateIcon() 
    {
        if (stored_data.icon != null)
        {
            icon.sprite = stored_data.icon;
            icon.enabled = true;
        }
    }

    //Update the Item Count or Durability Bar for Slot
    private void UpdateCount() 
    {
        if (stored_data.maxDurability > 0)
        {
            if (stored_data.durabilityId == 0)
            {
                if (stored_data.maxDurability > stored_item.durability)
                {
                    slider.gameObject.SetActive(true);
                    slider.maxValue = stored_data.maxDurability;
                    slider.minValue = 0;
                    slider.value = stored_item.durability;
                }
                else
                {
                    slider.gameObject.SetActive(false);
                }
            }
            else
            {
                slider.gameObject.SetActive(false);
                numberItems = stored_item.durability;
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
        else if(!slider.gameObject.activeSelf) 
        {
            numberItems = stored_item.itemStack;
            if (numberItems > 1)
            {
                slot_Amount.text = numberItems.ToString();
            }
            else if (numberItems < 2 && slot_Amount.text != "")
            {
                slot_Amount.text = "";
            }
        }
    }

    //Move Temp Icon to Pointer Position
    private void MoveTempIcon()
    {
        if (!tempHoverIcon.image.enabled)
        {
            tempHoverIcon.image.enabled = true;
            icon.enabled = false;
            if (stored_data.maxDurability > 0 && stored_data.durabilityId == 0)
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
    
    //Reset the Temp Icon Position
    private void ResetTempIcon()
    {
        tempHoverIcon.image.enabled = false;
        if (stored_item != null)
        {
            StartCoroutine(EnableIconDelay());
        }
    }

    //Enable Icon Delay
    private IEnumerator EnableIconDelay()
    {
        yield return new WaitForSeconds(.5F);
        if (itemInSlot)
        {
            icon.enabled = true;
            if (stored_data.maxDurability > 0 && stored_item.durability < 100 && stored_data.durabilityId == 0)
            {
                slider.gameObject.SetActive(true);
            }
            if (stored_item.itemStack > 1)
            {
                slot_Amount.text = stored_item.itemStack.ToString();
            }
        }
    }


}