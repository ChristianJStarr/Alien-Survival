using Photon.Realtime;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseControl : MonoBehaviour
{
    public bool gamePaused;
    public GameObject pauseMenu;
    void Start()
    {
        if (pauseMenu.activeSelf == true) { pauseMenu.SetActive(false); }
        gamePaused = false;
    }
    public void PauseGame() 
    {
        if (gamePaused == false) 
        {
            pauseMenu.SetActive(true);
            gamePaused = true;
        }
        else 
        {
            pauseMenu.SetActive(false);
            gamePaused = false;
        }
    }
    public void GoMainMenu() 
    {
        gamePaused = false;
        SceneManager.LoadScene(0);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
    }
}
