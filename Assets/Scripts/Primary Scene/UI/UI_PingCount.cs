
using TMPro;
using MLAPI;
using UnityEngine;
using System.Collections;

public class UI_PingCount : MonoBehaviour
{
    public TextMeshProUGUI countText;
    public Settings settings;
    private bool showPing = false;
    private GameServer gameServer;
    public GameObject toggleObject;
    private int ping = 0;

    private void Start()
    {
        if(NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient) 
        {
            if(GameServer.singleton != null) 
            {
                gameServer = GameServer.singleton;
                Change();
            }
        }
        else 
        {
            Destroy(this);
        }
    }

    //Subscribe to EVENT
    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;//Subscribe to Settings Change Event.
    }

    //Unsubscribe from EVENT
    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;//unSubscribe to Settings Change Event.
    }

    //Change Settings.
    private void Change()
    {
        if(showPing != settings.showPing) 
        {
            if (!showPing) StartCoroutine(UpdateLoop());
            showPing = settings.showPing;
            toggleObject.SetActive(showPing);
        }
    }

    //Update Loop for Calculating Ping
    private IEnumerator UpdateLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(1F);
        while (true) 
        {
            if (showPing) 
            {
                ping = gameServer.GetPlayerPing();
                countText.text = ping + " MS";
                if (ping < 101)
                {
                    ChangePingIconColor(Color.white);
                }
                else if (ping > 100 && ping < 181)
                {
                    ChangePingIconColor(Color.yellow);
                }
                else if (ping > 180)
                {
                    ChangePingIconColor(new Color32(224, 40, 40, 255));
                }
            }
            else 
            {
                break;
            }
            yield return wait;
        }
    }

    //Change Color of Signal Icon
    private void ChangePingIconColor(Color color) 
    {
        if(countText.color != color) 
        {
            countText.color = color;
        }
    }
}
