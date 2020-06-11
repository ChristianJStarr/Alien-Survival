using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableSystem : MonoBehaviour
{

    private GameServer gameServer;

    private void Start()
    {
        gameServer = GetComponent<GameServer>();
    }

    public void InteractWithClickable(PlayerInfo player, string uniqueId) 
    {
        Clickable clickable = FindClickableById(uniqueId);
        
        //Pickup Object
        if(clickable.type == 1) 
        {
            gameServer.ServerAddNewItemToInventory(player.clientId, clickable.itemId, clickable.maxAmount);
        }
    }

    private Clickable FindClickableById(string uniqueId) 
    {
        Clickable clickable = null;
        Clickable[] clickables = FindObjectsOfType<Clickable>();
        foreach (Clickable click in clickables)
        {
            if(click.unique == uniqueId) 
            {
                clickable = click;
                break;
            }
        }
        return clickable;
    }
}
