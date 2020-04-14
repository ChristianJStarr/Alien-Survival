using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlControl : MonoBehaviour
{
    public GameObject cover;
    public GameObject leftScreen;
    public GameObject rightScreen;

    private Image[] left;
    private Image[] right;

    void Start() 
    {
        left = leftScreen.GetComponentsInChildren<Image>();
        right = rightScreen.GetComponentsInChildren<Image>();
    }

    public void Hide() 
    {
        Change(false);
    }
    public void Show() 
    {
        Change(true);
    }
    private void Change(bool value)
    {
        if (left == null) 
        {
            left = leftScreen.GetComponentsInChildren<Image>();
        }
        if(right == null) 
        {
            right = rightScreen.GetComponentsInChildren<Image>();
        }

        Color color;
        Color textColor;
        if (value) 
        {
            color = new Color32(0, 0, 0, 70);
            textColor = new Color32(255, 255, 255, 150);
        }
        else 
        {
            color = new Color32(255, 255, 255, 0);
            textColor = new Color32(255, 255, 255, 0);
        }
        foreach (Image image in left)
        {
            image.color = color;
            if(image.GetComponentInChildren<TextMeshProUGUI>() != null) 
            {
                image.GetComponentInChildren<TextMeshProUGUI>().color = textColor;
            }
        }
        foreach (Image image in right)
        {
            image.color = color;
            if (image.GetComponentInChildren<TextMeshProUGUI>() != null)
            {
                image.GetComponentInChildren<TextMeshProUGUI>().color = textColor;
            }
        }
        cover.GetComponent<Image>().raycastTarget = !value;
    }
}
