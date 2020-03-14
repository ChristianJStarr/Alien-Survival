
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;


public class ItemSlot : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public TextMeshProUGUI slot_Amount;
    public int numberItems = 0;
    public Image icon;
    public GameObject Hover;
    public int slotNumber = 0;
    public Item item;
    Vector3 iconPos;
    Inventory inventory;



    private void Start()
    {
        inventory = Inventory.instance;
        iconPos = icon.gameObject.transform.position;
    }


    public void AddItem(Item newItem)
    {
        item = newItem;
        
        if (item.icon == null)
        {
        }
        else 
        {
            icon.sprite = item.icon;
            icon.enabled = true;
            item.sitSlot = slotNumber;
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
    }



    public void RemoveItemFromInventory()
    {
        Inventory.instance.Remove(item);
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
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
            icon.gameObject.transform.position = iconPos;
    }
    public void Toggle() 
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
    void Update()
    {

        if (numberItems >1)
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
}