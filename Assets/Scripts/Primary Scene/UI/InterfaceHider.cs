using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceHider : MonoBehaviour
{

    public static InterfaceHider Singleton;

    private void Awake() { Singleton = this; }






    //Interfaces

    public ChatManager chat;

    public UI_Topbar topBar;

    public UI_Inventory inventory;

    public ControlControl controls;

    public GameObject notifyTray;

    public UI_Reticle reticle;


    //Hide All, Used for Death/Sleep screen etc.
    public void HideAllInterfaces() 
    {
        //chat.Hide();
        topBar.Hide();
        inventory.Hide();
        controls.Hide();
        notifyTray.SetActive(false);
        reticle.Hide();
    }


    public void ShowAllInterfaces() 
    {
        //chat.Show();
        topBar.Show();
        inventory.Show();
        controls.Show();
        notifyTray.SetActive(true);
        reticle.Show();
    }








}
