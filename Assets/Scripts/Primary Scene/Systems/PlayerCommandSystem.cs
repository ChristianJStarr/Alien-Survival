using MLAPI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCommandSystem : MonoBehaviour
{
#if ((UNITY_EDITOR && !UNITY_CLOUD_BUILD) || UNITY_SERVER)
    private bool systemEnabled = false;

    public PlayerInfoSystem playerInfoSystem;
    public PlayerInteractSystem playerInteractSystem;
    public PlayerObjectSystem playerObjectSystem;

    public Dictionary<ulong, Queue<PlayerCommand>> commandQueue = new Dictionary<ulong, Queue<PlayerCommand>>();
    
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

    private int commandsExecuted = 0;
    private float lastTime = 0;



    //Execute the Command (50 sends per second per player) 50 players = 2500 per second = EZ
    public void ExecuteCommand(PlayerCommand command)
    {
        if (commandQueue.ContainsKey(command.clientId))
        {
            commandQueue[command.clientId].Enqueue(command);
        }
        else
        {
            commandQueue.Add(command.clientId, new Queue<PlayerCommand>());
            commandQueue[command.clientId].Enqueue(command);
        }
    }

    private void FixedUpdate() 
    {
        #region Debug-Menu

        //Debug Counting
        if(NetworkingManager.Singleton != null && GameServer.singleton != null) 
        {
            float currentTime = NetworkingManager.Singleton.NetworkTime;
            if (currentTime - lastTime >= 1)
            {
                lastTime = currentTime;
                GameServer.singleton.DebugCommandPerSecond = commandsExecuted;
                commandsExecuted = 0;
            }
        }
        #endregion

        //Read Queue & Execute Commands
        if (systemEnabled && commandQueue.Count > 0)
        {
            ulong[] clientIds = commandQueue.Keys.ToArray();
            int client_count = clientIds.Length;
            
            for (int i = 0; i < client_count; i++)
            {
                if(commandQueue[clientIds[i]].Count > 0) 
                {
                    ExecuteCommandOnClient(clientIds[i], commandQueue[clientIds[i]].Dequeue());
                    commandsExecuted++;
                }
            }
        }
    }

    private void ExecuteCommandOnClient(ulong clientId, PlayerCommand command) 
    {
        PlayerControlObject controlObject = playerObjectSystem.GetControlObjectByClientId(clientId);
        if (controlObject == null) return;
        //Apply Correction
        if (command.correction)
        {
            if (Vector3.Distance(controlObject.transform.position, command.correction_position) < 10)
            {
                controlObject.transform.position = command.correction_position;
            }
        }

        //ROTATE Player
        controlObject.Rotate(command.look);

        //MOVE Player
        if (command.move.magnitude > 0 || command.jump || command.crouch)
        {
            controlObject.Move(command.move, command.jump, command.crouch);
        }

        //Selected Slot Holdable
        if (controlObject.selectedSlot != command.selectedSlot)
        {
            controlObject.selectedSlot = command.selectedSlot;
            controlObject.holdableState = 0;
            if (command.selectedSlot != 0)
            {
                Item item = playerInfoSystem.Inventory_GetItemBySlot(command.clientId, command.selectedSlot);
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

        //USE
        if (command.use)
        {
            if (!controlObject.use)
            {
                controlObject.use = true;
                playerInteractSystem.Interact(command.clientId, command.networkTime, controlObject);
            }
            else if (controlObject.useDelayTime <= NetworkingManager.Singleton.NetworkTime)
            {
                playerInteractSystem.Interact(command.clientId, command.networkTime, controlObject);
            }
        }

        //RELOAD
        if (command.reload) 
        {
            playerInfoSystem.Inventory_ReloadToDurability(command.clientId, command.selectedSlot);
        }

    }
#endif
}

