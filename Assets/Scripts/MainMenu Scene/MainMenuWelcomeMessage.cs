using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuWelcomeMessage : MonoBehaviour
{
    public GameObject messageScreen;
    public MainMenuScript mainMenu;
    public GameObject[] messages;
    public PlayerStats playerStats;

    private void Start()
    {
        if(PlayerPrefs.GetInt("newPlayer") == 1) 
        {
            PlayerPrefs.DeleteKey("newPlayer");
            PlayerPrefs.Save();
            messageScreen.SetActive(true);
            mainMenu.CloseAll();
            messages[0].SetActive(true);
        }
        else if(playerStats.notifyData.Length > 0)
        {
            messageScreen.SetActive(true);
            mainMenu.CloseAll();
            messages[2].SetActive(true);
        }
    }

    public void CloseActive() 
    {
        for (int i = 0; i < messages.Length; i++)
        {
            if (messages[i].activeSelf) 
            {
                messages[i].SetActive(false);
                SetNext(i);
                break;
            }
        }
    }
    private void SetNext(int value) 
    {
        value++;
        if(value == 2) 
        {
            if(playerStats.notifyData.Length > 0) 
            {
                messages[value].SetActive(true);
                return;
            }
            else 
            {
                SetNext(2);
                return;
            }
        }

        if(value < messages.Length) 
        {
            messages[value].SetActive(true);
        }
        else 
        {
            messageScreen.SetActive(false);
        }
    }
}
