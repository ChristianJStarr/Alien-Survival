using System;
using TMPro;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    #region Singleton
    public static DebugMenu Singleton;
    private void Awake() { Singleton = this; }
    #endregion

    public TextMeshProUGUI command_line, position_line, objects_line, movement_line, connect_line; //Debug Interface Text
    private int objectsLength, objectsLoaded, aiLength, playerLength; //Stored Variables for Updating Objects Line
    private ulong clientId;
    private string username;


    //Update Connect Stats
    public static void UpdateConnect(ulong _clientId, string _username) 
    {
        if(Singleton != null) 
        {
            Singleton.Update_Connect(_clientId, _username);
        }
    }
    private void Update_Connect(ulong _clientId, string _username) 
    {
        clientId = _clientId;
        username = _username;
        connect_line.text = "CD: " + clientId + " N: " + username;
    }

    //Update Game Time
    public static void UpdateTime(TimeSpan currentTime) 
    {
        if(Singleton != null) 
        {
            Singleton.Update_Time(currentTime);
        }
    }
    private void Update_Time(TimeSpan currentTime) 
    {
        string am = "am";
        int hour = currentTime.Hours;
        if (hour > 12) 
        {
            hour -= 12;
            am = "pm";
        }
        connect_line.text = "CD: " + clientId + " N: " + username + " T: " + hour + ":" + currentTime.Minutes + am;
    }


    //Update Player Command Stats
    public static void UpdateCommand(PlayerCommand command) 
    {
        if(Singleton != null) 
        {
            Singleton.Update_Command(command);
        }
    }
    private void Update_Command(PlayerCommand command)
    {
        int jump = 0; if (command.jump) jump = 1;
        int crouch = 0; if (command.crouch) crouch = 1;
        command_line.text = "CM: " + command.move.ToString("F2") + " CL: " + command.look.ToString("F2") + " J:" + jump + " C:" + crouch;
    }


    //Update Player Movement Stats
    public static void UpdateMovement(Vector3 position, Vector3 rotation, Vector3 velocity, Vector3 forward, Vector3 right, Vector3 movement) 
    {
        if(Singleton != null) 
        {
            Singleton.Update_Movement(position, rotation, velocity, forward, right, movement);
        }
    }
    private void Update_Movement(Vector3 position, Vector3 rotation, Vector3 velocity, Vector3 forward, Vector3 right, Vector3 movement) 
    {
        position_line.text = "P: " + position.ToString("F2") + " R: " + rotation.ToString("F2") + " V: " + velocity.ToString("F2");
        movement_line.text = "F: " + forward.ToString("F2") + " R: " + right.ToString("F2") + " M: " + movement.ToString("F2");
    }


    //Update World Object Stats
    public static void UpdateObjects(int _objectsLength, int _objectsLoaded) 
    {
        if(Singleton != null) 
        {
            Singleton.Update_Objects(_objectsLength, _objectsLoaded);
        }
    }
    private void Update_Objects(int _objectsLength, int _objectsLoaded) 
    {
        objectsLength = _objectsLength;
        objectsLoaded = _objectsLoaded;
        objects_line.text = "P: " + playerLength + " A: " + aiLength + " W: (" + objectsLength + "/" + objectsLoaded + ")";
    }
   
    
    //Update Player Stats
    public static void UpdatePlayers(int _playerLength)
    {
        if (Singleton != null)
        {
            Singleton.Update_Players(_playerLength);
        }
    }
    private void Update_Players(int _playerLength)
    {
        playerLength = _playerLength;
        objects_line.text = "P: " + playerLength + " A: " + aiLength + " W: (" + objectsLength + "/" + objectsLoaded + ")";
    }

    
    //Update AI Stats
    public static void UpdateAI(int _aiLength)
    {
        if (Singleton != null)
        {
            Singleton.Update_AI(_aiLength);
        }
    }
    private void Update_AI(int _aiLength)
    {
        aiLength = _aiLength;
        objects_line.text = "P: " + playerLength + " A: " + aiLength + " W: (" + objectsLength + "/" + objectsLoaded + ")";
    }
}
