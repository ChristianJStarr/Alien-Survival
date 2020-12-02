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
    public Topbar topbar;



    public UI_Inventory inventory;
    public UI_DeathScreen deathScreen; //----Death Screen
    public UI_Joystick joystick; //----Joy Stick


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
        InterfaceHider.Singleton.HideAllInterfaces();
        deathScreen.EnableScreen(TimeSpan.FromHours(hours));
        joystick.ForceStopAutoSprint();
    }
    //Hide
    public void HideDeathScreen() 
    {
        InterfaceHider.Singleton.ShowAllInterfaces();
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
