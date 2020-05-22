using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControlLayoutConfigure : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public GameObject button_Container;
    private List<Image> buttons = new List<Image>();


    private void Start()
    {
        foreach (Image item in button_Container.GetComponentsInChildren<Image>()) 
        {
            buttons.Add(item);
            
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }


}
