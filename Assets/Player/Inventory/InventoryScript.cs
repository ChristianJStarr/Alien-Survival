using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

/* This object manages the inventory UI. */

public class InventoryScript : MonoBehaviour
{
    public GameObject inventoryUI;  // The entire UI
    public Transform itemsParent;   // The parent object of all the items
    public GameObject inventoryBkg;
    public GameObject bounds;
    public Inventory inventory; 
    public ItemSlot[] slots;
    bool invOpen = false;
    FirstPersonController fps;
    Transform[] holds;

    public bool InOpen() 
    {
        return invOpen;
    }
    void Start()
    {
        slots = itemsParent.GetComponentsInChildren<ItemSlot>(true);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].slotNumber = i;
        }
    }

    public void InvButton() 
    {
     
        if (invOpen)
        {
            invOpen = false;
            bounds.GetComponent<RectTransform>().localScale = new Vector3(0, 0, 0);
        }
        else
        {
            invOpen = true;
            bounds.GetComponent<RectTransform>().localScale = new Vector3(2, 2, 2);
        }
        inventoryBkg.SetActive(invOpen);
        

        for (int i = 0; i < slots.Length; i++)
        {
            if (i > 8)
            {
                slots[i].Toggle();
            }
        }
        UpdateUI();
    }
    public void DropItem(int value) 
    {
        if (slots[value].item != null && slots[value].item.dropableObject != null)
        {
            Item dropItem = slots[value].item;
            Transform trans = fps.gameObject.GetComponentInChildren<Transform>();
            Vector3 rot = trans.rotation.eulerAngles;
            rot = new Vector3(rot.x, rot.y + 90, rot.z);
            trans.rotation = Quaternion.Euler(rot);
            GameObject dropped = Instantiate(slots[value].item.dropableObject, trans.position - trans.right, trans.rotation);
            dropped.GetComponent<Rigidbody>().AddForce(-dropped.transform.right * 200f);

            if (dropItem.itemStack > 1)
            {
                dropItem.itemStack--;
            }
            else
            {
                DisposeItem(value);

            }
        }
    }
    public void DisposeItem(int value) 
    {
        slots[value].item.RemoveFromInventory();
        slots[value].item = null;
        UpdateUI();
    }


    public void UpdateUI()
    {
        Debug.Log("UI - Updating. Items: " + inventory.items.Count);
        for (int e = 0; e < inventory.items.Count; e++)
        {
            int cuSlot = inventory.items[e].currSlot;
            if (cuSlot == 44 && GetAvailSlot() != 44)
            {
                int availSlot = GetAvailSlot();
                slots[e].ClearSlot();
                slots[availSlot].AddItem(inventory.items[e]);
                inventory.items[e].currSlot = availSlot;
            }
            else 
            {
                slots[inventory.items[e].sitSlot].ClearSlot();
                slots[cuSlot].AddItem(inventory.items[e]);
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (inventory.items.Contains(slots[i].item))
            {

            }
            else 
            {
                slots[i].ClearSlot();
            }
        }
    }

    public void SetPlayer(FirstPersonController fp) 
    {
        fps = fp;
    }

    private int GetAvailSlot() 
    {
        int slotN = 44;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null) 
            {

                return i;
            }
        }
        return slotN;
    }



    private void ClearSlot(int value)
    {
        if (slots[value].item != null && holds.Length != 0)
        {
            if (slots[value].item.isHoldable == true) 
            {
                for (int i = 0; i < holds.Length; i++)
                {
                    string itemName = slots[value].item.name;
                    if (itemName == holds[i].name)
                    {
                        holds[i].gameObject.SetActive(false);
                        return;
                    }
                }
            }
            
        }

    }
    private void EnableSlot(int value)
    {
        if (slots[value].item != null && holds.Length != 0)
        {
            for (int i = 0; i < holds.Length; i++)
            {
                string itemName = slots[value].item.name;
                if (itemName == holds[i].name)
                {
                    holds[i].gameObject.SetActive(true);
                    Transform[] child = holds[i].gameObject.GetComponentsInChildren<Transform>(true);
                    for (int e = 0; e < child.Length; e++)
                    {
                        if (child[e].gameObject.name != holds[i].gameObject.name) 
                        {
                           child[e].gameObject.SetActive(true);
                        }
                    }
                    return;
                }
            }
        }
    }
}