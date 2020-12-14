using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData_1", menuName = "ScriptableObjects/ItemData")]
[Serializable]
public class ItemData : ScriptableObject
{
    //-----------------Item Data-------------------
    [SerializeField] public int itemId = 0;
    [SerializeField] public string itemName = "New Item";
    [SerializeField] public string description = "This is the description for this item";
    [SerializeField]public Sprite icon = null;
    [SerializeField] public int maxItemStack = 200;
    [SerializeField] public int armorType;


    //-----------------Item Usability-------------------
    [SerializeField] public bool isUsable = false;
    [SerializeField] public bool startMaxDurability = false;
    [SerializeField] public int useType = 0; // 1)Shoot 2)Melee 3)Tool 4)Consume 5)Build
    [SerializeField] public int useRange = 10; //Meters
    [SerializeField] public float useDelay = 1.2F; //Seconds
    [SerializeField] public int useParticleId = 1;
    [SerializeField] public int useAmount = 1; //Damage
    [SerializeField] public string[] itemUse;
    [SerializeField] public int maxDurability = 0;
    [SerializeField] public int durabilityId = 0;
    [SerializeField] public int durabilityRefilId = 0;


    //-----------------Item Holdable-------------------
    [SerializeField] public bool isHoldable = false;
    [SerializeField] public int holdableId;
    [SerializeField] public GameObject holdableObject;


    //-----------------Item Crafting-------------------
    [SerializeField] public bool isCraftable = false;
    [SerializeField] public int craftAmount = 0;
    [SerializeField] public string[] recipe;


    //-----------------Item Placeable-------------------
    [SerializeField] public bool isPlaceable = false;
    [SerializeField] public GameObject placeableItem;


    //---------------------Item Armor-------------------
    [SerializeField] public bool isArmor = false;
    [SerializeField] public bool showInInventory = true;


    //--------------------Item Tool---------------------
    [SerializeField] public int toolId;
    [SerializeField] public int toolGatherAmount;

    //-------------------Item Sounds--------------------
    [SerializeField] public int useSoundId;
    [SerializeField] public int hitSoundId;

    
}
