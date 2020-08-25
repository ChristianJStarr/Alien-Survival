
using TMPro;
using MLAPI;
using UnityEngine;
using System.Collections;

public class PingCounter : MonoBehaviour
{
    public GameObject pingPanel;
    public float refresh = 20;
    private int timer, avgPing;
    public TextMeshProUGUI countText;
    public Settings settings;
    private bool showPing = false;
    private GameServer gameServer;
    private ulong clientId;

    private void Start()
    {
        clientId = NetworkingManager.Singleton.LocalClientId;
        gameServer = GameServer.singleton;
        showPing = settings.showPing; //Get showPing bool.
        pingPanel.SetActive(showPing); //Activate/Deactivate fpsPanel
        StartCoroutine(UpdateLoop());
    }


    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;//Subscribe to Settings Change Event.
    }

    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;//unSubscribe to Settings Change Event.
    }

    //Change Settings.
    private void Change()
    {
        showPing = settings.showPing;
        pingPanel.SetActive(showPing);
    }

    private IEnumerator UpdateLoop()
    {
        if (showPing)
        {
            gameServer.GetPlayerPing(clientId, returnValue =>
            {
                if (returnValue < 0) { returnValue = 0; }
                avgPing += returnValue;
                timer++;
                countText.text = (avgPing / timer) + " MS";
                
            });
        }
        yield return new WaitForSeconds(1F);
        StartCoroutine(UpdateLoop());
    }
}
