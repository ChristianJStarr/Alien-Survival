﻿using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using MLAPI;

public class Topbar : MonoBehaviour
{
    GameServer gameServer;
    public Slider userExp_slider, userHealth_slider, userWater_slider, userFood_slider;
    public TextMeshProUGUI userExp_text, userHealth_text, userWater_text, userFood_text, netStat_ping;


    //NetStat
    public GameObject netStat_window;
    public Image netStat_image;
    public Sprite iconWifi;
    public Sprite iconClose;
    private bool netStatOpen = false;


    private void Start()
    {
        gameServer = GameServer.singleton;
    }

    //Update player info
    public void Incoming(PlayerInfo playerInfo) 
    {
        userExp_slider.value = 100;
        userHealth_slider.value = playerInfo.health;
        userWater_slider.value = playerInfo.water;
        userFood_slider.value = playerInfo.food;
        userExp_text.text = "LEVEL " + 1;
        userHealth_text.text = "HP " + playerInfo.health;
        userWater_text.text = "WATER " + playerInfo.water;
        userFood_text.text = "FOOD " + playerInfo.food;
    }

    public void NetStatToggle() 
    {
        if (netStat_window.activeSelf) 
        {
            netStatOpen = false;
            netStat_window.SetActive(false);
            netStat_image.sprite = iconWifi;
        }
        else 
        {
            netStatOpen = true;
            netStat_window.SetActive(true);
            netStat_image.sprite = iconClose;
            StartCoroutine(GetPing());
        }
    }

    private IEnumerator GetPing() 
    {
        
        yield return new WaitForSeconds(1F);
        ServerConnect.singleton.RequestPing(ping => {
            netStat_ping.text = ping + "ms";
        });
        if (netStatOpen) 
        {
            StartCoroutine(GetPing());
        }
    }
}
