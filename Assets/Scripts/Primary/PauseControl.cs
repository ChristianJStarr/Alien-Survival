using MLAPI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseControl : MonoBehaviour
{
    //What: Pause Menu Controller.
    //Where: Primary Scene / Interface

    public bool gamePaused; //Is the game paused.
    public GameObject pauseMenu; //Pause Menu container.
    public GameObject pauseButtons; //Pause Menu buttons.
    public GameObject settingsMenu; //pause Menu Settings container.
    public ControlControl controls; //Controls Controller script.

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
            controls.Hide(); //Hides the UI Controls layer.
            gamePaused = true;
            
        }
        else //If game is paused and button was clicked.
        {
            //Un-Pause the game.
            pauseMenu.SetActive(false);//Hide the pause menu.
            controls.Show(); //Shows the UI Controls layer.
            gamePaused = false;
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


    //Exit to Main Menu button function. Called from button inside of pause menu.
    public void ButtonExitMain() 
    {
        gamePaused = false;
        NetworkingManager.Singleton.StopClient();
        SceneManager.LoadScene(1);
    }
}
