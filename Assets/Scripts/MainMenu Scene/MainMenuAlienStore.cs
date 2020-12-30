using System;
using TMPro;
using UnityEngine;

public class MainMenuAlienStore : MonoBehaviour
{
    public WebServer webServer;
    public TextMeshProUGUI pointCount;
    public PlayerStats playerStats;
    public MainMenuStatUpdater statUpdater;

    public AlienStoreSlide[] storeItems;

    private void Start()
    {
        UpdateStats();
    }

    public void PurchaseItem(int itemId) 
    {
        webServer.AlienStorePurchase(PlayerPrefs.GetInt("userId"), PlayerPrefs.GetString("authKey"), itemId);
    }

    public void UpdateStats() 
    {
        pointCount.text = playerStats.playerCoins.ToString();
        foreach (AlienStoreSlide slide in storeItems)
        {
            if (playerStats.playerExp >= slide.exp) 
            {
                if (playerStats.storeData.Length > 0) 
                {
                    string[] storeData = playerStats.storeData.Split(',');
                    foreach (string item in storeData)
                    {
                        int itemId = Convert.ToInt32(item);
                        if (itemId == slide.itemId)
                        {
                            slide.SetSlide(false, true);
                            break;
                        }
                        else
                        {
                            slide.SetSlide(false, false);
                            break;
                        }
                    }
                }
            }
            else 
            {
                slide.SetSlide(true, false);
            }
        }
    }
}
