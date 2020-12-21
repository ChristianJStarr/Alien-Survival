using UnityEngine;
using TMPro;
using System;
public class MainMenuStatUpdater : MonoBehaviour
{
    public TextMeshProUGUI userName, userHours, userSp;
    public UI_ExpBar expBar;
    public PlayerStats playerStats;


    string hours_survived = "HOURS SURVIVED";
    string level = "LEVEL";


    private void Awake() 
    {
        LanguageChanged();
    }

    private void Start() 
    {
        UpdateText();
    }

    private void OnEnable()
    {
        MultiLangSystem.ChangedLanguage += LanguageChanged;
    }
    private void OnDisable()
    {
        MultiLangSystem.ChangedLanguage -= LanguageChanged;
    }

    public void LanguageChanged() 
    {
        LangDataSingle data1 = MultiLangSystem.GetLangDataFromKey("level");
        if (data1 != null)
        {
            level = data1.text;
        }
        LangDataSingle data2 = MultiLangSystem.GetLangDataFromKey("hourssurvived");
        if (data2 != null)
        {
            hours_survived = data2.text;
        }
    }


    public void UpdateText() 
    {
        userName.text = PlayerPrefs.GetString("username");
        userHours.text = Math.Round(playerStats.playerHours,2) + " " + hours_survived;
        userSp.text = playerStats.playerCoins.ToString();
        expBar.SetExp(playerStats.playerExp, level);
        
    }
}
