using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadSceneScript : MonoBehaviour
{
#if !UNITY_SERVER
    //Load Scene Script
    //Gather Player Stats from Web Server then Load into Main Menu
    //Currently Just Loads Into Main Menu
    //Stat Request will be implemented when SteamID stuff is configured.

    public TextMeshProUGUI game_version;
    public PlayerStats playerStats;
    public WebServer webServer;

    private bool loadingLevel = false;

    void Start() 
    {
        game_version.text = GetGameVersion();
        LoadMainMenuScene();
    }

    private void LoadMainMenuScene()
    {
        if (!loadingLevel)
        {
            loadingLevel = true;
            SceneManager.LoadSceneAsync(1);
        }
    }

    private string GetGameVersion() 
    {
        return string.Format("v{0}", Application.version);
    }

#endif
}

