using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftSlide : MonoBehaviour, IPointerClickHandler

{
    public Image image;
    public Image bkg;
    public TextMeshProUGUI name;
    public Item item;
    public bool craftable;
    CraftingMenu craftingMenu;
    Color onColor = new Color32(67, 67, 67, 180);
    Color offColor = new Color32(67, 67, 67, 85);
    Color textColor = new Color32(188, 188, 188, 255);

    void Start() 
    {
        craftingMenu = CraftingMenu.Instance();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(item != null) 
        {
            craftingMenu.ShowTooltip(item);
        }
    }

    public void Craftable(bool value) 
    {
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
