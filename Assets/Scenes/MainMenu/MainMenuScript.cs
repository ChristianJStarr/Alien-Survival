using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public GameObject mainScreen;
    public GameObject loadScreen;
    public GameObject profileMenu;
    public GameObject settingsMenu;
    public GameObject onlineMenu;



    public void PlayMenu() 
    {
        mainScreen.SetActive(false);
        onlineMenu.SetActive(true);
    }
    public void ProfileMenu() 
    {
        mainScreen.SetActive(false);
        profileMenu.SetActive(true);
    }
    public void SettingsMenu() 
    {
        mainScreen.SetActive(false);
        settingsMenu.SetActive(true);
    }
    public void CloseMenu() 
    {

            onlineMenu.SetActive(false);

            profileMenu.SetActive(false);
  
            settingsMenu.SetActive(false);

            mainScreen.SetActive(true);

    }
    public void JoinServer()
    {
        SceneManager.LoadScene(1);
        loadScreen.SetActive(true);

    }
}
