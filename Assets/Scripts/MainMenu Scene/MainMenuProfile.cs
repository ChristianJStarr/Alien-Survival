
using UnityEngine;
using TMPro;
using System;

public class MainMenuProfile : MonoBehaviour
{
    #region Singleton
    public static MainMenuProfile Singleton;
    private void Awake() { Singleton = this; }
    #endregion

    public TextMeshProUGUI ui_timeSurvived, ui_kills, ui_deaths, ui_kdRatio, ui_percentile;
    public PlayerStats playerStats;


    private void Start()
    {
        UpdateProfile_Task();
    }

    public static void UpdateProfileMenu() 
    {
        if(Singleton != null) 
        {
            Singleton.UpdateProfile_Task();
        }
    }

    private void UpdateProfile_Task() 
    {
        TimeSpan time = TimeSpan.FromHours(playerStats.playerHours);
        
        ui_timeSurvived.text = time.Hours + "h " + time.Minutes + "m";
        ui_timeSurvived.text = string.Format("{0}h {1}m", time.Hours, time.Minutes);
        ui_kills.text = playerStats.playerKills.ToString();
        ui_deaths.text = playerStats.playerDeaths.ToString();
        ui_percentile.text = string.Format("{0}%", playerStats.playerPercentile);
        ui_kdRatio.text = GetRatio(playerStats.playerKills, playerStats.playerDeaths);
    }

    private string GetRatio(int a, int b) 
    {
        if(a != 0 && b != 0) 
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                {
                    a %= b;
                }
                else 
                {
                    b %= a;
                }
            }
            int ratio = a == 0 ? b : a;
            a /= ratio;
            b /= ratio;
        }
        return string.Format("{0}:{1}", a, b);
    }
}
