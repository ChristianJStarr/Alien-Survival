using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopToolTipHandler : MonoBehaviour
{
    public Image item_icon;
    public TextMeshProUGUI item_title, item_description;

    public Transform typeContainer;
    public GameObject typeSlidePrefab;

    //Use Type Slides
    private List<TopToolTypeSlide> typeSlides = new List<TopToolTypeSlide>();

    //Stored Item Information
    private Item storedItem;
    private ItemData storedItemData;

    public void SetData(ItemData data, Item item)
    {
        if(data != null && item != null) 
        {
            HandleData(data);
            HandleItem(item);
        }
    }

    private void HandleData(ItemData data) 
    {
        item_title.text = data.name;
        item_description.text = data.description;
        item_icon.sprite = data.icon;
        BuildTypes(data.itemUse);
        storedItemData = data;
    }

    private void HandleItem(Item item)
    {
        storedItem = item;
    }

    private void BuildTypes(string[] types) 
    {
        for (int i = 0; i < typeSlides.Count; i++)
        {
            if (typeSlides[i] != null)
            {
                Destroy(typeSlides[i].gameObject);
                typeSlides.RemoveAt(i);
            }

        }
        if (types != null)
        {
            foreach (string item in types)
            {
                string[] type = item.Split('-');
                int item_type = Convert.ToInt32(type[0]);
                int type_amount = Convert.ToInt32(type[1]);
                TopToolTypeSlide slide = Instantiate(typeSlidePrefab, typeContainer).GetComponent<TopToolTypeSlide>();
                slide.UpdateType(item_type, type_amount);
                typeSlides.Add(slide);
            }
        }
    }
}
