using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using MLAPI;
using System;

public class Topbar : InterfaceMenu
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



    public override void Enable(string data) 
    {
        gameObject.SetActive(true);
        UpdateData(data);
    }

    public override void Disable() 
    {
        gameObject.SetActive(false);
    }

    //Update player info
    public override void UpdateData(string data) 
    {
        string[] datas = data.Split(',');
        int exp = Convert.ToInt32(datas[0]);
        int health = Convert.ToInt32(datas[1]);
        int water = Convert.ToInt32(datas[2]);
        int food = Convert.ToInt32(datas[3]);

        userExp_slider.value = 100;
        userHealth_slider.value = health;
        userWater_slider.value = water;
        userFood_slider.value = food;
        userExp_text.text = "LEVEL " + 1;
        userHealth_text.text = "HP " + health;
        userWater_text.text = "WATER " + water;
        userFood_text.text = "FOOD " + food;
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
        ServerConnect.singleton.RequestPing(ping => {
            netStat_ping.text = "PING:" + ping + "ms";
        });
        yield return new WaitForSeconds(5F);
        if (netStatOpen) 
        {
            StartCoroutine(GetPing());
        }
    }
}
