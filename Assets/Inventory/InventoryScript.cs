using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

/* This object manages the inventory UI. */

public class InventoryScript : MonoBehaviour
{
    public GameObject pistol;
    public GameObject inventoryUI;  // The entire UI
    public Transform itemsParent;   // The parent object of all the items
    public GameObject inventoryBkg;
    public GameObject bounds;
    //public GameObject holdables;
    public FirstPersonController fps;


    Inventory inventory;    // Our current inventory
    public ItemSlot[] slots;
    Transform[] holds;
    bool invOpen = false;
    int curSlot = 0;
    int fslot;
    int bslot;
    int prevSlot;
    int secSlot;
    bool isClearing = false;

    public bool InOpen() 
    {
        return invOpen;
    }
    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallback += UpdateUI;
        
        slots = itemsParent.GetComponentsInChildren<ItemSlot>(true);
        //holds = holdables.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].slotNumber = i;
        }
        ChangeHover();
    }

    // Check to see if we should open/close the inventory
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {


            if (!invOpen)
            {
                DropItem(curSlot);
                
            }
            else
            {

            }
        }

        Debug.Log(Input.GetAxis("Horizontal"));
        
        if (Input.GetAxis("Horizontal") > 0f && (!invOpen))
        {
            if (!isClearing)
            {
                if (curSlot >= 1)
                {
                    secSlot = prevSlot;
                    prevSlot = curSlot;
                    curSlot = curSlot - 1;
                }
                else
                {
                    curSlot = 8;
                    secSlot = prevSlot;
                    prevSlot = 0;
                }
                ChangeHover();
            }
        }
        if (Input.GetAxis("Horizontal") < 0f && (!invOpen))
        {
            if (!isClearing) 
            {
                if (curSlot <= 7)
                {
                    secSlot = prevSlot;
                    prevSlot = curSlot;
                    curSlot = curSlot + 1;
                }
                else
                {
                    curSlot = 0;
                    secSlot = prevSlot;
                    prevSlot = 8;
                }
                ChangeHover();
            }
        }
    }
    public void InvButton() 
    {
     
        if (invOpen)
        {
           // Cursor.lockState = CursorLockMode.Locked;
            invOpen = false;
            //Cursor.visible = false;
        }
        else
        {
            //Cursor.lockState = CursorLockMode.None;
            invOpen = true;
            //Cursor.visible = true;
        }
        inventoryBkg.SetActive(!inventoryBkg.activeSelf);
        bounds.SetActive(!bounds.activeSelf);
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
        Debug.Log(slots[value].item.name);

        if (value == curSlot) 
        {
            ClearSlot(value);
        }
        slots[value].item.RemoveFromInventory();
        slots[value].item = null;
        UpdateUI();
    }


    public void UpdateUI()
    {
        for (int e = 0; e < inventory.items.Count; e++)
        {
            int curSlot = inventory.items[e].currSlot;
            if (curSlot == 44 && GetAvailSlot() != 44)
            {
                int availSlot = GetAvailSlot();
                slots[e].ClearSlot();
                slots[availSlot].AddItem(inventory.items[e]);
                inventory.items[e].currSlot = availSlot;
            }
            else 
            {
                slots[inventory.items[e].sitSlot].ClearSlot();
                slots[curSlot].AddItem(inventory.items[e]);
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


    private void ChangeHover()
    {
        if (curSlot >= 0 && curSlot <= 8)
        {
           EnableCurrentSlot(curSlot, prevSlot);
        }
    }


    private void EnableCurrentSlot(int value, int value2)
    {
        slots[value2].Hover.SetActive(false);
            slots[value].Hover.SetActive(true);
            if (slots[value2].item != null && slots[value2].item.dropableObject.tag == "HoldableAnim") 
            {
                StartCoroutine(ClearPrevious(value, value2));
            }
            else 
            {
                EnableSlot(value);
            } 
    }
    IEnumerator ClearPrevious(int value, int value2)
    {
        isClearing = true;
        HoldAnim(value2);
        yield return new WaitForSeconds(0.3f);
        ClearSlot(value2);
        EnableSlot(value);
        isClearing = false;
    }
    private void HoldAnim(int value) 
    {
        if (holds.Length != 0)
        {
            for (int i = 0; i < holds.Length; i++)
            {
                string itemName = slots[value].item.name;
                if (itemName == holds[i].name)
                {
                    holds[i].gameObject.GetComponent<Animator>().SetTrigger("putAway");
                    return;
                }
                
            }
        }
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