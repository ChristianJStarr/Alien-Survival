using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionManager : MonoBehaviour
{

    #region Singleton

    public static PlayerActionManager singleton;

    void Awake()
    {
        singleton = this;
    }

    #endregion

    private int id;
    private string authKey;
    private GameServer gameServer;

    void Start()
    {
        id = PlayerPrefs.GetInt("id");
        authKey = PlayerPrefs.GetString("authKey");
        gameServer = GameServer.singleton;
    }

    public void InteractWithClickable(string uniqueId) 
    {
        gameServer.InteractWithClickable(id, authKey, uniqueId);
    }
}
