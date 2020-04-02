
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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
    public Item item;
    public InventoryScript inventoryScript;
    public GameObject itemTooltip;
    public GameObject itemToolcraft;
    public GameObject toolTipSplit;
    public Slider toolTipSlider;
    Vector3 iconPos;
    


    private void Start()
    {
        iconPos = icon.gameObject.transform.position;
        itemTooltip.SetActive(false);
    }


    public void AddItem(Item newItem)
    {
        item = newItem;
        
        if (item.icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
            item.sitSlot = slotNumber;
            numberItems = item.itemStack;
        }
        
    }

    public GameObject GetImage() 
    {
        return icon.gameObject;
    }

    public void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.enabled = false;
        numberItems = 0;
        UpdateText();
    }

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
    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            icon.gameObject.transform.position = Input.mousePosition;
            item.isDragging = true;
            SetTooltip();
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        icon.gameObject.transform.position = iconPos;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (item != null)
        {
            //SetTooltip();
        }
    }
    public void Toggle() 
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
    public void UpdateText()
    { }
    void Update(){
        
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
    public void SetTooltip() 
    {
        if (inventoryScript.invOpen) 
        {
            itemTooltip.SetActive(true);
            itemToolcraft.SetActive(false);
            if (item.maxItemStack > 1)
            {
                toolTipSlider.minValue = 1;
                toolTipSlider.maxValue = item.itemStack;
                toolTipSlider.value = toolTipSlider.maxValue / 2;
                toolTip_button.text = "SPLIT  " + toolTipSlider.value;
                toolTipSplit.SetActive(true);
            }
            else
            {
                toolTipSplit.SetActive(false);
            }
            toolTipImage.sprite = icon.sprite;
            toolTip_name.text = item.name;
            toolTip_desc.text = item.description;
            inventoryScript.toolTipItem = item;
        }
    }
}