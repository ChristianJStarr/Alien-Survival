using UnityEngine;
using TMPro;
using System;

public class BlogSlide : MonoBehaviour
{
    public TextMeshProUGUI title;
    public string link;

    public void Set(string t, string l) 
    {
        title.text = t;
        link = l;
    }
    public void OpenLink()
    {
        if (!String.IsNullOrEmpty(link)) 
        {
            Application.OpenURL(link);
        }
    }
}
