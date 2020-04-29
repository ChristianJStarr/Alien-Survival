using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System;

public class PlayerLogin : MonoBehaviour
{
    public GameObject coinNotify;
    private static PlayerLogin playerLogin;
    public PlayerStats playerStats;
    public AutoSave autoSave;

    public static PlayerLogin Instance() 
    {
        if (!playerLogin)
        {
            playerLogin = FindObjectOfType(typeof(PlayerLogin)) as PlayerLogin;
        }
        return playerLogin;
    }

    void Update() 
    {
    }

    void Start() 
    {
    }

    public void SetPlayerStats() 
    {
        //Debug.Log("NETWORK - Setting Stats");
        WWWForm form = new WWWForm();
        form.AddField("all", 2);
        form.AddField("username", PlayerPrefs.GetString("username"));
        form.AddField("password", PlayerPrefs.GetString("password"));
        form.AddField("coins", playerStats.playerCoins);
        form.AddField("hours", playerStats.playerHours.ToString());
        form.AddField("exp", playerStats.playerExp);
        UnityWebRequest w = UnityWebRequest.Post("https://outurer.com/stats.php", form);
        StartCoroutine(SetStatsWait(w));

    }
    private IEnumerator SetStatsWait(UnityWebRequest _w)
    {
        yield return _w.SendWebRequest();
        if (_w.downloadHandler.text.StartsWith("TRUE"))
        {
            //Debug.Log("Network - Set Stats Success");
        }
    }
    public void AddCoin(int value) 
    {
        playerStats.playerCoins += value;
    }
    public bool RemoveCoin(int value) 
    {
        if (playerStats.playerCoins >= value) 
        {
            playerStats.playerCoins -= value;
            SetPlayerStats();
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CanRemoveCoin(int value)
    {
        if (playerStats.playerCoins >= value)
        {
            return true;
        }
        else
        {
            coinNotify.SetActive(true);
            return false;
        }
    }
    public void CloseNotify() 
    {
        coinNotify.SetActive(false);
    }
}
