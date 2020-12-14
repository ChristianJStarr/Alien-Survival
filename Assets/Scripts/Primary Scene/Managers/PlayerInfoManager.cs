using MLAPI;
using MLAPI.Serialization.Pooled;
using System.IO;
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
    public PlayerInfo storedPlayerInfo;
    private Backpack playerBackpack;
    private string authKey;

    private int lastHealth = 100;

    private void Start()
    {
        if(NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient)
        {
            gameServer = GameServer.singleton;
            authKey = PlayerPrefs.GetString("authKey");
        }
        else 
        {
            DebugMsg.Notify("InfoManager : Networking Manager Null.", 1);
        }
    }



    //Primary Intake for PlayerInfo. Called from GameServer ClientRPC
    public void IntakeStream(Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            if (storedPlayerInfo == null || storedPlayerInfo.inventory == null)
            {
                storedPlayerInfo = new PlayerInfo()
                {
                    inventory = new Inventory()
                };
            }
            int depth = reader.ReadInt32Packed();
            if (depth == 1)//All
            {
                storedPlayerInfo.health = reader.ReadInt32Packed();
                storedPlayerInfo.food = reader.ReadInt32Packed();
                storedPlayerInfo.water = reader.ReadInt32Packed();
                int inventoryLength = reader.ReadInt32Packed();
                if (inventoryLength > 0)
                {
                    storedPlayerInfo.inventory.Clear();
                    for (int i = 0; i < inventoryLength; i++)
                    {
                        storedPlayerInfo.inventory.items.Add(new Item()
                        {
                            itemId = reader.ReadInt32Packed(),
                            itemStack = reader.ReadInt32Packed(),
                            currSlot = reader.ReadInt32Packed(),
                            durability = reader.ReadInt32Packed()
                        });
                    }
                }
                storedPlayerInfo.inventory.blueprints = reader.ReadIntArrayPacked();
                UpdateAll();
            }
            else if (depth == 2)//Health
            {
                storedPlayerInfo.health = reader.ReadInt32Packed();
                UpdateHealth();
            }
            else if (depth == 3)//Food
            {
                storedPlayerInfo.food = reader.ReadInt32Packed();
                UpdateFood();
            }
            else if (depth == 4)//Water
            {
                storedPlayerInfo.water = reader.ReadInt32Packed();
                UpdateWater();
            }
            else if (depth == 5)//Items
            {
                int inventoryLength = reader.ReadInt32Packed();
                if (inventoryLength > 0)
                {
                    storedPlayerInfo.inventory.Clear();
                    for (int i = 0; i < inventoryLength; i++)
                    {
                        storedPlayerInfo.inventory.items.Add(new Item()
                        {
                            itemId = reader.ReadInt32Packed(),
                            itemStack = reader.ReadInt32Packed(),
                            currSlot = reader.ReadInt32Packed(),
                            durability = reader.ReadInt32Packed()
                        });
                    }
                    UpdateInventory();
                }
            }
            else if (depth == 7) //Blueprints
            {
                storedPlayerInfo.inventory.blueprints = reader.ReadIntArrayPacked();
                UpdateInventory();
            }
            else if (depth == 8) //Health / Food / Water
            {
                storedPlayerInfo.health = reader.ReadInt32Packed();
                storedPlayerInfo.food = reader.ReadInt32Packed();
                storedPlayerInfo.water = reader.ReadInt32Packed();
                UpdateHealth();
            }
        }
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
    //    Update Specific Components that use Live PlayerInfo          //
    //-----------------------------------------------------------------//

    //Entire PlayerInfo could have Changed
    public void UpdateAll()
    {
        inventory.Incoming();
        topbar.Incoming(storedPlayerInfo.health, 0, storedPlayerInfo.water, storedPlayerInfo.food);
        BackpackEffect();
    }
    //Health has Changed
    public void UpdateHealth() 
    {
        topbar.Incoming(storedPlayerInfo.health, 0, storedPlayerInfo.water, storedPlayerInfo.food);
        BackpackEffect();
    }
    //Food has Changed
    public void UpdateFood() 
    {
        topbar.Incoming(storedPlayerInfo.health, 0, storedPlayerInfo.water, storedPlayerInfo.food);
    }
    //Water has Changed
    public void UpdateWater() 
    {
        topbar.Incoming(storedPlayerInfo.health, 0, storedPlayerInfo.water, storedPlayerInfo.food);
    }
    //Inventory has Changed
    public void UpdateInventory() 
    {
        inventory.Incoming();
    }

    //-----------------------------------------------------------------//
    //             Player Requests to Modify Own Values                //
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
    //Player Backpack Effect
    private void BackpackEffect() 
    {
        if (playerBackpack != null)
        {
            if (storedPlayerInfo.health > lastHealth)
            {
                playerBackpack.UpdateBackpackGlow(2);
            }
            else if (storedPlayerInfo.health < lastHealth)
            {
                playerBackpack.UpdateBackpackGlow(1);
            }
            lastHealth = storedPlayerInfo.health;
        }
    }
}
