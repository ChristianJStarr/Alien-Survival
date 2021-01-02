using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlideModule : MonoBehaviour
{
    public Vector2 offPos;
    public Vector2 onPos;
    public int uiID;
    public RectTransform rect;
    public bool allowActivateOnHover;


    // UI Data
    public bool needsUIData;
    public UIData storedUIData;



    public void UpdateSlide(UIData storedData = null) 
    {
    
    }

}
