using UnityEngine;
using TMPro;
/// <summary>
/// Update the UI with Player Stats.
/// </summary>
public class MainMenuStatUpdater : MonoBehaviour
{
    public TextMeshProUGUI userName, userHours, userSp, joinSp;
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
        userHours.text = playerStats.playerHours + " HOURS SURVIVED";
        userSp.text = playerStats.playerCoins + " SP";
        joinSp.text = userSp.text;
        expBar.SetExp(playerStats.playerExp);
    }
}
