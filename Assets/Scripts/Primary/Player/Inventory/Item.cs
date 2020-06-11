using System;
using UnityEngine;

[Serializable]
public class Item
{
    public int itemID = 0;
    public int itemStack = 0;
    public int maxItemStack = 0;
    public int currSlot = 0;
    public int armorType = 0;

    public string special = "";

    public bool isCraftable = false;
    public bool isHoldable = false;
    public bool isArmor = false;
    public bool isDragging = false;
    public bool showInInventory = true;

}


