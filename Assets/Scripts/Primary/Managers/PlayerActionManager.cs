using UnityEditor.UIElements;
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
    //                   Interact With Item                            //
    //-----------------------------------------------------------------//

    //Interact with Something
    public void PlayerInteract(Ray ray, int selectedSlot) 
    {
        gameServer.PlayerInteract(authKey, ray, selectedSlot);
    }

    //Callback if successful
    public void PlayerInteractCallback(bool success) 
    {
        if (itemHandler == null)
        {
            itemHandler = FindObjectOfType<SelectedItemHandler>();
        }
        if (itemHandler != null)
        {
            itemHandler.SelectedReturn(success);
        }
    }

    //Add to Durability (Reload)
    public void ReloadToDurability(int slot)
    {
       // gameServer.ReloadToDurability(authKey, slot);
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
        ControlControl.Singleton.Hide();
        topbar.gameObject.SetActive(false);
        DeathScreen.SetActive(true);
    }

    public void HideDeathScreen() 
    {
        ControlControl.Singleton.Show();
        topbar.gameObject.SetActive(true);
        DeathScreen.SetActive(false);
    }
}
