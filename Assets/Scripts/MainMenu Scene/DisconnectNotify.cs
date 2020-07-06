using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisconnectNotify : MonoBehaviour
{
    public PlayerStats playerStats;
    public TextMeshProUGUI coinText, expText, timeText, serverName;

    void Start()
    {
        if(playerStats.notifyData.Length > 0) 
        {
            string[] datas = playerStats.notifyData.Split(',');
            serverName.text = datas[0];
            timeText.text = GetTimeText(datas[1]);
            expText.text = datas[2];
            coinText.text = datas[3];
            playerStats.notifyData = "";
        }
    }
    private string GetTimeText(string time) 
    {
        float hours = float.Parse(time);
        int minutes = (int)TimeSpan.FromHours(hours).TotalMinutes;
        if(minutes >= 60) 
        {
            int hour = minutes / 60;
            int left = minutes - (hour * 60);
            if(left > 0) 
            {
                return hour + "hr " + left + "m survived.";
            }
            else if(hour == 1)
            {
                return "1 hour survived.";
            }
            else
            {
                return hour + " hours survived";
            }
        }
        else 
        {
            return minutes + " minutes survived.";
        }
    }
}
