using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCommandSystem : MonoBehaviour
{
    private bool systemEnabled = false;
    public Dictionary<ulong, PlayerControlObject> players = new Dictionary<ulong, PlayerControlObject>();
#if UNITY_EDITOR
    public int ExecutionsPerSecond = 0;
    private int executionsCount;
#endif
    //Movement Vars

    public bool StartSystem() 
    {
        systemEnabled = true;
#if UNITY_EDITOR
        StartCoroutine(CountExecutions());
#endif
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

#if UNITY_EDITOR
    private IEnumerator CountExecutions() 
    {
        WaitForSeconds wait = new WaitForSeconds(1);
        while (systemEnabled) 
        {
            if(ExecutionsPerSecond != executionsCount) 
            {
                ExecutionsPerSecond = executionsCount;
            }
            executionsCount = 0;
            Debug.Log("Executions Per Second: " + ExecutionsPerSecond);
            yield return wait;
        }
    }

#endif


    //------------Player Object Commands------------//

    //Teleport To Player
    public void Teleport_ToPlayer(ulong clientId, ulong target_clientId) 
    {
        if(players.ContainsKey(clientId) && players.ContainsKey(target_clientId)) 
        {
            players[clientId].transform.position = players[target_clientId].transform.position;
        }
    }

    //Teleport To Vector3
    public void Teleport_ToVector(ulong clientId, Vector3 target_position) 
    {
        if (players.ContainsKey(clientId)) 
        {
            players[clientId].transform.position = target_position;
        }
    }


    //Execute the Command (60 sends per second per player) 50 players = 3000 per second
    public void ExecuteCommand(PlayerCommand command)
    {
        if (systemEnabled && players.ContainsKey(command.clientId)) 
        {
            PlayerControlObject controlObject = players[command.clientId];

            //ROTATE Player
            if(command.look.magnitude > 0)
            {
                controlObject.Rotate(command.look, command.sensitivity);
            }

            //MOVE Player
            if(command.move.magnitude > 0 || command.jump || command.crouch) 
            {
                controlObject.Move(command.move, command.jump, command.crouch);
            }
#if UNITY_EDITOR
            executionsCount++;
#endif
        }
    }
}

