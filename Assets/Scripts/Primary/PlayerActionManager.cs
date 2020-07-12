using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerActionManager : MonoBehaviour
{

    #region Singleton

    public static PlayerActionManager singleton;

    void Awake()
    {
        singleton = this;
    }

    #endregion

    private int id;
    private string authKey;
    private GameServer gameServer;


    //DeathDropUI
    public GameObject deathDropContainer;
    public GameObject deathDropGfx;
    public GameObject deathScreen;
    public Image dragButton;
    public TextMeshProUGUI dragButtonText;
    private ItemSlot[] deathDropItemSlots;
    private Item[] currentDeathDropItems;
    private bool deathDropUIActive = false;
    private string currentDeathDropUnique;
    private bool needClose = false;

    private SelectedItemHandler itemHandler;
    
    
    void Start()
    {
        id = PlayerPrefs.GetInt("userId");
        authKey = PlayerPrefs.GetString("authKey");
        gameServer = GameServer.singleton;
        deathDropItemSlots = deathDropContainer.GetComponentsInChildren<ItemSlot>(true);
        for (int i = 0; i < deathDropItemSlots.Length; i++)
        {
            deathDropItemSlots[i].slotNumber = i + 1;
        }
    }



    //-----------------------------------------------------------------//
    //                      Interact With                              //
    //-----------------------------------------------------------------//


    //Interact with Clickable
    public void InteractWithClickable(string uniqueId) 
    {
        if(gameServer != null) 
        {
            gameServer.InteractWithClickable(id, authKey, uniqueId);
        }
    }


    //Interact with Resource
    public void InteractWithResource(string uniqueId)
    {
        if (gameServer != null)
        {
            gameServer.InteractWithResource(id, authKey, uniqueId);
        }
    }


    //Interact with DeathDrop
    public void InteractWithDeathDrop(string uniqueId, Item[] drops) 
    {
        foreach (ItemSlot slot in deathDropItemSlots)
        {
            slot.ClearSlot();
        }
        currentDeathDropUnique = uniqueId;
        for (int i = 0; i < drops.Length; i++)
        {
            drops[i].currSlot = i + 1;
        }
        foreach (ItemSlot slot in deathDropItemSlots)
        {
            foreach(Item drop in drops) 
            {
                if(drop.currSlot == slot.slotNumber) 
                {
                    slot.AddItem(drop, gameServer.GetItemDataById(drop.itemID));
                    break;
                }
                
            }   
        }
        ToggleDeathDropUI(true);
        currentDeathDropItems = drops;
    }
        private void ToggleDeathDropUI(bool value)
        {
            deathDropGfx.SetActive(value);
            deathDropUIActive = value;
        }
        public void CloseDeathDropUI()
        {
            ToggleDeathDropUI(false);
        }
        public void TakeAllDeathDrop()
        {
            gameServer.InteractWithDeathDrop(id, authKey, currentDeathDropUnique, 100, returnValue =>
            {
                if (returnValue != null)
                {
                    currentDeathDropItems = returnValue;
                    if (returnValue.Length == 0)
                    {
                        currentDeathDropItems = null;
                        ToggleDeathDropUI(false);
                        return;
                    }
                    InteractWithDeathDrop(currentDeathDropUnique, returnValue);
                }
                else
                {
                    currentDeathDropItems = null;
                    ToggleDeathDropUI(false);
                }
            });

        }
        public void TakeSingleDeathDrop(int slotNumber)
        {
            gameServer.InteractWithDeathDrop(id, authKey, currentDeathDropUnique, slotNumber, returnValue =>
            {
                if (returnValue != null)
                {
                    currentDeathDropItems = returnValue;
                    if (returnValue.Length == 0)
                    {
                        currentDeathDropItems = null;
                        ToggleDeathDropUI(false);
                        return;
                    }
                    InteractWithDeathDrop(currentDeathDropUnique, returnValue);
                }
                else
                {
                    currentDeathDropItems = null;
                    ToggleDeathDropUI(false);
                }
            });
        }
        public void DragStarted()
        {
            needClose = false;
            dragButton.color = new Color32(183, 183, 183, 170);
            dragButtonText.text = "Drag Item Here";
        }
        public void DragEnded()
        {
            if (needClose)
            {
                CloseDeathDropUI();
                return;
            }
            dragButton.color = new Color32(183, 183, 183, 255);
            dragButtonText.text = "Inventory";
        }
        public void ShowDeathScreen()
        {
            deathScreen.SetActive(true);
        }
        public void HideDeathScreen()
        {
            deathScreen.SetActive(false);
        }


    //-----------------------------------------------------------------//
    //                      Selected Items                             //
    //-----------------------------------------------------------------//

    //Use Selected
    public void UseSelectedItem(int slot, Transform aim, SelectedItemHandler handler) 
    {
        if(itemHandler == null) 
        {
            itemHandler = handler;
        }
        gameServer.UseSelectedItem(id, authKey, slot, aim);
    }

    public void UseSelectedItemReturn(bool value) 
    {
        itemHandler.SelectedReturn(value);
    }


    //Add to Durability (Reload)
    public void ReloadToDurability(int slot) 
    {
        gameServer.ReloadToDurability(id, authKey, slot);
    }


    //-----------------------------------------------------------------//
    //                      Debug Commands                             //
    //-----------------------------------------------------------------//


    //Request to Teleport
    public void RequestToTeleport(ulong targetClient) 
    {
        gameServer.RequestToTeleport(id, authKey, targetClient);
    }
    //Request to Cheat Item
    public void RequestToCheatItem(int itemId) 
    {
        gameServer.RequestToCheatItem(id, authKey, itemId);
    }

 



    public void RequestDisconnect() 
    {
        gameServer.RequestToDisconnect(id, authKey);
    }
   
}
