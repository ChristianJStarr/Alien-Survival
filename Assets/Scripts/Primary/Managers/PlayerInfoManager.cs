using MLAPI;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerInfoManager : MonoBehaviour
{

    #region Singleton

    public static PlayerInfoManager singleton;

    void Awake()
    {
        singleton = this;
    }

    #endregion
    public InventoryGfx inventoryGfx;
    public LoadAwake loadAwake;
    public Topbar topbar;
    private GameServer gameServer;
    private PlayerInfo storedPlayerInfo;
    private GameObject player;
    private Backpack playerBackpack;
    private ulong clientId;
    private string authKey;
    private bool firstRequest = true;

    private void Start()
    {
        if(NetworkingManager.Singleton != null) 
        {
            if (NetworkingManager.Singleton.IsClient)
            {
                storedPlayerInfo = new PlayerInfo();
                gameServer = GameServer.singleton;
                clientId = NetworkingManager.Singleton.LocalClientId;
                authKey = PlayerPrefs.GetString("authKey");
                StartCoroutine(MainPlayerLoop());
                player = FindObjectOfType<FirstPersonController>().gameObject;
            }
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
        if (inventoryGfx != null && storedPlayerInfo != null)
        {
            inventoryGfx.Incoming(storedPlayerInfo);
        }
    }

    //-------Update Top Bar
    private void UpdatedTopbar()
    {
        DebugMsg.Notify("InfoManager : Updating Top Bar.", 3);
        if (topbar != null && storedPlayerInfo != null)
        {
            topbar.Incoming(storedPlayerInfo);
        }
        
    }
    
    
    
    //-----------------------------------------------------------------//
    //                      Clickable - Storage                        //
    //-----------------------------------------------------------------//

    public void ShowStorage(string data)
    {
        inventoryGfx.InvButton(data);
    }

    public void UpdateExtraUIData(string data) 
    {
        inventoryGfx.UpdateExtraUIData(data);
    }
    //-------Backpack Initialize
    public void InitializeBackpackEffect(Backpack backpack) 
    {
        playerBackpack = backpack;
    }


    public void CloseInventory() 
    {
        inventoryGfx.CloseInventory();
    }


    //-----------------------------------------------------------------//
    //             Update PlayerInfo Called From Server                //
    //-----------------------------------------------------------------//
   
    public void UpdateAll(PlayerInfo info)
    {
        storedPlayerInfo = info;
        UpdatedInventory();
        UpdatedTopbar();
        if (firstRequest && loadAwake != null)
        {
            loadAwake.ReadyWake();
            firstRequest = false;
        }
    }
    public void UpdateHealth(int health) 
    {
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
        storedPlayerInfo.food = food;
        UpdatedTopbar();
    }
    public void UpdateWater(int water) 
    {
        storedPlayerInfo.water = water;
        UpdatedTopbar();
    }
    public void UpdateItems(Item[] items) 
    {
        storedPlayerInfo.items = items;
        UpdatedInventory();
    }
    public void UpdateArmor(Item[] armor) 
    {
        storedPlayerInfo.armor = armor;
        UpdatedInventory();
    }
    public void UpdateBlueprints(int[] blueprints) 
    {
        storedPlayerInfo.blueprints = blueprints;
        UpdatedInventory();
    }




    //-----------------------------------------------------------------//
    //             Player Request : Modify Own Values                  //
    //-----------------------------------------------------------------//


    //-------Set Player Location
    public void SetPlayer_Location(Vector3 location) 
    {
        gameServer.SetPlayerLocation(authKey, location);
    }

    //-------INVENTORY

    //Move Item By Slots
    public void MoveItemBySlots(int oldSlot, int newSlot) 
    {
        gameServer.MovePlayerItemBySlot(authKey, oldSlot, newSlot);
    }
    //Remove Item By Slot
    public void RemoveItemBySlot(int curSlot) 
    {
        gameServer.RemovePlayerItemBySlot(clientId, authKey, curSlot);
    }
    //Craft Item By Item
    public void CraftItemById(int itemId, int amount)
    {
        gameServer.CraftItemById(clientId, authKey, itemId, amount);
    }

 
    //-----------------------------------------------------------------//
    //                      Client Side Loops                          //
    //-----------------------------------------------------------------//

    //-------Main Player Loop
    private IEnumerator MainPlayerLoop() 
    {
        yield return new WaitForSeconds(2F);

        //Store Player Location
        StorePlayerLocation();
        
        //Restart Infinite Loop
        StartCoroutine(MainPlayerLoop());
    }

    //-------Store Location
    private void StorePlayerLocation()
    {
        if (player != null)
        {
            SetPlayer_Location(player.transform.position);
        }
        else
        {
            FirstPersonController fps = FindObjectOfType<FirstPersonController>();
            if (fps != null) 
            {
                player = fps.gameObject;
                SetPlayer_Location(player.transform.position);
            } 
        }
    }
    
}
