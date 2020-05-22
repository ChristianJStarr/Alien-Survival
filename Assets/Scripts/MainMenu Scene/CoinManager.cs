using UnityEngine;
/// <summary>
/// Server Coin Manager. Likely Depreciated.
/// </summary>
public class CoinManager : MonoBehaviour
{
    public GameObject coinNotify;
    public PlayerStats playerStats;
    public WebServer webServer;
    public MainMenuStatUpdater mainMenuStatUpdater;

    public void AddCoin(int value) 
    {
        mainMenuStatUpdater.UpdateText();
        playerStats.playerCoins += value;
        webServer.StatSend(PlayerPrefs.GetInt("userId"), PlayerPrefs.GetString("authKey"), playerStats, onRequestFinished =>
        {
            if (onRequestFinished)
            {
                Debug.Log("Network - Web - Player Stats save successful.");
            }
            else
            {
                Debug.Log("Network - Web - Failed to save Player Stats.");
            }
        });
    }
    public bool RemoveCoin(int value) 
    {
        if (playerStats.playerCoins >= value) 
        {
            mainMenuStatUpdater.UpdateText();
            playerStats.playerCoins -= value;
            webServer.StatSend(PlayerPrefs.GetInt("userId"), PlayerPrefs.GetString("authKey"), playerStats, onRequestFinished => 
            {
                if (onRequestFinished) 
                {
                    Debug.Log("Network - Web - Player Stats save successful.");
                }
                else 
                {
                    Debug.Log("Network - Web - Failed to save Player Stats.");
                }
            });
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CanRemoveCoin(int value)
    {
        if(coinNotify == null) 
        {
            return true;
        }
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
        if(coinNotify == null) 
        {
            return;
        }
        coinNotify.SetActive(false);
    }
}
