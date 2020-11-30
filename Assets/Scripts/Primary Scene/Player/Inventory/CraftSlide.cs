
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftSlide : MonoBehaviour, IPointerClickHandler

{
    public Image image;
    public Image bkg;
    public TextMeshProUGUI name;
    public ItemData item;
    public bool crafting;
    public bool craftable;

    private CraftingMenu craftingMenu;

    private Color onColor = new Color32(67, 67, 67, 180);
    private Color offColor = new Color32(67, 67, 67, 85);
    private Color textColor = new Color32(188, 188, 188, 255);


    public void OnPointerClick(PointerEventData eventData)
    {
        if(item != null) 
        {
            craftingMenu.ShowTooltip(item);
        }
    }

    public void Craftable(bool value, ItemData itemData, CraftingMenu menu)
    {
        craftingMenu = menu;
        item = itemData;
        image.sprite = itemData.icon;
        name.text = itemData.itemName;
        craftable = value;
        if (value) 
        {
            bkg.color = onColor;
            name.color = Color.white;
        }
        else 
        {
            bkg.color = offColor;
            name.color = textColor;
        }
    }
}
