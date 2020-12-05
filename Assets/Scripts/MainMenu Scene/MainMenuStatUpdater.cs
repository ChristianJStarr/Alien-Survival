using UnityEngine;
using TMPro;
using System;
public class MainMenuStatUpdater : MonoBehaviour
{
    public TextMeshProUGUI userName, userHours, userSp;
    public UI_ExpBar expBar;
    public PlayerStats playerStats;


    void Start() 
    {
        UpdateText();
    }

    public void UpdateText() 
    {
        userName.text = PlayerPrefs.GetString("username");
        userHours.text = Math.Round(playerStats.playerHours,2) + " HOURS SURVIVED";
        userSp.text = playerStats.playerCoins.ToString();
        expBar.SetExp(playerStats.playerExp);
    }
}
