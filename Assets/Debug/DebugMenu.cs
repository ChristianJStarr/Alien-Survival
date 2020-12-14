using System;
using TMPro;
using UnityEngine;

public class DebugMenu : MonoBehaviour
{
    #region Singleton
    public static DebugMenu Singleton;
    private void Awake() { Singleton = this; }
    #endregion

    public TextMeshProUGUI command_line, position_line, objects_line, movement_line, connect_line, touchpad_line; //Debug Interface Text
    private int objectsLength, objectsLoaded, aiLength, playerLength; //Stored Variables for Updating Objects Line
    private ulong clientId;
    private string username;

    private string command_format = "CM: {0} CL: {1} J: {2} C: {3} R: {4} A: {5}";
    private string position_format = "P: {0} R: {1} V: {2}";
    private string objects_format = "P: {0} A: {1} W: {2}";
    private string movement_format = "F: {0} R: {1} M: {2}";
    private string connect_format = "CD: {0} N: {1} T: {2}";
    private string touchpad_format = "T: {0} S: {1} A: {2}";


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
        connect_line.text = string.Format(connect_format, clientId, username, 0);
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
        string time = hour + ":" + currentTime.Minutes + am;
        connect_line.text = string.Format(connect_format, clientId, username, time);
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
        int reload = 0; if (command.reload) reload = 1;
        int aim = 0;if (command.aim) aim = 1;
        command_line.text = string.Format(command_format, command.move.ToString("F2"), command.look.ToString("F2"), jump, crouch, reload, aim);
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
        position_line.text = string.Format(position_format, position.ToString("F2"), rotation.ToString("F2"), velocity.ToString("F2"));
        movement_line.text = string.Format(movement_format, forward.ToString("F2"), right.ToString("F2"), movement.ToString("F2"));
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
        objects_line.text = string.Format(objects_format, playerLength, aiLength,"(" + objectsLength + "/" + objectsLoaded + ")");
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
        objects_line.text = string.Format(objects_format, playerLength, aiLength, "(" + objectsLength + "/" + objectsLoaded + ")");
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
        objects_line.text = string.Format(objects_format, playerLength, aiLength, "(" + objectsLength + "/" + objectsLoaded + ")");
    }

    //Update Touchpad Stats
    public static void UpdateTouch(Vector2 touchAxis, Vector2 sensitivity, Vector2 acceleration) 
    {
        if(Singleton != null) 
        {
            Singleton.Update_Touch(touchAxis, sensitivity, acceleration);
        }
    }
    private void Update_Touch(Vector2 touchAxis, Vector2 sensitivity, Vector2 accelertation) 
    {
        touchpad_line.text = string.Format(touchpad_format, touchAxis.ToString("F2"), sensitivity.ToString("F2"), accelertation.ToString("F2")); 
    }

}
