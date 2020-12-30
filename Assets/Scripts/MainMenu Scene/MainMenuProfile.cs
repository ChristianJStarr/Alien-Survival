
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
        int rat = Ratio(playerStats.playerKills, playerStats.playerDeaths);

        ui_timeSurvived.text = time.Hours + "h " + time.Minutes + "m";
        ui_timeSurvived.text = string.Format("{0}h {1}m", time.Hours, time.Minutes);
        ui_kills.text = playerStats.playerKills.ToString();
        ui_deaths.text = playerStats.playerDeaths.ToString();
        ui_percentile.text = string.Format("{0}%", playerStats.playerPercentile);
        ui_kdRatio.text = string.Format("{0}:{1}", playerStats.playerKills / rat, playerStats.playerDeaths / rat);
    }

    static int Ratio(int a, int b)
    {
        return b == 0 ? Math.Abs(a) : Ratio(b, a % b);
    }
}
