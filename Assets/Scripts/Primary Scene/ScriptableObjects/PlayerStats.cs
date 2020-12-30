﻿using UnityEngine;

public class PlayerStats : ScriptableObject
{
    public string playerName = ""; //Player Username
    public int playerExp = 0; //Player Experience
    public int playerCoins = 0; //Server Coin Amount
    public float playerHours = 0.0F; //Hours Played
    public string storeData = "";
    public string notifyData = "";

    public void Align(StatRequestData requestData) 
    {
        playerExp = requestData.stats.exp;
        playerCoins = requestData.stats.coins;
        playerHours = requestData.stats.hours;
        storeData = requestData.stats.store_data;
        notifyData = requestData.stats.notify_data;
    }

}
