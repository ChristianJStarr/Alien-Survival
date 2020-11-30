using MLAPI;
using MLAPI.LagCompensation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractSystem : MonoBehaviour
{
    private bool systemEnabled = false;
    public bool StartSystem()
    {
        systemEnabled = true;
        return systemEnabled;
    }
    public bool StopSystem()
    {
        systemEnabled = false;
        return !systemEnabled;
    }



    public PlayerInfoSystem playerInfoSystem;
    public WorldAISystem worldAISystem;

    // USE TYPES
    //  1)Shoot
    //  2)Melee
    //  3)Tool
    //  4)Consume
    //  5)Build


    //Interact
    public void Interact(ulong clientId, float networkTime, PlayerControlObject controlObject)
    {
        if (controlObject.selectedSlot != 0)
        {
            Item item = playerInfoSystem.Inventory_GetItemFromSlot(clientId, controlObject.selectedSlot);
            if (item != null)
            {
                //Item in hand
                ItemData data = ItemDataManager.Singleton.GetItemData(item.itemID);
                if (data != null && data.isUsable) //Item can be used 
                {
                    if (data.useType == 1) //Shoot
                    {
                        Interact_Shoot(clientId, networkTime, item, data, controlObject);
                    }
                    else if (data.useType == 2) //Melee
                    {
                        Interact_Melee(clientId, networkTime, item, data, controlObject);
                    }
                    else if (data.useType == 3) //Tool
                    {
                        Interact_Tool(clientId, networkTime, item, data, controlObject);
                    }
                    else if (data.useType == 4) //Consume
                    {
                        Interact_Consume(clientId, networkTime, item, data, controlObject);
                    }
                    else if (data.useType == 5) //Build
                    {
                        Interact_Build(clientId, networkTime, item, data, controlObject);
                    }
                    else
                    {
                        Interact_Clickable(clientId, networkTime, controlObject);
                    }
                }
                else
                {
                    Interact_Clickable(clientId, networkTime, controlObject);
                }
            }
            else
            {
                Interact_Clickable(clientId, networkTime, controlObject);
            }
        }
        else
        {
            Interact_Clickable(clientId, networkTime, controlObject);
        }
    }

    //Shoot
    private void Interact_Shoot(ulong clientId, float networkTime, Item item, ItemData data, PlayerControlObject controlObject)
    {
        Interact_LagCompensateRaycast(networkTime, () =>
        {
            RaycastHit hit;
            int distance = Mathf.Clamp(data.useRange, 10, 1000); //Set minimum distance, incase of looking at interactable
            Physics.Raycast(controlObject.cameraPoint.position, controlObject.cameraPoint.forward, out hit, distance, 0);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Clickable") && hit.distance < 11)
                {
                    ClickableObject clickable = hit.collider.GetComponent<ClickableObject>();
                    if (clickable != null)
                    {
                        //Interact with Clickable
                    }
                }
                else if (item.durability > 0 && playerInfoSystem.Inventory_ChangeItemDurability(clientId, -1, data.maxDurability, item.currSlot))
                {
                    //Damage Health of Object
                    if (hit.distance <= data.useRange)
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            PlayerControlObject hitPlayer = hit.collider.GetComponent<PlayerControlObject>();
                            if (hitPlayer != null)
                            {
                                //Damage Player
                                playerInfoSystem.SetPlayerHealth(hitPlayer.OwnerClientId, data.useAmount, true);
                            }
                        }
                        else if (hit.collider.CompareTag("AI"))
                        {
                            AIControlObject hitAI = hit.collider.GetComponent<AIControlObject>();
                            if (hitAI != null)
                            {
                                worldAISystem.DamageAI(hitAI.NetworkId, data.useAmount);
                            }
                        }
                    }
                }
                else
                {
                    Interact_Callback(clientId, 2);
                }
            }
        });
    }

    //Melee
    private void Interact_Melee(ulong clientId, float networkTime, Item item, ItemData data, PlayerControlObject controlObject)
    {

    }

    //Tool
    private void Interact_Tool(ulong clientId, float networkTime, Item item, ItemData data, PlayerControlObject controlObject)
    {

    }

    //Consume
    private void Interact_Consume(ulong clientId, float networkTime, Item item, ItemData data, PlayerControlObject controlObject)
    {

    }

    //Build
    private void Interact_Build(ulong clientId, float networkTime, Item item, ItemData data, PlayerControlObject controlObject)
    {

    }

    //Clickable
    private void Interact_Clickable(ulong clientId, float networkTime, PlayerControlObject controlObject)
    {
        Interact_LagCompensateRaycast(networkTime, () =>
        {
            RaycastHit hit;
            Physics.Raycast(controlObject.cameraPoint.position, controlObject.cameraPoint.forward, out hit, 10, 0);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Clickable") && hit.distance < 11)
                {
                    ClickableObject clickable = hit.collider.GetComponent<ClickableObject>();
                    if (clickable != null)
                    {
                        //Interact with Clickable
                    }
                }
            }
        });
    }



    //Interact Callback -> To Client
    private void Interact_Callback(ulong clientId, int callbackCode)
    {

    }

    //Lag Compensation Wrap
    private void Interact_LagCompensateRaycast(float networkTime, System.Action action)
    {
        LagCompensationManager.Simulate(NetworkingManager.Singleton.NetworkTime - networkTime, action);
    }



}
