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
    private Topbar topbar;
    private GameServer gameServer;
    private PlayerInfo storedPlayerInfo;
    private GameObject player;
    private Backpack playerBackpack;
    private int userId;
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
                topbar = FindObjectOfType<Topbar>();
                gameServer = FindObjectOfType<GameServer>();
                clientId = NetworkingManager.Singleton.LocalClientId;
                authKey = PlayerPrefs.GetString("authKey");
                StartCoroutine(MainPlayerLoop());
                GetPlayer_AllInfo();
                player = FindObjectOfType<FirstPersonController>().gameObject;
            }

        }
        else 
        {
            DebugMsg.Notify("InfoManager : Networking Manager Null.", 1);
        }
    }


    //-------Update Inventory
    private void UpdateInventory()
    {
        DebugMsg.Notify("InfoManager : Updating Inventory.", 1);
        if (inventoryGfx != null && storedPlayerInfo != null)
        {
            inventoryGfx.Incoming(storedPlayerInfo);
        }
    }

    //-------Update Top Bar
    private void UpdateTopBar()
    {
        DebugMsg.Notify("InfoManager : Updating Top Bar.", 1);
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


    //-----------------------------------------------------------------//
    //             Player Request : Request Own Values                 //
    //-----------------------------------------------------------------//


    //-------Get All The Player Info
    public void GetPlayer_AllInfo() 
    {
        gameServer.GetPlayerInventoryItems(clientId, returnValue1 =>
        {
            storedPlayerInfo.items = returnValue1;
            gameServer.GetPlayerInventoryArmor(clientId, returnValue2 =>
            {
                storedPlayerInfo.armor = returnValue2;
                gameServer.GetPlayerInventoryBlueprints(clientId, returnValue3 =>
                {
                    storedPlayerInfo.blueprints = returnValue3;
                    gameServer.GetPlayerHealth(clientId, returnValue4 =>
                    {
                        storedPlayerInfo.health = returnValue4;
                        gameServer.GetPlayerFood(clientId, returnValue5 =>
                        {
                            storedPlayerInfo.food = returnValue5;
                            gameServer.GetPlayerWater(clientId, returnValue6 =>
                            {
                                storedPlayerInfo.water = returnValue6;
                                UpdateTopBar();
                                UpdateInventory();
                                if (firstRequest && loadAwake != null) 
                                {
                                    loadAwake.ReadyWake();
                                    firstRequest = false;
                                }
                            });
                        });
                    });
                });
            });
        });
    }
    
    //-------Get Player Inventory Items
    public void GetPlayer_InventoryItems() 
    {
        gameServer.GetPlayerInventoryItems(clientId, returnValue => 
        {
            storedPlayerInfo.items = returnValue;
            UpdateInventory();
        });
    }
    
    //-------Get Player Inventory Armor
    public void GetPlayer_InventoryArmor()
        {
        gameServer.GetPlayerInventoryArmor(clientId, returnValue =>
            {
                storedPlayerInfo.armor = returnValue;
                UpdateInventory();
            });
        }
    
    //-------Get Player Inventory Blueprints
    public void GetPlayer_InventoryBlueprints()
        {
        gameServer.GetPlayerInventoryBlueprints(clientId, returnValue =>
            {
                storedPlayerInfo.blueprints = returnValue;
                UpdateInventory();
            });
        }
    
    //-------Get Player Health
    public void GetPlayer_Health()
        {
        gameServer.GetPlayerHealth(clientId, returnValue =>
            {
                if(storedPlayerInfo.health > returnValue) 
                {
                    if (playerBackpack != null)
                    {
                        playerBackpack.UpdateBackpackGlow(1);
                    }
                }
                else if(storedPlayerInfo.health < returnValue) 
                {
                    if (playerBackpack != null)
                    {
                        playerBackpack.UpdateBackpackGlow(2);
                    }
                }
                storedPlayerInfo.health = returnValue;
                UpdateTopBar();
            });
        }
    
    //-------Get Player Inventory Items
    public void GetPlayer_Food()
        {
        gameServer.GetPlayerFood(clientId, returnValue =>
            {
                storedPlayerInfo.food = returnValue;
                UpdateTopBar();
            });
        }
    
    //-------Get Player Inventory Items
    public void GetPlayer_Water()
    {
        gameServer.GetPlayerWater(clientId, returnValue =>
            {
            storedPlayerInfo.water = returnValue;
            UpdateTopBar();
        });
    }


    //-----------------------------------------------------------------//
    //             Player Request : Modify Own Values                  //
    //-----------------------------------------------------------------//


    //-------Set Player Location
    public void SetPlayer_Location(Vector3 location) 
    {
        gameServer.SetPlayerLocation(authKey, location);
    }

    //-------Move Item by Slot
    public void MoveItemBySlots(int curSlot, int newSlot) 
    {
        gameServer.MovePlayerItemBySlot(clientId, authKey, curSlot, newSlot);
    }

    //-------Remove Item by Slot
    public void RemoveItemBySlot(int curSlot) 
    {
        gameServer.RemovePlayerItemBySlot(clientId, authKey, curSlot);
    }

    //-------Craft Item by Id
    public void CraftItemById(int itemId, int amount)
    {
        gameServer.CraftItemById(clientId, authKey, itemId, amount);
    }

 
    //-----------------------------------------------------------------//
    //                      Client Side Loops                          //
    //-----------------------------------------------------------------//


    //Food and Water Deplete Speed
    private int depleteRate = 1;
    private int depleteCount = 1;
    private int depleteIntensity = 1;

    //-------Main Player Loop
    private IEnumerator MainPlayerLoop() 
    {
        yield return new WaitForSeconds(2F);

        StorePlayerLocation();
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
            } 
        }
    }
    
}
