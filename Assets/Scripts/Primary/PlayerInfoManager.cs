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
    private Topbar topbar;
    private GameServer gameServer;
    private int id;
    private string authKey;
    private PlayerInfo storedPlayerInfo;
    private GameObject player;

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
                StartCoroutine(FoodDepleteLoop());
                GetPlayer_AllInfo();
                player = FindObjectOfType<FirstPersonController>().gameObject;
            }

        }
        else 
        {
            Debug.Log("[Client] InfoManager : Networking Manager Null.");
        }
    }

    private void UpdateInventory()
    {
        Debug.Log("[Client] InfoManager : Updating Inventory.");
        inventoryGfx.Incoming(storedPlayerInfo);
    }
    private void UpdateTopBar()
    {
        Debug.Log("[Client] InfoManager : Updating Top Bar.");
        topbar.Incoming(storedPlayerInfo);
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


    public void SetPlayer_Health(int value)
    {
        gameServer.SetPlayerHealth(id, authKey, value);
    }
    public void SetPlayer_Food(int value) 
    {
        gameServer.SetPlayerFood(id, authKey, value);
    }
    public void SetPlayer_Water(int value) 
    {
        gameServer.SetPlayerWater(id, authKey, value);
    }

    public void MoveItemBySlots(int curSlot, int newSlot) 
    {
        gameServer.MovePlayerItemBySlot(id, authKey, curSlot, newSlot);
    }
    public void RemoveItemBySlot(int curSlot) 
    {
        gameServer.RemovePlayerItemBySlot(id, authKey, curSlot);
    }

    public void CraftItemById(int itemId, int amount) 
    {
        gameServer.CraftItemById(id, authKey, itemId, amount);
    }

    public void RequestToDie() 
    {
        Debug.Log(player.transform.localPosition);
        SetPlayer_Health(-100);
        gameServer.RequestToDie(id, authKey);
    }

    //-----------------------------------------------------------------//
    //                      Client Side Loops                          //
    //-----------------------------------------------------------------//


    private IEnumerator FoodDepleteLoop() 
    {
        yield return new WaitForSeconds(10f);
        
        if (storedPlayerInfo != null) 
        {
            int intensity = 1;
            int remove = 0;
            int healthDeplete = 0;
            if (storedPlayerInfo.health < 100)
            {
                remove++;
                intensity = 3;
            }

            if (storedPlayerInfo.food != 0 && (storedPlayerInfo.food + (-1 * intensity)) >= 0) 
            {
                SetPlayer_Food(-1 * intensity);
                remove++;
            }

            if(storedPlayerInfo.water != 0 && (storedPlayerInfo.water + (-1 * intensity)) >= 0) 
            {
                SetPlayer_Water(-1 * intensity);
                remove++;
            }

            if(remove == 3) 
            {
                SetPlayer_Health(1);
            }

            if(storedPlayerInfo.food == 0) 
            {
                healthDeplete++;
            }

            if (storedPlayerInfo.water == 0) 
            {
                healthDeplete++;
            }

            if(healthDeplete > 0) 
            {
                if(storedPlayerInfo.health > 0) 
                {
                    SetPlayer_Health(-1 * healthDeplete);
                }
            }
        }
        StartCoroutine(FoodDepleteLoop());
    }

}
