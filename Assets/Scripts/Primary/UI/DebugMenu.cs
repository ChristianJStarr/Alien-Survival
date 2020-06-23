using MLAPI;
using MLAPI.Connection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    public Transform playerListContainer;
    public Transform itemListContainer;
    public GameObject playerListPefab;
    public GameObject debugMenu;

    private List<DebugMenuSlide> slides;
    private List<DebugMenuSlide> items;

    private GameServer gameServer;


    private ulong selectedPlayer = 999999;
    private int selectedItem = 0;

    private void Start()
    {
        if (NetworkingManager.Singleton.IsClient) 
        {

            gameServer = GameServer.singleton;
            slides = new List<DebugMenuSlide>();
            items = new List<DebugMenuSlide>();
            UpdatePlayers();
            UpdateItems();
        }
    }

    public void ToggleMenu() 
    {
        debugMenu.SetActive(!debugMenu.activeSelf);
        UpdatePlayers();
        UpdateItems();
    }


    public void UpdatePlayers() 
    {
        foreach (DebugMenuSlide item in slides)
        {
            Destroy(item.gameObject);
        }

        ulong[] clients = NetworkingManager.Singleton.ConnectedClients.Keys.ToArray();

        foreach (ulong client in clients)
        {
            if(gameServer != null) 
            {
                gameServer.GetNameByClientId(client, returnValue => 
                {
                    if(returnValue.Length > 0) 
                    {
                        DebugMenuSlide slide = Instantiate(playerListPefab, playerListContainer).GetComponent<DebugMenuSlide>();
                        slide.UpdateValues(returnValue, (int)client, this);
                        slides.Add(slide);
                    }
                });
            }
        }
    }

    private void UpdateItems() 
    {
        ItemData[] itemDatas = gameServer.GetAllItemData();
        if(itemDatas != null) 
        {
            foreach (ItemData item in itemDatas)
            {
                DebugMenuSlide slide = Instantiate(playerListPefab, itemListContainer).GetComponent<DebugMenuSlide>();
                slide.UpdateValues(item.name, item.itemID, this);
                slide.isItem = true;
                items.Add(slide);
            }
        }
    }

    public void SelectPlayer(int player) 
    {
        foreach (DebugMenuSlide slide in slides)
        {
            slide.Selected(slide.id == player);
        }
        selectedPlayer = Convert.ToUInt64(player);
    }
    public void SelectItem(int item) 
    {
        foreach(DebugMenuSlide slide in items) 
        {
            slide.Selected(slide.id == item);
        }
        selectedItem = item;
    }

    public void SpawnItem() 
    {
        if(selectedItem != 0) 
        {
            PlayerActionManager.singleton.RequestToCheatItem(selectedItem);
        }
    }
    public void TeleportPlayer() 
    {
        if(selectedPlayer != 999999) 
        {
           PlayerActionManager.singleton.RequestToTeleport(selectedPlayer);
        }
    }
}
