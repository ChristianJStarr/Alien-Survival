using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class ItemSlot : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public TextMeshProUGUI slot_Amount, toolTip_name, toolTip_desc, toolTip_button;
    public int numberItems = 0;
    public Image icon;
    public Image toolTipImage;
    public GameObject Hover;
    public int slotNumber = 0;
    public int armorType = 0;
    public int dragType = 1;
    public Item item;
    public ItemData itemData;
    public InventoryGfx inventoryGfx;
    Vector3 iconPos;

    private void Start()
    {
        iconPos = icon.gameObject.transform.position;
    }
    private void Update()
    {

        if (numberItems > 1)
        {
            slot_Amount.text = numberItems.ToString();
        }
        else
        {
            if (slot_Amount.text != "")
            {
                slot_Amount.text = "";
            }
        }

    }



    //Drag: On Drag
    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            icon.gameObject.transform.position = Input.mousePosition;
            if (dragType == 1)
            {
                SetTooltip();
            }
            if (dragType == 2)
            {
                PlayerActionManager.singleton.DragStarted();
            }
        }
    }
    //Drag: End Drag
    public void OnEndDrag(PointerEventData eventData)
    {
        icon.gameObject.transform.position = iconPos;
        if (dragType == 2)
        {
            PlayerActionManager.singleton.DragEnded();
        }
    }
    //Drag: PointerClick
    public void OnPointerClick(PointerEventData eventData)
    {
        if (item != null)
        {
            SetTooltip();
        }
    }



    //Update Item to Slot
    public void AddItem(Item newItem, ItemData data)
    {
        item = newItem;
        if (data.icon != null)
        {
            icon.sprite = data.icon;
            icon.enabled = true;
            numberItems = item.itemStack;
            itemData = data;
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
        numberItems = 0;
        UpdateText();
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
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
    
    //Update Text
    public void UpdateText()
    { }
     
    //Set Tooltip
    public void SetTooltip() 
    {
        if(item != null) 
        {
            inventoryGfx.ActivateToolTip(item);
        }
    }
}