using System;
using TMPro;
using UnityEngine;

public class UI_DeathScreen : MonoBehaviour
{
    public TextMeshProUGUI t_time;

    public void ExitToMainMenu() 
    {
        PlayerActionManager.singleton.RequestDisconnect();
    }

    public void EnableScreen(TimeSpan timeSurvived) 
    {
        SetTimeText(timeSurvived);
        gameObject.SetActive(true);
    }


    public void DisableScreen() 
    {
        gameObject.SetActive(false);
    }


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
