using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class UI_DeathScreen : MonoBehaviour
{
    public TextMeshProUGUI t_time;
    public Volume darkPostFx;
    public int lerpSpeed = 5;
    private bool canLerp = false;
    private float targetWeight = 0;


    private void Update() 
    {
        if (canLerp) 
        {
            if(darkPostFx.weight != targetWeight) 
            {
                darkPostFx.weight = Mathf.Lerp(darkPostFx.weight, targetWeight, lerpSpeed * Time.deltaTime);
            }
        }
    }

    //Exit Main Menu Button
    public void ExitToMainMenu() 
    {
        PlayerUIManager.Singleton.RequestDisconnect();
    }

    //Enable Death Screen
    public void EnableScreen(TimeSpan timeSurvived) 
    {
        DeathCam.Singleton.Activate(true);
        SetTimeText(timeSurvived);
        ToggleDarkPostFx(true);
        gameObject.SetActive(true);
    }

    //Disable Death Screen
    public void DisableScreen() 
    {
        DeathCam.Singleton.Activate(false);
        gameObject.SetActive(false);
        ToggleDarkPostFx(false);
    }

    //Toggle Dark Grayscale Post Fx
    private void ToggleDarkPostFx(bool value) 
    {
        if (value) 
        {
            targetWeight = 1;
            canLerp = true;
        }
        else 
        {
            targetWeight = 0;
            canLerp = false;
            darkPostFx.weight = 0;
        }
    }

    //Set Time Survived Text
    private void SetTimeText(TimeSpan span) 
    {
        if(span.Hours > 0) 
        {
            t_time.text = span.Hours + "h " + span.Minutes + "m " + span.Seconds + "s Survived";
        }
        else if(span.Minutes > 0) 
        {
            t_time.text = span.Minutes + "m " + span.Seconds + "s Survived";
        }
        else if(span.Seconds > 0)  
        {
            t_time.text = span.Seconds + "s Survived";
        }
        else 
        {
            t_time.text = "";
        }
    }
}
