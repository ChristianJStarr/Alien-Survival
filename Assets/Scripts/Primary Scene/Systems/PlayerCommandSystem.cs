using MLAPI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Working Perfectly as of 1/20/2021

public class PlayerCommandSystem : MonoBehaviour
{
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
    private bool systemEnabled = false;

    //Systems
    public PlayerInfoSystem playerInfoSystem;
    public PlayerInteractSystem playerInteractSystem;
    public PlayerObjectSystem playerObjectSystem;

    //Commands
    public Dictionary<ulong, StoredClientCommand> commands = new Dictionary<ulong, StoredClientCommand>();

    public bool StartSystem() 
    {
        systemEnabled = true;

        return true;
    }
    public bool StopSystem() 
    {
        systemEnabled = false;
        return true;
    }


    private void FixedUpdate()
    {
        if (systemEnabled && commands.Count > 0)
        {
            ulong[] clientIds = commands.Keys.ToArray();
            int client_count = clientIds.Length;
            for (int i = 0; i < client_count; i++)
            {
                StoredClientCommand command = commands[clientIds[i]];
                if (command.last_tick + 1 != command.stored[0].tick) 
                {
                    for (int e = 3; e > 0; e--)
                    {
                        int tick = command.stored[e].tick;
                        if (tick >= command.last_tick + 1) 
                        {
                            ExecuteClientCommand(clientIds[i], command.stored[e]);
                            command.last_tick = tick;
                        }
                    }
                    ExecuteClientCommand(clientIds[i], command.stored[0]);
                    command.last_tick = command.stored[0].tick;
                }
                else 
                {
                    ExecuteClientCommand(clientIds[i], command.stored[0]);
                    command.last_tick = command.stored[0].tick;
                }
            }
        }
    }

    public void StoreClientCommand(ClientCommand[] command)
    {
        if (commands.ContainsKey(command[0].clientId))
        {
            commands[command[0].clientId].stored = command;
        }
        else
        {
            commands.Add(command[0].clientId, new StoredClientCommand() 
            {
                stored = command
            });
        }
    }
    private void ExecuteClientCommand(ulong clientId, ClientCommand command) 
    {
        PlayerControlObject controlObject = playerObjectSystem.GetControlObjectByClientId(clientId);
        if (controlObject != null) 
        {
            if (command.look_axis.magnitude > 0)
            {
                controlObject.Rotate(command.look_axis);
            }

            if (command.move_axis.magnitude > 0 || command.jump || command.crouch)
            {
                controlObject.Move(command.move_axis, command.jump, command.crouch);
            }

            if (controlObject.selectedSlot != command.selected_slot)
            {
                controlObject.selectedSlot = command.selected_slot;
                controlObject.holdableState = 0;
                if (command.selected_slot != 0)
                {
                    Item item = playerInfoSystem.Inventory_GetItemBySlot(command.clientId, command.selected_slot);
                    if (item != null)
                    {
                        controlObject.selectedItem = item;
                        ItemData itemData = ItemDataManager.Singleton.GetItemData(item.itemId);
                        if (itemData != null && itemData.isHoldable && itemData.holdableId != 0)
                        {
                            if (itemData.holdableId != controlObject.holdableId)
                            {
                                controlObject.holdableId = itemData.holdableId;
                            }
                        }
                        else
                        {
                            controlObject.holdableId = 0;
                        }
                    }
                    else
                    {
                        controlObject.holdableId = 0;
                    }
                }
            }

            if (command.use)
            {
                if (!controlObject.use)
                {
                    controlObject.use = true;
                    playerInteractSystem.Interact(command.clientId, command.tick, controlObject);
                }
                else if (controlObject.useDelayTime <= NetworkingManager.Singleton.NetworkTime)
                {
                    playerInteractSystem.Interact(command.clientId, command.tick, controlObject);
                }
            }

            if (command.reload)
            {
                playerInfoSystem.Inventory_ReloadToDurability(command.clientId, command.selected_slot);
            }
        }
    }
#endif
}

public class StoredClientCommand 
{
    public int last_tick;
    public ClientCommand[] stored;
}

