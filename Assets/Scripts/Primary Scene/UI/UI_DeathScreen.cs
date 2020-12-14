using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class UI_DeathScreen : MonoBehaviour
{
    public TextMeshProUGUI t_time;
    public Volume darkPostFx;


    //Exit Main Menu Button
    public void ExitToMainMenu() 
    {
        PlayerUIManager.Singleton.RequestDisconnect();
    }

    //Enable Death Screen
    public void EnableScreen(TimeSpan timeSurvived) 
    {
        DeathCam.Show();
        SetTimeText(timeSurvived);
        ToggleDarkPostFx(true);
        gameObject.SetActive(true);
    }

    //Disable Death Screen
    public void DisableScreen() 
    {
        DeathCam.Hide();
        ToggleDarkPostFx(false);
        gameObject.SetActive(false);
    }

    //Toggle Dark Grayscale Post Fx
    private void ToggleDarkPostFx(bool value) 
    {
        if (value) 
        {
            LerpPostFx(1, 2);
        }
        else 
        {
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

    //PostFx Weight Lerp
    private IEnumerator LerpPostFx(float target, float duration) 
    {
        float time = 0;
        float startValue = darkPostFx.weight;
        while (time < duration)
        {
            darkPostFx.weight = Mathf.Lerp(startValue, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        darkPostFx.weight = target;
    }

}
