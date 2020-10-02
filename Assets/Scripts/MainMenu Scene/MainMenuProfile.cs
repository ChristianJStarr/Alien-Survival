
using UnityEngine;
using TMPro;
using System;

public class MainMenuProfile : MonoBehaviour
{
    public TextMeshProUGUI ui_profileTitle, ui_timeSurvived, ui_kills, ui_deaths, ui_kdRatio, ui_percentile;
    public GameObject registerButton;
    
    public PlayerStats playerStats;




    private void Start()
    {
        ui_profileTitle.text = playerStats.playerName + "'s PROFILE";
        TimeSpan time = TimeSpan.FromHours(playerStats.playerHours);
        ui_timeSurvived.text = time.Hours + "h " + time.Minutes + "m";
        if (playerStats.playerName.Contains("-Guest"))
        {
            registerButton.SetActive(true);
        }
    }

    public void RegisterAccount() 
    {
        
    }

}
