using UnityEngine;
using TMPro;

public class ProfileCorner : MonoBehaviour
{
    public TextMeshProUGUI userName, userHours, userSp, joinSp;
    public ExpBar expBar;
    public PlayerStats playerStats;
    void Start() 
    {
        UpdateText();
    }
    void Update() 
    {
        if (userSp.text != playerStats.playerCoins + " SP")
        {
            UpdateText();
        }
        if (joinSp.text != playerStats.playerCoins + " SP")
        {
            UpdateText();
        }
    }



    public void UpdateText() 
    {
        userName.text = playerStats.playerName;
        userHours.text = playerStats.playerHours + " HOURS SURVIVED";
        userSp.text = playerStats.playerCoins + " SP";
        joinSp.text = userSp.text;
        expBar.SetExp(playerStats.playerExp);
    }
}
