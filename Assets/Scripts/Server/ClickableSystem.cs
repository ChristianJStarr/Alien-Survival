using UnityEngine;

public class ClickableSystem : MonoBehaviour
{

    private GameServer gameServer;
    private void Start()
    {
        gameServer = GameServer.singleton;
    }

    public void InteractWithClickable(PlayerInfo player, string uniqueId) 
    {
        Clickable clickable = FindClickableById(uniqueId);
        
        //Pickup Object
        if(clickable.type == 1) 
        {
            if(gameServer != null) 
            {
                gameServer.ServerAddNewItemToInventory(player.clientId, clickable.itemId, clickable.maxAmount);
            }
        }
    }

    private Clickable FindClickableById(string uniqueId) 
    {
        Clickable clickable = null;
        Clickable[] clickables = FindObjectsOfType<Clickable>();
        for (int i = 0; i < clickables.Length; i++)
        {
            if (clickables[i].unique == uniqueId)
            {
                clickable = clickables[i];
                break;
            }
        }
        return clickable;
    }
}
