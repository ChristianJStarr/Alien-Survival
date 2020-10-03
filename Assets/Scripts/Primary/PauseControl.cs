using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseControl : MonoBehaviour
{
    //What: Pause Menu Controller.
    //Where: Primary Scene / Interface

    public bool gamePaused; //Is the game paused.
    public GameObject pauseMenu; //Pause Menu container.
    public GameObject pauseButtons; //Pause Menu buttons.
    public GameObject settingsMenu; //pause Menu Settings container.
    public TextMeshProUGUI disconnectButtonText; //Disconnect Button
    public Button disconnectButton;

    void Start()
    {
        if (pauseMenu.activeSelf == true) //If pause menu was left active.
        {
            pauseMenu.SetActive(false);//Turn off pause menu.
        }
        gamePaused = false; //Game is not paused by default.
    }


    //Pause button function. Called from buttons on topbar and inside pause menu.
    public void ButtonPauseGame() 
    {
        //Toggle for pause menu
        if (!gamePaused) //If game is not paused and button was clicked.
        {
            //Pause the game.
            pauseMenu.SetActive(true);//Show the pause menu.
            gamePaused = true;

            InterfaceHider.Singleton.HideAllInterfaces();
            
            StartCoroutine(BattleLog());
        }
        else //If game is paused and button was clicked.
        {
            //Un-Pause the game.
            pauseMenu.SetActive(false);//Hide the pause menu.
            gamePaused = false;

            InterfaceHider.Singleton.ShowAllInterfaces();
        }
    }


    //Settings button function. Called from buttons inside of pause menu and settings menu.
    public void ButtonSettings() 
    {
        if (settingsMenu.activeSelf) //Is the settings menu open?
        {
            pauseButtons.SetActive(true);
            settingsMenu.SetActive(false);
        }
        else 
        {
            pauseButtons.SetActive(false);
            settingsMenu.SetActive(true);
        }
    }


    private IEnumerator BattleLog() 
    {
        disconnectButton.interactable = false;
        WaitForSeconds wait = new WaitForSeconds(1F);
        int pos = 5;
        while(true) 
        {
            if (!gamePaused) 
            {
                break;
            }
            disconnectButtonText.text = "DISCONNECT (" + pos + ")";
            pos--;
            yield return wait;
            if(pos == 0) 
            {
                disconnectButtonText.text = "DISCONNECT";
                disconnectButton.interactable = true;
                break;
            }
        }
    }


    //Exit to Main Menu button function. Called from button inside of pause menu.
    public void ButtonExitMain() 
    {
        gamePaused = false;
        PlayerActionManager.singleton.RequestDisconnect();
    }
}
