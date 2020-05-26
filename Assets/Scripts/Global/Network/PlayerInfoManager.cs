using MLAPI;
using System.Collections;
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

    private Inventory inventory;
    private Topbar topbar;
    private GameServer gameServer;
    private int id;
    private string authKey;
    private PlayerInfo storedPlayerInfo;

    private void Start()
    {
        if (NetworkingManager.Singleton.IsClient) 
        {
            storedPlayerInfo = new PlayerInfo();
            topbar = FindObjectOfType<Topbar>();
            inventory = Inventory.instance;
            gameServer = FindObjectOfType<GameServer>();
            id = PlayerPrefs.GetInt("id");
            authKey = PlayerPrefs.GetString("authKey");
            StartCoroutine(FoodDepleteLoop());
            GetPlayer_AllInfo();
        } 
    }

    private void UpdateInventory()
    {
        inventory.Incoming(storedPlayerInfo);
    }
    private void UpdateTopBar()
    {
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
                                Debug.Log("get all player info complete");
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

    //-------Get Player Inventory Items
    public void SetPlayer_InventoryItems(string value)
        {
            gameServer.SetPlayerInventoryItems(id, authKey, value);
        }
    //-------Get Player Inventory Armor
    public void SetPlayer_InventoryArmor(string value)
        {
            gameServer.SetPlayerInventoryArmor(id, authKey, value);
        }
    //-------Get Player Inventory Blueprints
    public void SetPlayer_InventoryBlueprints(string value)
        {
            gameServer.SetPlayerInventoryBlueprints(id, authKey, value);
        }

    public void SetPlayer_Food(int value) 
    {
        gameServer.SetPlayerFood(id, authKey, value);
    }
    public void SetPlayer_Water(int value) 
    {
        gameServer.SetPlayerWater(id, authKey, value);
    }

    private IEnumerator FoodDepleteLoop() 
    {
        Debug.Log("Deplete food");
        yield return new WaitForSeconds(10f);
        if (storedPlayerInfo != null) 
        {
            SetPlayer_Food(-1);
            SetPlayer_Water(-1);
        }
        StartCoroutine(FoodDepleteLoop());
    }

}
