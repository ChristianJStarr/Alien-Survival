using MLAPI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCommandSystem : MonoBehaviour
{
    private bool systemEnabled = false;

    public PlayerInfoSystem playerInfoSystem;
    public PlayerInteractSystem playerInteractSystem;

    public Dictionary<ulong, PlayerControlObject> players = new Dictionary<ulong, PlayerControlObject>();
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


    public void RegisterPlayer(ulong clientId, NetworkedObject playerObject) 
    {
        PlayerControlObject controlObject = playerObject.GetComponent<PlayerControlObject>();
        if(controlObject != null) 
        {
            if (players.ContainsKey(clientId)) { players.Remove(clientId); }
            players.Add(clientId, controlObject);
        }
    }
    public void RemovePlayer(ulong clientId) 
    {
        if (players.ContainsKey(clientId)) 
        {
            players.Remove(clientId);
        }
    }

    private int commandsExecuted = 0;
    private float lastTime = 0;

    //------------Player Object Commands------------//

    //Teleport To Player
    public void Teleport_ToPlayer(ulong clientId, ulong target_clientId) 
    {
        if(players.ContainsKey(clientId) && players.ContainsKey(target_clientId)) 
        {
            players[clientId].characterController.enabled = false;
            players[clientId].transform.position = players[target_clientId].transform.position;
            players[clientId].characterController.enabled = true;
        }
    }

    //Teleport To Vector3
    public void Teleport_ToVector(ulong clientId, Vector3 target_position) 
    {
        if (players.ContainsKey(clientId)) 
        {
            players[clientId].ApplyCorrection(target_position);
        }
    }
    public void Teleport_ToVector(ulong clientId, Vector3 target_position, Quaternion target_rotation)
    {
        if (players.ContainsKey(clientId))
        {
            players[clientId].ApplyCorrection(target_position);
            players[clientId].ApplyCorrection(new Vector2(0, target_rotation.eulerAngles.y));
        }
    }
    
    //Get Positions Array
    public Vector3[] GetPlayerPositionsArray() 
    {
        PlayerControlObject[] temp = players.Values.ToArray();
        int length = temp.Length;
        Vector3[] instance = new Vector3[length];
        for (int i = 0; i < length; i++)
        {
            instance[i] = temp[i].transform.position;
        }
        return instance;
    }

    //Execute the Command (60 sends per second per player) 50 players = 3000 per second
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

        //Read Queue & Execute Commands
        if (systemEnabled && commandQueue.Count > 0) 
        {
            ulong[] clients = players.Keys.ToArray();
            for (int i = 0; i < clients.Length; i++)
            {
                if (commandQueue.ContainsKey(clients[i]) && commandQueue[clients[i]].Count > 0) 
                {
                    commandsExecuted++;
                    PlayerCommand command = commandQueue[clients[i]].Dequeue();
                    PlayerControlObject controlObject = players[command.clientId];
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
                        if (command.selectedSlot != 0)
                        {
                            Item item = playerInfoSystem.Inventory_GetItemFromSlot(command.clientId, command.selectedSlot);
                            if (item != null)
                            {
                                controlObject.selectedItem = item;
                                ItemData itemData = ItemDataManager.Singleton.GetItemData(item.itemID);
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
                        else
                        {
                            controlObject.holdableId = 0;
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
                }
            }
        }
    }

}

