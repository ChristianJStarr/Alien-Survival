using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementSystem : MonoBehaviour
{
    public Dictionary<ulong, PlayerControlObject> players = new Dictionary<ulong, PlayerControlObject>(); 
    
    private bool systemEnabled = false;


    public void StartSystem() 
    {
        systemEnabled = true;
    }
    public void StopSystem() 
    {
        systemEnabled = false;
    }


    public void IncomingCommand(PlayerCommand command) 
    {
        if (players.ContainsKey(command.clientId)) 
        {
            //Execute
            ExecuteCommand(command);
        }
        else 
        {
            //Populate then Execute

            PlayerControlObject controlObject = NetworkingManager.Singleton.ConnectedClients[command.clientId].PlayerObject.GetComponent<PlayerControlObject>();
            if(controlObject != null) 
            {
                players.Add(command.clientId, controlObject);
                ExecuteCommand(command);
            }
            
        }
    }

    private void ExecuteCommand(PlayerCommand command) 
    {
    }
}
