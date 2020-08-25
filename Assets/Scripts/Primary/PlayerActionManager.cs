using System;
using System.Collections;
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

    private ulong clientId;
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
            gameServer.InteractWithClickable(clientId, authKey, uniqueId);
        }
    }


    //Interact with Resource
    public void InteractWithResource(string uniqueId)
    {
        if (gameServer != null)
        {
            gameServer.InteractWithResource(clientId, authKey, uniqueId);
        }
    }


    //Interact with DeathDrop
    public void InteractWithDeathDrop(string uniqueId, Item[] drops)
    {
        for (int i = 0; i < drops.Length; i++)
        {
            drops[i].currSlot = i + 1;
        }
        InvUI.ClearSlots(deathDropItemSlots);
        InvUI.SortSlots(drops, deathDropItemSlots);
        currentDeathDropUnique = uniqueId;
        currentDeathDropItems = drops;
        ToggleDeathDropUI(true);
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
        gameServer.InteractWithDeathDrop(clientId, authKey, currentDeathDropUnique, 100, returnValue =>
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
        gameServer.InteractWithDeathDrop(clientId, authKey, currentDeathDropUnique, slotNumber, returnValue =>
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
    public void RequestToRespawn() 
    {
        gameServer.RequestToRespawn(authKey);
    }

   


    //-----------------------------------------------------------------//
    //                      Selected Items                             //
    //-----------------------------------------------------------------//

    //Use Selected
    public void UseSelectedItem(int slot, Transform aim) 
    {
        if (itemHandler == null)
        {
            itemHandler = FindObjectOfType<SelectedItemHandler>();
        }
        gameServer.UseSelectedItem(authKey, slot, aim);
    }

    public void UseSelectedItemReturn(bool value) 
    {
        if(itemHandler == null) 
        {
            itemHandler = FindObjectOfType<SelectedItemHandler>();
        }
        if(itemHandler != null)
        {
            itemHandler.SelectedReturn(value);
        }
    }


    //Add to Durability (Reload)
    public void ReloadToDurability(int slot) 
    {
        gameServer.ReloadToDurability(authKey, slot);
    }


    //Place placeable object
    public void PlacePlaceable(Transform location, int itemId, int itemSlot) 
    {
        gameServer.Client_PlacePlaceableObject(authKey, itemId, itemSlot, location);
    }






    

    //-----------------------------------------------------------------//
    //                      Debug Commands                             //
    //-----------------------------------------------------------------//





    public void RequestDisconnect() 
    {
        gameServer.RequestToDisconnect(authKey);
    }
   
}
