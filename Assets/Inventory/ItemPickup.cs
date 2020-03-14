using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : Interactable
{
    public Item item;

    public override void Interact()
    {
        base.Interact();

        PickUp();
    }
    private void PickUp() 
    {
        bool wasPickedUp = Inventory.instance.Add(Object.Instantiate(item)); 
        
        if (wasPickedUp)
        {
            Destroy(gameObject);
        }

    }
}       
    
