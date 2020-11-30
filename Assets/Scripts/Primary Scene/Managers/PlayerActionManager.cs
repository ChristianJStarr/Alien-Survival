using System;
using UnityEngine;

public class PlayerActionManager : MonoBehaviour
{

    #region Singleton

    public static PlayerActionManager singleton;

    void Awake()
    {
        singleton = this;
    }

    #endregion

    private string authKey;
    private GameServer gameServer;
    private SelectedItemHandler itemHandler;
    public Topbar topbar;



    //----Death Screen
    public UI_DeathScreen deathScreen;
    
    void Start()
    {
        authKey = PlayerPrefs.GetString("authKey");
        gameServer = GameServer.singleton;
    }





    //-----------------------------------------------------------------//
    //                   Player Requests                               //
    //-----------------------------------------------------------------//

    //Request to Disconnect
    public void RequestDisconnect() 
    {
        gameServer.RequestToDisconnect(authKey);
    }


    //----Death Screen
    //Show
    public void ShowDeathScreen(double hours) 
    {
        InterfaceHider.Singleton.HideAllInterfaces();
        deathScreen.EnableScreen(TimeSpan.FromHours(hours));
    }
    //Hide
    public void HideDeathScreen() 
    {
        InterfaceHider.Singleton.ShowAllInterfaces();
        deathScreen.DisableScreen();
    }
}
