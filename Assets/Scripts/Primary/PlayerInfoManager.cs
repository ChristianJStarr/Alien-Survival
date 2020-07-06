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
    private int id;
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
                id = PlayerPrefs.GetInt("userId");
                authKey = PlayerPrefs.GetString("authKey");
                StartCoroutine(MainPlayerLoop());
                GetPlayer_AllInfo();
                player = FindObjectOfType<FirstPersonController>().gameObject;
            }

        }
        else 
        {
            Debug.Log("[Client] InfoManager : Networking Manager Null.");
        }
    }


    //-------Update Inventory
    private void UpdateInventory()
    {
        Debug.Log("[Client] InfoManager : Updating Inventory.");
        if (inventoryGfx != null && storedPlayerInfo != null)
        {
            inventoryGfx.Incoming(storedPlayerInfo);
        }
    }

    //-------Update Top Bar
    private void UpdateTopBar()
    {
        Debug.Log("[Client] InfoManager : Updating Top Bar.");
        if (topbar != null && storedPlayerInfo != null)
        {
            topbar.Incoming(storedPlayerInfo);
        }
        
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
        gameServer.GetPlayerInventoryItems(id, returnValue1 =>
        {
            storedPlayerInfo.items = returnValue1;
            gameServer.GetPlayerInventoryArmor(id, returnValue2 =>
            {
                storedPlayerInfo.armor = returnValue2;
                gameServer.GetPlayerInventoryBlueprints(id, returnValue3 =>
                {
                    storedPlayerInfo.blueprints = returnValue3;
                    gameServer.GetPlayerHealth(id, returnValue4 =>
                    {
                        storedPlayerInfo.health = returnValue4;
                        gameServer.GetPlayerFood(id, returnValue5 =>
                        {
                            storedPlayerInfo.food = returnValue5;
                            gameServer.GetPlayerWater(id, returnValue6 =>
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
        gameServer.GetPlayerInventoryItems(id, returnValue => 
        {
            storedPlayerInfo.items = returnValue;
            UpdateInventory();
        });
    }
    
    //-------Get Player Inventory Armor
    public void GetPlayer_InventoryArmor()
        {
        gameServer.GetPlayerInventoryArmor(id, returnValue =>
            {
                storedPlayerInfo.armor = returnValue;
                UpdateInventory();
            });
        }
    
    //-------Get Player Inventory Blueprints
    public void GetPlayer_InventoryBlueprints()
        {
        gameServer.GetPlayerInventoryBlueprints(id, returnValue =>
            {
                storedPlayerInfo.blueprints = returnValue;
                UpdateInventory();
            });
        }
    
    //-------Get Player Health
    public void GetPlayer_Health()
        {
        gameServer.GetPlayerHealth(id, returnValue =>
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
        gameServer.GetPlayerFood(id, returnValue =>
            {
                storedPlayerInfo.food = returnValue;
                UpdateTopBar();
            });
        }
    
    //-------Get Player Inventory Items
    public void GetPlayer_Water()
    {
        gameServer.GetPlayerWater(id, returnValue =>
            {
            storedPlayerInfo.water = returnValue;
            UpdateTopBar();
        });
    }

    //-----------------------------------------------------------------//
    //             Player Request : Modify Own Values                  //
    //-----------------------------------------------------------------//

    //-------Set Player Health
    public void SetPlayer_Health(int value)
    {
        gameServer.SetPlayerHealth(id, authKey, value);
    }

    //-------Set Player Food
    public void SetPlayer_Food(int value) 
    {
        gameServer.SetPlayerFood(id, authKey, value);
    }

    //-------Set Player Water
    public void SetPlayer_Water(int value) 
    {
        gameServer.SetPlayerWater(id, authKey, value);
    }

    //-------Set Player Location
    public void SetPlayer_Location(Vector3 location) 
    {
        gameServer.SetPlayerLocation(id, authKey, location);
    }

    //-------Move Item by Slot
    public void MoveItemBySlots(int curSlot, int newSlot) 
    {
        gameServer.MovePlayerItemBySlot(id, authKey, curSlot, newSlot);
    }

    //-------Remove Item by Slot
    public void RemoveItemBySlot(int curSlot) 
    {
        gameServer.RemovePlayerItemBySlot(id, authKey, curSlot);
    }

    //-------Craft Item by Id
    public void CraftItemById(int itemId, int amount) 
    {
        gameServer.CraftItemById(id, authKey, itemId, amount);
    }

    //-------Request to Die
    public void RequestToDie() 
    {
        Debug.Log(player.transform.localPosition);
        SetPlayer_Health(-100);
        gameServer.RequestToDie(id, authKey);
    }


    public void GetIfEnoughItems(int itemId, int amount, System.Action<bool> callback) 
    {
        gameServer.GetIfEnoughItems(id, itemId, amount, returnValue => { callback(returnValue); });
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
        DepleteFoodWater();
        StartCoroutine(MainPlayerLoop());
    }

    //Deplete Food & Water
    private void DepleteFoodWater() 
    {
        if(depleteRate == depleteCount) 
        {
            depleteCount = 1;
            if (storedPlayerInfo.food > 0)
            {
                SetPlayer_Food(-1 * depleteIntensity);
            }
            if (storedPlayerInfo.water > 0)
            {
                SetPlayer_Water(-1 * depleteIntensity);
            }
        }
        else 
        {
            depleteCount++;
        }
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
