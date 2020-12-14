using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ItemDataManager : MonoBehaviour
{
    public static ItemDataManager Singleton;
    [SerializeField]
    public ItemData[] ItemData = new ItemData[0];

#if UNITY_EDITOR
    private void OnValidate()
    {
        //string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Content/ItemData" });
        //int count = guids.Length;
        //if (ItemData.Length == count) return;
        //ItemData = new ItemData[count];
        //for (int n = 0; n < count; n++)
        //{
        //    var path = AssetDatabase.GUIDToAssetPath(guids[n]);
        //    ItemData[n] = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        //    //ItemData[n].description = "This is a " + ItemData[n].itemName + ".";
        //}
    }
#endif


    private void Awake() 
    {
        Singleton = this;
    }


    // Get ItemData From Item ID
    public ItemData GetItemData(int itemId) 
    {
        
        for (int i = 0; i < ItemData.Length; i++)
        {
            if (itemId == ItemData[i].itemId) 
            {
                ItemData data = ItemData[i];
                Debug.Log("Item Data Requested. " + data.itemId + " " + data.icon.texture.width);
                return data;
            }
        }
        return null;
    }
    public bool IsCraftable(int itemId) 
    {
        return GetItemData(itemId).isCraftable;
    }
    public bool IsPlaceable(int itemId)
    {
        return GetItemData(itemId).isPlaceable;
    }
    public bool IsHoldable(int itemId)
    {
        return GetItemData(itemId).isHoldable;
    }
    public bool IsArmor(int itemId)
    {
        return GetItemData(itemId).isArmor;
    }
    public GameObject GetHoldableObject(int itemId) 
    {
        return GetItemData(itemId).holdableObject;
    }
    public GameObject GetPlaceableObject(int itemId) 
    {
        return GetItemData(itemId).placeableItem;
    }
    public Sprite GetIcon(int itemId) 
    {
        return GetItemData(itemId).icon;
    }

}
