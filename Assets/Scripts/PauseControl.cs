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
    private PhotonRoom photonRoom;
    public AutoSave autoSave;
    public ControlControl controls;
    void Start()
    {
        photonRoom = FindObjectOfType<PhotonRoom>();
        if (pauseMenu.activeSelf == true) { pauseMenu.SetActive(false); }
        gamePaused = false;
    }
    public void PauseGame() 
    {
        if (!gamePaused) 
        {
            controls.Hide();
            pauseMenu.SetActive(true);
            gamePaused = true;
            autoSave.Save();
        }
        else 
        {
            pauseMenu.SetActive(false);
            gamePaused = false;
            controls.Show();
        }
    }
    public void GoMainMenu() 
    {
        gamePaused = false;
        photonRoom.LeaveGame();
        SceneManager.LoadScene(1);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.JoinLobby();
    }
}
