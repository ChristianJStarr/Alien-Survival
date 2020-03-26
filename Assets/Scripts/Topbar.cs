using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Topbar : MonoBehaviour
{
    private int userExp, userHealth, userWater, userFood;
    public Slider userExp_slider, userHealth_slider, userWater_slider, userFood_slider;
    public TextMeshProUGUI userExp_text, userHealth_text, pingText;
    public PlayerStats playerStats;
    public RawImage pingImage;
    public bool showPing = false;


    void Update() 
    {
        UpdateUIBar();
        GetPing();
    }

    void UpdateUIBar() 
    {
        userExp = playerStats.playerExp;
        userHealth = playerStats.playerHealth;
        userWater = playerStats.playerWater;
        userFood = playerStats.playerFood;

        int level = GetLevel(userExp);

        userExp_slider.value = (userExp - (level * 1000)) / 10; ;
        userHealth_slider.value = userHealth;
        userWater_slider.value = userWater;
        userFood_slider.value = userFood;

        userExp_text.text = "LEVEL " + level;
        userHealth_text.text = "HP " + userHealth;
    }
    public int GetLevel(int value)
    {
        value = value / 1000;

        if (value <= 0)
        {
            return 1;
        }
        else
        {
            return value;
        }
    }
    private void GetPing() 
    {
        int ping = PhotonNetwork.GetPing();
        if (showPing)
        {
            if (pingImage.enabled)
            {
                pingImage.enabled = false;
            }
            pingText.text = ping + "ms";
        }
        else
        {
            if (!pingImage.enabled)
            {
                pingImage.enabled = true;
                pingText.text = "";
            }
            Color green = new Color32(63, 191, 63, 200);
            Color yellow = new Color32(191, 191, 63, 200);
            Color red = new Color32(191, 63, 63, 200);

            if (ping < 101 && ping != 0)
            {
                pingImage.color = green;
            }
            else if (ping > 100 && ping < 151)
            {
                pingImage.color = yellow;
            }
            else if (ping > 150)
            {
                pingImage.color = red;
            }
            else 
            {
                pingImage.color = red;
            }
        }
    }
    public void ShowPing() 
    {
        showPing = !showPing;
    }
}
