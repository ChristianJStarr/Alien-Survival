using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using TMPro;
using Photon.Pun;
using System.IO;
using System;

public class InventoryScript : MonoBehaviour
{

    public GameObject inventoryUI, inventoryBkg, playerMenu, toolTipMenu, craftMenu, bounds, bounds2, bounds3;
    public ControlControl controls;
    public Transform itemsParent, armorSlots;
    public Slider splitSlider;
    public TextMeshProUGUI splitText;
    public Item toolTipItem;
    public Inventory inventory; 
    public ItemSlot[] slots, armor;
    public bool invOpen = false;
    FirstPersonController fps;
    CraftingMenu craftingMenu;
    Transform[] holds;

    public bool InOpen() 
    {
        return invOpen;
    }
    void Start()
    {
        craftingMenu = GetComponent<CraftingMenu>();
        slots = itemsParent.GetComponentsInChildren<ItemSlot>(true);
        armor = armorSlots.GetComponentsInChildren<ItemSlot>(true);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].slotNumber = i;
        }
        for (int i = 0; i < armor.Length; i++)
        {
            armor[i].slotNumber = i + 33;
        }
        UpdateUI();
        
    }

    public void GetAvailableCraft() 
    {
        craftingMenu.GetAvailable();
    }

    public void InvButton() 
    {
     
        if (invOpen)
        {
            invOpen = false;
            toolTipMenu.SetActive(false);
            controls.Show();
        }
        else
        {
            controls.Hide();
            invOpen = true;
        }
        inventoryBkg.SetActive(invOpen);
        playerMenu.SetActive(invOpen);
        craftMenu.SetActive(invOpen);
        for (int i = 0; i < slots.Length; i++)
        {
            if (i > 5)
            {
                slots[i].Toggle();
            }
        }
        for (int i = 0; i < armor.Length; i++)
        {
            armor[i].Toggle();  
        }
        UpdateUI();
    }
    public void DropItem(int value) 
    {
        //Debug.Log("Inventory - Drop Item Called" + value);
        if (slots[value].item != null && !String.IsNullOrEmpty(slots[value].item.dropObject))
        {
            //Debug.Log("Inventory - Drop Item Action");
            Item dropItem = slots[value].item;
            Transform trans = fps.gameObject.GetComponentInChildren<Transform>();
            Vector3 rot = trans.rotation.eulerAngles;
            rot = new Vector3(rot.x, rot.y + 90, rot.z);
            trans.rotation = Quaternion.Euler(rot);
            GameObject dropped = PhotonNetwork.Instantiate(Path.Combine("Persistant", dropItem.dropObject), trans.position - trans.right, trans.rotation);
            dropped.GetComponent<DroppedItem>().Set(dropItem.name, dropItem.itemID, dropItem.itemStack, dropItem.special);
            dropped.GetComponent<Rigidbody>().AddForce(-dropped.transform.right * 200f);

            DisposeItem(value);

        }
    }

    public void UpdateSplitSlider() 
    {
        splitText.text = "SPLIT  " + splitSlider.value; 
    }

    public void Split() 
    {
        Item item = toolTipItem;
        if (item == null)
            return;

        if(item.maxItemStack > 1 && item.itemStack > 1) 
        {
            Item newItem = Instantiate(item);
            newItem.itemStack = (int)splitSlider.value;
            newItem.currSlot = 44;
            newItem.sitSlot = 0;
            if (inventory.AddSingle(newItem))
            {
                item.itemStack -= (int)splitSlider.value;
                ItemSlot slot = GetSlot(item);
                if(slot != null) 
                {
                    slot.SetTooltip();
                }
            }
            else
            {
                Destroy(newItem);
            }
            UpdateUI();
        }
    }


    public void DisposeItem(int value) 
    {
        inventory.Remove(slots[value].item);
        slots[value].item = null;
        UpdateUI();
    }

    public void RemoveFromArmor(Item item) 
    {
        inventory.armor.Remove(item);
        inventory.items.Add(item);
    }
    public void AddToArmor(Item item) 
    {
        inventory.items.Remove(item);
        inventory.armor.Add(item);
    }

    public void UpdateUI()
    {
        for (int i = 0; i < inventory.armor.Count; i++)
        {
            int armorType = inventory.armor[i].armorType;
            foreach(ItemSlot slot in armor) 
            {
                if (slot.armorType == armorType) 
                {
                    slot.AddItem(inventory.armor[i]);
                    break;
                }
            }
        }
        for (int e = 0; e < inventory.items.Count; e++)
        {
            int cuSlot = inventory.items[e].currSlot;
            if (cuSlot == 44 && GetAvailSlot() != 44)
            {
                int availSlot = GetAvailSlot();
                slots[e].ClearSlot();
                slots[availSlot].AddItem(inventory.items[e]);
                inventory.items[e].currSlot = availSlot;
                inventory.items[e].sitSlot = availSlot;
            }
            else 
            {
                if (inventory.items[e].sitSlot > 32)
                {
                    armor[inventory.items[e].sitSlot - 32].ClearSlot();
                    inventory.items[e].sitSlot = cuSlot;
                    slots[cuSlot].AddItem(inventory.items[e]);
                }
                else
                {
                    slots[inventory.items[e].sitSlot].ClearSlot();
                    slots[cuSlot].AddItem(inventory.items[e]);
                    inventory.items[e].sitSlot = cuSlot;
                }
            }
        }
        foreach (ItemSlot slot in slots)
        {
            if (!inventory.items.Contains(slot.item))
            {
                slot.ClearSlot();
            }
        }
        foreach (ItemSlot slot in armor)
        {
            if (!inventory.armor.Contains(slot.item)) 
            {
                slot.ClearSlot();
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

    public ItemSlot GetSlot(Item item) 
    {
        ItemSlot theSlot = null;
        foreach (ItemSlot slot in slots)
        {
            if (slot.item == item) 
            {
                theSlot = slot;
                break;
            }
        }
        return theSlot;
    }

    public void RemoveItem(Item item) 
    {
        inventory.Remove(item);
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