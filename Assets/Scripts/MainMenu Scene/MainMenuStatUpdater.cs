using UnityEngine;
using TMPro;
using System;
/// <summary>
/// Update the UI with Player Stats.
/// </summary>
public class MainMenuStatUpdater : MonoBehaviour
{
    public TextMeshProUGUI userName, userHours, userSp;
    public ExpBar expBar;
    public PlayerStats playerStats;
    /// <summary>
    /// Stat Updater 
    /// </summary>
    void Start() 
    {
        UpdateText();
    }
    /// <summary>
    /// Update UI Text from PlayerPrefs and Stats. Does not call web request.
    /// </summary>
    public void UpdateText() 
    {
        userName.text = PlayerPrefs.GetString("username");
        userHours.text = Math.Round(playerStats.playerHours,2) + " HOURS SURVIVED";
        userSp.text = playerStats.playerCoins.ToString();
        expBar.SetExp(playerStats.playerExp);
    }
}
