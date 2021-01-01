using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class MainMenuStatUpdater : MonoBehaviour
{
    #region Singleton
    public static MainMenuStatUpdater Singleton;
    #endregion

    public TextMeshProUGUI userName, userHours, userSp;
    public UI_ExpBar expBar;
    public PlayerStats playerStats;
    public MainMenuWelcomeMessage welcomeMessage;

    string hours_survived = "HOURS SURVIVED";
    string level = "LEVEL";


    private void Awake() 
    {
        Singleton = this;
        LanguageChanged();
    }
    private void Start() 
    {
        UpdateMenu_Task();
    }

    #region Multi-Lang System
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
    #endregion

    public static void UpdateMenuStats() 
    {
        if(Singleton != null) 
        {
            Singleton.UpdateMenu_Task();
        }
    }
    public void UpdateMenu_Task() 
    {
        if (playerStats) 
        {
            userName.text = PlayerPrefs.GetString("username");
            userHours.text = Math.Round(playerStats.playerHours, 2) + " " + hours_survived;
            userSp.text = playerStats.playerCoins.ToString();
            expBar.SetExp(playerStats.playerExp, level);
            if (playerStats.notifyData.Length > 0)
            {
                welcomeMessage.ShowNotify();
            }
            StartCoroutine(UIUpdateFix());
        }
    }
    public IEnumerator UIUpdateFix()
    {
        WaitForSeconds wait = new WaitForSeconds(0.25F);
        string text = playerStats.playerCoins.ToString();
        yield return wait;
        userSp.text = text + " ";
        yield return wait;
        userSp.text = text;
        yield return wait;
        userSp.text = text + " ";
        yield return wait;
        userSp.text = text;
    }
}
