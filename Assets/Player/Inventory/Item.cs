using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]

public class Item : ScriptableObject
{
    public string dropObject = "object-2";
    public Sprite icon = null;
    
    public int itemStack = 1;
    public int maxItemStack = 64;
    public int itemID = 0;
    public int currSlot;
    public int sitSlot;
    public int armorType = 0;
    public int craftAmount = 0;

    public string special = "";
    new public string name = "New Item";
    public string description = "This is the description for this item";

    public bool showInInventory = true;
    public bool isCraftable = false;
    public bool isHoldable = false;
    public bool isArmor = false;
    public bool isDragging = false;

    public string[] recipe;
}