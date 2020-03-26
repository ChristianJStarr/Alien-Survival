using UnityEngine;

/* The base item class. All items should derive from this. */

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public int itemID = 0;
    new public string name = "New Item";    // Name of the item
    public Sprite icon = null;              // Item icon
    public bool showInInventory = true;
    public int itemStack = 1;
    public int maxItemStack = 64;
    public GameObject dropableObject;
    public bool isDragging = false;
    public int currSlot;
    public int sitSlot;
    public bool isHoldable = false;
    public string special;

    // Called when the item is pressed in the inventory
    public virtual void Use()
    {
        // Use the item
        // Something may happen
    }
    

    // Call this method to remove the item from inventory
    public void RemoveFromInventory()
    {
        Inventory.instance.Remove(this);
    }

    //         100,100, 2#2#2#2#-2#2#2#=
}