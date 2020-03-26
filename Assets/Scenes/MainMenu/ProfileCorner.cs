using Photon.Realtime;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProfileCorner : MonoBehaviour
{
    public TextMeshProUGUI userName, userHours, userSp, joinSp;
    public ExpBar expBar;
    public PlayerStats playerStats;
    void Start() 
    {
        userName.text = playerStats.playerName;
        userHours.text = playerStats.playerHours + " HOURS PLAYED";
        userSp.text = playerStats.playerCoins + " SP";
        joinSp.text = userSp.text;
        expBar.SetExp(playerStats.playerExp);
    }
}
