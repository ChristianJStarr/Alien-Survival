using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tooltip : MonoBehaviour
{
    public Image item_icon;
    public TextMeshProUGUI item_title, item_description, split_button;

    public Transform typeContainer;
    public GameObject typeSlidePrefab;

    //Split Slider
    public Slider splitSlider;
    private int splitAmount;


    //Use Type Slides
    private List<TopToolTypeSlide> typeSlides = new List<TopToolTypeSlide>();

    //Stored Item Information
    private Item storedItem;
    private ItemData storedItemData;

    //Set ItemData to this tooltip.
    public void SetData(ItemData data, Item item)
    {
        item_title.text = data.itemName;
        item_description.text = data.description;
        item_icon.sprite = data.icon;
        storedItemData = data;
        storedItem = item;
        splitSlider.minValue = 1;
        if (item.itemStack > 1)
        {
            splitAmount = item.itemStack / 2;
            splitSlider.value = splitAmount;
            splitSlider.maxValue = item.itemStack - 1;
        }
        else
        {
            splitSlider.maxValue = 1;
        }
        BuildTypes(data.itemUse);
    }

    //Build Type Attribute Slides
    private void BuildTypes(string[] types) 
    {
        HideAllSlides(); //Hide all visible slides
        if (types != null && types.Length != 0) 
        {
            //Not enough slides. Spawn them
            if (types.Length > typeSlides.Count)
            {
                SpawnSlides(types.Length - typeSlides.Count);
            }
            //Apply Data to Slide
            for (int i = 0; i < types.Length; i++)
            {
                string[] data = types[i].Split('-');
                int item_type = Convert.ToInt32(data[0]);
                int type_amount = Convert.ToInt32(data[1]);
                typeSlides[i].UpdateType(item_type, type_amount);
            }
        }
    }

    //Hide All Type Attribute Slides
    private void HideAllSlides() 
    {
        for (int i = 0; i < typeSlides.Count; i++)
        {
            if(typeSlides[i] != null) 
            {
                typeSlides[i].gameObject.SetActive(false);
            }
        }
    }

    //Spawn Type Attribute Slides
    private void SpawnSlides(int amount) 
    {
        for (int i = 0; i < amount; i++)
        {
            typeSlides.Add(Instantiate(typeSlidePrefab, typeContainer).GetComponent<TopToolTypeSlide>());
        }
    }



    //Tooltip Buttons

    //Drop Item
    public void Button_DropItem() 
    {
        if(storedItem != null) 
        {
            PlayerInfoManager.singleton.RemoveItemBySlot(storedItem.currSlot);
        }
    }
    //Slider Changed
    public void Slider_HasChanged() 
    {
        splitAmount = (int) splitSlider.value;
        Slider_UpdateButton();
    }
    //Set Slider (1/2)
    public void Slider_SetHalf() 
    {
        if (storedItem.itemStack > 1)
        {
            splitAmount = storedItem.itemStack / 2;
            splitSlider.value = splitAmount;

        }
        else
        {
            splitAmount = 1;
            splitSlider.value = 1;
        }
    }
    //Set Slider (1/4)
    public void Slider_SetFourth() 
    {
        if (storedItem.itemStack > 3)
        {
            splitAmount = storedItem.itemStack / 4;
            splitSlider.value = splitAmount;
        }
        else
        {
            splitAmount = 1;
            splitSlider.value = 1;
        }
    }
    //Set Slider (1/8)
    public void SliderSetEigth() 
    {
        if(storedItem.itemStack > 7) 
        {
            splitAmount = storedItem.itemStack / 8;
            splitSlider.value = splitAmount;
        }
        else
        {
            splitAmount = 1;
            splitSlider.value = 1;
        }
    }
    //Update Split Button
    private void Slider_UpdateButton() 
    {
        split_button.text = "SPLIT " + splitAmount;
    }
    //Split
    public void Slider_Splt() 
    {
        if(storedItem != null && splitAmount > 0 && splitAmount < storedItem.itemStack) 
        {
            PlayerInfoManager.singleton.SplitItemBySlot(storedItem.currSlot, splitAmount);
        }
    }
}
