using System;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{

    #region Singleton

    public static PlayerUIManager Singleton;

    void Awake()
    {
        Singleton = this;
    }

    #endregion

    private string authKey;
    private GameServer gameServer;


    public UI_Topbar topbar;
    public UI_Inventory inventory;
    public UI_DeathScreen deathScreen; //----Death Screen
    public UI_Joystick joystick; //----Joy Stick
    public UI_PauseControl pauseMenu;

    //Interfaces
    public ChatManager chat;
    public ControlsManager controls;
    public GameObject notifyTray;
    public UI_Reticle reticle;


    //Inventory
    public void Show_Inventory() 
    {
        HideAllInterfaces();
    }
    public void Hide_Inventory() 
    {
    
    }




    //Hide All, Used for Death/Sleep screen etc.
    public void HideAllInterfaces()
    {
        //chat.Hide();
        topbar.Hide();
        inventory.Hide();
        controls.Hide();
        notifyTray.SetActive(false);
        reticle.Hide();
    }

    //Show All
    public void ShowAllInterfaces()
    {
        //chat.Show();
        topbar.Show();
        inventory.Show();
        controls.Show();
        notifyTray.SetActive(true);
        reticle.Show();
    }


    void Start()
    {
        authKey = PlayerPrefs.GetString("authKey");
        gameServer = GameServer.singleton;
    }





    //-----------------------------------------------------------------//
    //                   Server Requests                               //
    //-----------------------------------------------------------------//

    //----Death Screen
    //Show
    public void ShowDeathScreen(double hours) 
    {
        HideAllInterfaces();
        deathScreen.EnableScreen(TimeSpan.FromHours(hours));
        joystick.ForceStopAutoSprint();
    }
    //Hide
    public void HideDeathScreen() 
    {
        ShowAllInterfaces();
        deathScreen.DisableScreen();
    }



    //-----------------------------------------------------------------//
    //                   Player Requests                               //
    //-----------------------------------------------------------------//


    //Request to Disconnect
    public void RequestDisconnect()
    {
        gameServer.RequestToDisconnect(authKey);
    }

}
