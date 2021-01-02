using MLAPI;
using MLAPI.LagCompensation;
using UnityEngine;

public class PlayerInteractSystem : MonoBehaviour
{
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
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

    public GameServer gameServer;
    public PlayerObjectSystem playerObjectSystem;
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
            Item item = playerInfoSystem.Inventory_GetItemBySlot(clientId, controlObject.selectedSlot);
            if (item != null)
            {
                //Item in hand
                ItemData data = ItemDataManager.Singleton.GetItemData(item.itemId);
                if (data != null && data.isUsable) //Item can be used 
                {
                    if (data.useType == 1) //Shoot
                    {
                        Interact_Shoot(clientId, networkTime, item, data, controlObject);
                        return;
                    }
                    else if (data.useType == 2) //Melee
                    {
                        Interact_Melee(clientId, networkTime, item, data, controlObject);
                        return;
                    }
                    else if (data.useType == 3) //Tool
                    {
                        Interact_Tool(clientId, networkTime, item, data, controlObject);
                        return;
                    }
                    else if (data.useType == 4) //Consume
                    {
                        Interact_Consume(clientId, networkTime, item, data, controlObject);
                        return;
                    }
                    else if (data.useType == 5) //Build
                    {
                        Interact_Build(clientId, networkTime, item, data, controlObject);
                        return;
                    }
                }
            }
        } 
        Interact_Clickable(clientId, networkTime, controlObject);
    }

    //Shoot
    private void Interact_Shoot(ulong clientId, float networkTime, Item item, ItemData data, PlayerControlObject controlObject)
    {
        //If enough Ammo
        //Set Holdable State Primary
        //Spawn muzzleflash particle
        //Play shot sound
        //Detect Damage

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
                        Interact_ClickableTask(clientId, clickable);
                    }
                }
                else if (item.durability > 0 && playerInfoSystem.Inventory_ChangeItemDurability(clientId, item.currSlot, -1))
                {
                    //Effect Durability
                    item.durability -= 1;

                    //Set Animation State
                    controlObject.holdableId = data.holdableId;
                    controlObject.holdableState = 1;

                    //Spawn Particle
                    gameServer.Server_SpawnParticle(data.useParticleId, controlObject.holdableObject.useParticlePoint.position, 100);

                    //Play Gunshot Sound
                    gameServer.Server_PlaySoundEffect(data.useSoundId, controlObject.transform.position, 500, clientId);


                    //Damage Health of Object
                    if (hit.distance <= data.useRange)
                    {
                        int particleId = 1; //Particle ID
                        if (hit.collider.CompareTag("Player"))
                        {
                            PlayerControlObject hitPlayer = hit.collider.GetComponent<PlayerControlObject>();
                            if (hitPlayer != null)
                            {
                                //Damage Player
                                playerInfoSystem.SetPlayerHealth(hitPlayer.OwnerClientId, data.useAmount, true);
                                //Set Particle
                                particleId = 2;
                            }
                        }
                        else if (hit.collider.CompareTag("AI"))
                        {
                            AIControlObject hitAI = hit.collider.GetComponent<AIControlObject>();
                            if (hitAI != null)
                            {
                                //Damage AI
                                worldAISystem.DamageAI(hitAI.NetworkId, data.useAmount);
                                //Set Particle
                                particleId = 2;
                            }
                        }


                        //Spawn Particle
                        gameServer.Server_SpawnParticle(data.useParticleId, controlObject.holdableObject.useParticlePoint.position, 100);

                        //Play Hit Sound
                        gameServer.Server_PlaySoundEffect(data.hitSoundId, hit.transform.position, 500);
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
                        Interact_ClickableTask(clientId, clickable);
                    }
                }
            }
        });
    }

    private void Interact_ClickableTask(ulong clientId, ClickableObject clickable) 
    {
        Debug.Log("Client:" + clientId + " Interacting with Clickable");
    }


    //Interact Callback -> To Client
    private void Interact_Callback(ulong clientId, int callbackCode)
    {

    }

    //Lag Compensation Wrapper
    private void Interact_LagCompensateRaycast(float networkTime, System.Action action)
    {
        LagCompensationManager.Simulate(NetworkingManager.Singleton.NetworkTime - networkTime, action);
    }


#endif
}
