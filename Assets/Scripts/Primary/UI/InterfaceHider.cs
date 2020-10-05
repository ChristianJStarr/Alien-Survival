using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceHider : MonoBehaviour
{

    public static InterfaceHider Singleton;

    private void Awake() { Singleton = this; }






    //Interfaces

    public ChatManager chat;

    public Topbar topBar;

    public InventoryGfx inventory;

    public ControlControl controls;

    public GameObject notifyTray;


    //Hide All, Used for Death/Sleep screen etc.
    public void HideAllInterfaces() 
    {
        chat.Hide();
        topBar.Hide();
        inventory.Hide();
        controls.Hide();
        notifyTray.SetActive(false);
    }


    public void ShowAllInterfaces() 
    {
        chat.Show();
        topBar.Show();
        inventory.Show();
        controls.Show();
        notifyTray.SetActive(true);
    }








}
