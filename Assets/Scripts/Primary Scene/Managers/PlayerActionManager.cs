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
    public GameObject DeathScreen;
    
    void Start()
    {
        authKey = PlayerPrefs.GetString("authKey");
        gameServer = GameServer.singleton;
    }





    //-----------------------------------------------------------------//
    //                   Player Requests                               //
    //-----------------------------------------------------------------//

    public void RequestDisconnect() 
    {
        gameServer.RequestToDisconnect(authKey);
    }



    public void ShowDeathScreen() 
    {
        InterfaceHider.Singleton.HideAllInterfaces();
        DeathScreen.SetActive(true);
    }

    public void HideDeathScreen() 
    {
        InterfaceHider.Singleton.ShowAllInterfaces();
        DeathScreen.SetActive(false);
    }
}
