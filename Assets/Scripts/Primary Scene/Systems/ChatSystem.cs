using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatSystem : MonoBehaviour
{

    //ALL - Player has Connected
    public void PlayerConnected_AllMessage(string name) 
    {
        SendToAll(StringColorizor(name, "FF5F5F") + " has connected.");   
    }
    //ALL - Player has Disconnected
    public void PlayerDisconnected_AllMessage(string name) 
    {
        SendToAll(StringColorizor(name, "FF5F5F") + " has disconnected.");
    }
    //ALL - Player Killed by Killer
    public void PlayerKilled_AllMessage(string name, string killer) 
    {
        SendToAll(StringColorizor(name, "FF5F5F") + " was killed by " + StringColorizor(killer, "FF5F5F") + ".");
    }

    //SPECIFIC - Player Welcome MOTD
    public void PlayerWelcome_Specific(string name, string servername, ulong clientId) 
    {
        SendToSpecific("Welcome " + StringColorizor(name,"FF5F5F") + " to " + servername + "! Current Time: " + StringColorizor("3:10PM" ,"FF5F5F") + ".", clientId);
    }



    //SEND TO ALL - TASK
    private void SendToAll(string message) 
    {
        GameServer.singleton.Chat_SendToAll(message);
    }

    //SEND TO SPECIFIC - TASK
    private void SendToSpecific(string message, ulong clientId) 
    {
        GameServer.singleton.Chat_SendToSpecific(message, clientId);
    }


    //COLORIZE - TOOL
    private string StringColorizor(string message, string hex)
    {
        return "<color=#" + hex + ">" + message + "</color>";
    }
}
