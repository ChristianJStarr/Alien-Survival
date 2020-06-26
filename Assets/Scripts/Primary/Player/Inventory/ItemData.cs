using UnityEngine;

[CreateAssetMenu(fileName = "ItemData_1", menuName = "ScriptableObjects/ItemData")]
public class ItemData : ScriptableObject
{
    public Sprite icon = null;
    public int itemID = 0;
    public int craftAmount = 0;
    public int maxItemStack = 200;
    public int armorType;
    
    // Use Types
    //  1)Shoot
    //  2)Melee
    //  3)Place
    //  4)Punch
    //  5)Build
    public int useType = 0;
    public int useRange = 10;
    public int useParticleId = 1;
    public string[] itemUse;
    public int maxDurability = 0;
    public int durabilityId = 0;
    
    public string name = "New Item";
    public string description = "This is the description for this item";
    public string[] recipe;

    public bool isCraftable = false;
    public bool isHoldable = false;
    public bool isArmor = false;
    public bool isDragging = false;
    public bool showInInventory = true;
    public bool startMaxDurability = false;
    public GameObject holdableObject;
}