using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData_1", menuName = "ScriptableObjects/ItemData")]
[Serializable]
public class ItemData : ScriptableObject
{
    [SerializeField]public Sprite icon = null;
    [SerializeField] public int itemID = 0;
    [SerializeField] public int craftAmount = 0;
    [SerializeField] public int maxItemStack = 200;
    [SerializeField] public int armorType;

    [SerializeField] public int toolId;
    [SerializeField] public int toolGatherAmount;


    [SerializeField] public int useType = 0;
    [SerializeField] public int useRange = 10;
    [SerializeField] public int useParticleId = 1;
    [SerializeField] public string[] itemUse;
    [SerializeField] public int maxDurability = 0;
    [SerializeField] public int durabilityId = 0;

    [SerializeField] public int holdableId;

    [SerializeField] public string itemName = "New Item";
    [SerializeField] public string description = "This is the description for this item";
    [SerializeField] public string[] recipe;
    [SerializeField] public string useSound;
    [SerializeField] public string hitSound;

    [SerializeField] public bool isCraftable = false;
    [SerializeField] public bool isPlaceable = false;
    [SerializeField] public bool isHoldable = false;
    [SerializeField] public bool isArmor = false;
    [SerializeField] public bool isDragging = false;
    [SerializeField] public bool showInInventory = true;
    [SerializeField] public bool startMaxDurability = false;
    [SerializeField] public GameObject holdableObject;
    [SerializeField] public GameObject placeableItem;
}

// Use Types
//  1)Shoot
//  2)Melee
//  3)Place
//  4)Punch
//  5)Build