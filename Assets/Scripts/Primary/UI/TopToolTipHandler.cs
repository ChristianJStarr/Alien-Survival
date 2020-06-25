using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopToolTipHandler : MonoBehaviour
{
    public Image item_icon;
    public TextMeshProUGUI item_title, item_description;

    public Transform typeContainer;
    public GameObject typeSlidePrefab;

    private List<TopToolTypeSlide> typeSlides;
    private Item cacheItem;
    private ItemData cacheData;

    private void Start()
    {
        typeSlides = new List<TopToolTypeSlide>();
    }

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
    }
    private void HandleItem(Item item)
    {
        cacheItem = item;

    }
    private void BuildTypes(string[] types) 
    {
        if (typeSlides != null)
        {
            if (typeSlides.Count > 0)
            {
                foreach (TopToolTypeSlide slide in typeSlides.ToList())
                {
                    Destroy(slide.gameObject);
                    typeSlides.Remove(slide);
                }
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
