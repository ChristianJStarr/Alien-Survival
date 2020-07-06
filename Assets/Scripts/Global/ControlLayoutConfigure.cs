using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControlLayoutConfigure : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public GameObject button_Container;
    private List<Image> buttons;

    private void Start()
    {
        buttons = new List<Image>();
        if(button_Container != null) 
        {
            Image[] images = button_Container.GetComponentsInChildren<Image>();
            if(images != null) 
            {
                foreach (Image image in images)
                {
                    buttons.Add(image);
                }
            }
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
    }


}
