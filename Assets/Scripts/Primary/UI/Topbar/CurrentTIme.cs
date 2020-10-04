using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrentTIme : MonoBehaviour
{
    public Settings settings;

    public TextMeshProUGUI text;

    private bool showTime;

    int refresh = 20;
    float timer;



    private void Update() 
    {
        if(showTime && Time.unscaledTime > timer) 
        {
            timer = Time.unscaledTime + refresh;
            text.text = DateTime.Now.ToString("h:m tt");
        }
    }

    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;//Subscribe to Settings Change Event.
    }

    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;//unSubscribe to Settings Change Event.
    }


    private void Start() 
    {
        Change();
    }

    public void Change() 
    {
        showTime = settings.showTime;
        if(!showTime && text.gameObject.activeSelf)
        {
            text.gameObject.SetActive(false);
        }
        else if(showTime && !text.gameObject.activeSelf) 
        {
            text.gameObject.SetActive(true);
        }
    }

}
