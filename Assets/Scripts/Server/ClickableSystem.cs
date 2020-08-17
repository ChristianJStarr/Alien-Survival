using System;
using System.Collections.Generic;
using UnityEngine;

public class ClickableSystem : MonoBehaviour
{
    private GameServer gameServer;

    private List<Clickable> activeClickables;
    private void Start()
    {
        activeClickables = new List<Clickable>();
        gameServer = GameServer.singleton;
    }


    //Create Clickable Object
    public bool RegisterClickable(Clickable click, UIData uiData) 
    {
        if (!activeClickables.Contains(click))
        {
            click.uniqueId = GenerateUniqueId();
            if(click.clickType == 1) 
            {
                click.data = JsonUtility.ToJson(uiData);
            }
            else if(click.clickType == 2) 
            {
                click.data = JsonUtility.ToJson(uiData);
            }
            else if(click.clickType == 3) 
            {
                click.data = JsonUtility.ToJson(uiData);
            }
            activeClickables.Add(click);
            return true;
        }
        return false;
    }

    public bool RemoveClickable(Clickable click) 
    {
        if (activeClickables.Contains(click)) 
        {
            activeClickables.Remove(click);
            Destroy(click.gameObject);
            return true;
        }
        return false;
    }


    //Find Clickable By Unique ID
    public Clickable FindClickableByUnique(string uniqueId) 
    {
        if(activeClickables.Count > 0) 
        {
            foreach(Clickable click in activeClickables) 
            {
                if(click.uniqueId == uniqueId) 
                {
                    return click;
                }
            }
            return null;
        }
        return null;
    }

    //Generate Unique ID for Creation of Clickable
    private string GenerateUniqueId() 
    {
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[10];
        var random = new System.Random();
        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }
        return new string(stringChars);
    }
   

}
