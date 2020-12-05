using MLAPI;
using UnityEngine;

public class PlayerInfoManager : MonoBehaviour
{

    #region Singleton

    public static PlayerInfoManager singleton;

    void Awake()
    {
        singleton = this;
    }

    #endregion
    public UI_Inventory inventory;
    public LoadAwake loadAwake;
    public UI_Topbar topbar;
    private GameServer gameServer;
    private PlayerInfo storedPlayerInfo;
    private Backpack playerBackpack;
    private string authKey;
    private bool firstRequest = true;

    private void Start()
    {
        if(NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient)
        {
            storedPlayerInfo = new PlayerInfo();
            gameServer = GameServer.singleton;
            authKey = PlayerPrefs.GetString("authKey");
        }
        else 
        {
            DebugMsg.Notify("InfoManager : Networking Manager Null.", 1);
        }
    }

    //-------Update Inventory
    private void UpdatedInventory()
    {
        DebugMsg.Notify("InfoManager : Updating Inventory.", 3);
        inventory.Incoming(storedPlayerInfo.items, storedPlayerInfo.armor, storedPlayerInfo.blueprints);
    }

    //-------Update Top Bar
    private void UpdatedTopbar()
    {
        DebugMsg.Notify("InfoManager : Updating TopBar.", 3);
        topbar.Incoming(storedPlayerInfo.health, 0, storedPlayerInfo.water, storedPlayerInfo.food);
    }
    
    
    
    //-----------------------------------------------------------------//
    //                      Clickable - Storage                        //
    //-----------------------------------------------------------------//


    //-------Backpack Initialize
    public void InitializeBackpackEffect(Backpack backpack) 
    {
        playerBackpack = backpack;
    }

    //-------Inventory
    //Show
    public void ShowInventoryScreen(int uiType, UIData data)
    {
        inventory.OpenInventory(uiType, data);
    }
    //Hide
    public void HideInventoryScreen()
    {
        inventory.CloseInventory();
    }




    //-----------------------------------------------------------------//
    //             Update PlayerInfo Called From Server                //
    //-----------------------------------------------------------------//

    public void UpdateAll(PlayerInfo info)
    {
        if (storedPlayerInfo == null) return;
        storedPlayerInfo = info;
        UpdatedInventory();
        UpdatedTopbar();
        if (firstRequest && loadAwake != null)
        {
            loadAwake.playerHasInfo = true;
            firstRequest = false;
        }
    }
    public void UpdateHealth(int health) 
    {
        if (storedPlayerInfo == null) return;
        if (storedPlayerInfo.health > health)
        {
            if (playerBackpack != null)
            {
                playerBackpack.UpdateBackpackGlow(1);
            }
        }
        else if (storedPlayerInfo.health < health)
        {
            if (playerBackpack != null)
            {
                playerBackpack.UpdateBackpackGlow(2);
            }
        }
        storedPlayerInfo.health = health;
        UpdatedTopbar();
    }
    public void UpdateFood(int food) 
    {
        if (storedPlayerInfo == null) return;
        storedPlayerInfo.food = food;
        UpdatedTopbar();
    }
    public void UpdateWater(int water) 
    {
        if (storedPlayerInfo == null) return;
        storedPlayerInfo.water = water;
        UpdatedTopbar();
    }
    public void UpdateItems(Item[] items) 
    {
        if (storedPlayerInfo == null) return;
        storedPlayerInfo.items = items;
        UpdatedInventory();
    }
    public void UpdateArmor(Item[] armor) 
    {
        if (storedPlayerInfo == null) return;
        storedPlayerInfo.armor = armor;
        UpdatedInventory();
    }
    public void UpdateBlueprints(int[] blueprints) 
    {
        if (storedPlayerInfo == null) return;
        storedPlayerInfo.blueprints = blueprints;
        UpdatedInventory();
    }




    //-----------------------------------------------------------------//
    //             Player Request : Modify Own Values                  //
    //-----------------------------------------------------------------//


    //Move Item By Slots
    public void MoveItemBySlots(int oldSlot, int newSlot) 
    {
        gameServer.MovePlayerItemBySlot(authKey, oldSlot, newSlot);
    }
    //Remove Item By Slot
    public void RemoveItemBySlot(int curSlot) 
    {
        gameServer.ClientRemoveItemBySlot(authKey, curSlot);
    }
    //Craft Item By Item
    public void CraftItemById(int itemId, int amount)
    {
        gameServer.CraftItemById(authKey, itemId, amount);
    }

    //Split Item by Slot & Amount
    public void SplitItemBySlot(int slot, int amount) 
    {
        gameServer.ClientSplitItemBySlot(authKey, slot, amount);
    }

    
}
