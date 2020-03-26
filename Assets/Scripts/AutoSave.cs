using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AutoSave : MonoBehaviour
{
    public GameObject saveTile;
    private Animation anim;
    private TextMeshProUGUI text;
    public PlayerStats playerStats;
    int saveRound;
    void Start() 
    {
        anim = saveTile.GetComponent<Animation>();
        text = saveTile.GetComponentInChildren<TextMeshProUGUI>();
        saveRound = 0;
        RunAutoSave();
    }

    IEnumerator AutoSaveLoop() 
    {
        yield return new WaitForSeconds(300);
        if (saveRound < 3) 
        {
            saveRound++;
            RunAutoSave();
            AutoSaveOn();
            LocalPlayerStats();
        }
        else 
        {
            saveRound = 0;
            CloudPlayerStats();
            AutoSaveOn();
            RunAutoSave();
        }
        
        CloudPlayerStats();
    }
    void RunAutoSave() 
    {
        StartCoroutine(AutoSaveLoop());
    }

    public void AutoSaveOn() 
    {
        anim.Play("AutoSaveOn");
        text.text = "AUTO-SAVING...";
        StartCoroutine(OffWait());
    }

    IEnumerator OffWait() 
    {
        yield return new WaitForSeconds(2);
        AutoSaveOff();
    }

    public void AutoSaveOff() 
    {
        text.text = "COMPLETE";
        anim.Play("AutoSaveOff");
    }
    public void CloudPlayerStats()
    {
        WWWForm form = new WWWForm();
        form.AddField("all", 2);
        form.AddField("username", PlayerPrefs.GetString("username"));
        form.AddField("password", PlayerPrefs.GetString("password"));
        form.AddField("coins", playerStats.playerCoins);
        form.AddField("hours", playerStats.playerHours.ToString());
        form.AddField("exp", playerStats.playerExp);
        form.AddField("health", playerStats.playerHealth);
        form.AddField("water", playerStats.playerWater);
        form.AddField("food", playerStats.playerFood);
        form.AddField("recent", System.DateTime.Now.ToString());
        UnityWebRequest w = UnityWebRequest.Post("https://outurer.com/stats.php", form);
        StartCoroutine(SetStatsWait(w));

    }
    private IEnumerator SetStatsWait(UnityWebRequest _w)
    {
        yield return _w.SendWebRequest();
        Debug.Log(_w.downloadHandler.text);
        if (_w.downloadHandler.text.StartsWith("TRUE"))
        {
            Debug.Log("Network - Auto-Save Success");
        }
    }
    public void LocalPlayerStats() 
    {
        PlayerPrefs.SetString("recent", System.DateTime.Now.ToString());
        PlayerPrefs.SetInt("coins", playerStats.playerCoins);
        PlayerPrefs.SetFloat("hours", playerStats.playerHours);
        PlayerPrefs.SetInt("exp", playerStats.playerExp);
        PlayerPrefs.SetInt("health", playerStats.playerHealth);
        PlayerPrefs.SetInt("water", playerStats.playerWater);
        PlayerPrefs.SetInt("food", playerStats.playerFood);
    }

}
