using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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
    public void Incoming(int health, int exp, int water, int food) 
    {
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


    public void Show() 
    {
        gameObject.SetActive(true);
    }
    public void Hide() 
    {
        gameObject.SetActive(false);
    }
}
