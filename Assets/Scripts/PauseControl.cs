using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

//Pause Menu Controller


public class PauseControl : MonoBehaviour
{

    public bool gamePaused; //Is the game paused.
    public GameObject pauseMenu;
    public GameObject pauseButtons;
    public GameObject settingsMenu;
    private PhotonRoom photonRoom;
    public AutoSave autoSave;
    public ControlControl controls;

    void Start()
    {
        photonRoom = FindObjectOfType<PhotonRoom>();
        if (pauseMenu.activeSelf == true) 
        {
            pauseMenu.SetActive(false);
        }
        gamePaused = false;
    }


    //Pause button function. Called from buttons on topbar and inside pause menu.
    public void ButtonPauseGame() 
    {
        //Toggle for pause menu
        if (!gamePaused) 
        {
            pauseMenu.SetActive(true);
            controls.Hide(); //Hides the UI Controls layer.
            autoSave.Save(); //Saves the game
            gamePaused = true;
            
        }
        else 
        {
            pauseMenu.SetActive(false);
            controls.Show(); //Shows the UI Controls layer.
            gamePaused = false;
        }
    }


    //Settings button function. Called from buttons inside of pause menu and settings menu.
    public void ButtonSettings() 
    {
        pauseButtons.SetActive(false);
        settingsMenu.SetActive(true);
    }


    //Exit to Main Menu button function. Called from button inside of pause menu.
    public void ButtonExitMain() 
    {
        gamePaused = false;
        photonRoom.LeaveGame();
        SceneManager.LoadScene(1); //Loads the Main Menu scene.
        PhotonNetwork.LeaveRoom(); //Leave the game room. Returns to Master Server and auto joins Lobby.
    }
}
