using MLAPI;
using MLAPI.LagCompensation;
using MLAPI.Messaging;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameServer : NetworkedBehaviour
{

    #region Singleton

    public static GameServer singleton;

    void Awake()
    {
        singleton = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion
    public List<PlayerInfo> activePlayers;
    public List<PlayerInfo> inactivePlayers;
    private PlayerInfoManager playerInfoManager;
    private ServerSaveData serverSaveData;


    private void Start()
    {
        if (NetworkingManager.Singleton.IsServer) 
        {
            StartGameServer();
        }
    }

    //-----------------------------------------------------------------//
    //             SERVER FUNCTIONS                                    //
    //-----------------------------------------------------------------//

    private void StartGameServer()
    {
        Debug.Log("Network - Server - Starting Game Server");
        activePlayers = new List<PlayerInfo>();
        inactivePlayers = new List<PlayerInfo>();
        serverSaveData = Resources.Load("Data/ServerSaveData") as ServerSaveData;
        if (serverSaveData.playerData != null) 
        {
            inactivePlayers = serverSaveData.playerData;
        }
        playerInfoManager = PlayerInfoManager.singleton;
    }
    public void StopGameServer() 
    {
        inactivePlayers.Concat(activePlayers).ToList();
        if (serverSaveData != null)
        {
            serverSaveData.playerData = inactivePlayers;
            NetworkingManager.Singleton.StopServer();
        }
    }
    public void ActiveManage(PlayerInfo info, bool add) 
    {
        if (add) 
        {
            activePlayers.Add(info);    
        }
        else 
        {
            activePlayers.Remove(info);
        }
    }
    public void InactiveManage(PlayerInfo info, bool add)
    {
        if (add)
        {
            inactivePlayers.Add(info);
        }
        else
        {
            inactivePlayers.Remove(info);
        }
    }

    //-----------------------------------------------------------------//
    //             CLIENT CALLBACKS                                    //
    //-----------------------------------------------------------------//

    public void PlayerConnected_Player(ulong networkId)
    {
        Debug.Log("Network - Game - Connected to Server.");
        InvokeServerRpc(HandoverNetworkId, PlayerPrefs.GetInt("id") , PlayerPrefs.GetString("authKey") , networkId);
    }
    [ServerRPC(RequireOwnership = false)]
    private void HandoverNetworkId(int id, string authKey, ulong networkId)
    {
        GetActivePlayerByAuth(id, authKey).networkId = networkId;
    }
    public void PlayerDisconnected_Player(ulong obj)
    {
        
    }

    //-----------------------------------------------------------------//
    //             SERVER SIDE TOOLS                                   //
    //-----------------------------------------------------------------//

    private PlayerInfo GetActivePlayerByAuth(int id, string authKey) 
    {
        PlayerInfo info = null;
        foreach (PlayerInfo player in activePlayers)
        {
            if(player.id == id) 
            {
                if(player.authKey == authKey) 
                {
                    info = player;
                    break;
                }
            }
        }
        return info;
    }
    private PlayerInfo GetActivePlayerById(ulong networkId)
    {
        PlayerInfo info = null;
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.networkId == networkId)
            {
                    info = player;
                    break;
            }
        }
        return info;
    }
    private PlayerInfo GetActivePlayerByClientId(ulong clientId)
    {
        PlayerInfo info = null;
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.clientId == clientId)
            {
                info = player;
                break;
            }
        }
        return info;
    }
    public void MovePlayerToInactive(ulong clientId) 
    {
        PlayerInfo player = GetActivePlayerByClientId(clientId);
        inactivePlayers.Add(player);
        activePlayers.Remove(player);
    }

    //-----------------------------------------------------------------//
    //             Player Request : Retrieve Player Info               //
    //-----------------------------------------------------------------//

    //--Health
    public void GetPlayerHealth(int id, Action<int> callback) 
    {
        StartCoroutine(GetPlayerHealth_Wait(id, returnValue =>{callback(returnValue);}));
    }
    private IEnumerator GetPlayerHealth_Wait(int id, Action<int> callback) 
    {
        RpcResponse<int> response = InvokeServerRpc(GetPlayerHealth_Rpc, id);
        while (!response.IsDone){yield return null;}
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private int GetPlayerHealth_Rpc(int id)
    {
        int value = 0;
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                value = player.health;
                break;
            }
        }
        return value;
    }
    //--Water
    public void GetPlayerWater(int id, Action<int> callback)
    {
        StartCoroutine(GetPlayerWater_Wait(id, returnValue => { callback(returnValue); }));
    }
    private IEnumerator GetPlayerWater_Wait(int id, Action<int> callback)
    {
        RpcResponse<int> response = InvokeServerRpc(GetPlayerWater_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private int GetPlayerWater_Rpc(int id)
    {
        int value = 0;
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                value = player.water;
                break;
            }
        }
        return value;
    }
    //--Food
    public void GetPlayerFood(int id, Action<int> callback)
    {
        StartCoroutine(GetPlayerFood_Wait(id, returnValue => { callback(returnValue); }));
    }
    private IEnumerator GetPlayerFood_Wait(int id, Action<int> callback)
    {
        RpcResponse<int> response = InvokeServerRpc(GetPlayerFood_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private int GetPlayerFood_Rpc(int id)
    {
        int value = 0;
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                value = player.food;
                break;
            }
        }
        return value;
    }
    //--Inventory items
    public void GetPlayerInventoryItems(int id, Action<string> callback)
    {
        StartCoroutine(GetPlayerInventoryItems_Wait(id, returnValue => { callback(returnValue); }));
    }
    private IEnumerator GetPlayerInventoryItems_Wait(int id, Action<string> callback)
    {
        RpcResponse<string> response = InvokeServerRpc(GetPlayerInventoryItems_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private string GetPlayerInventoryItems_Rpc(int id)
    {
        string value = "";
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                value = player.items;
                break;
            }
        }
        return value;
    }
    //--Inventory armor
    public void GetPlayerInventoryArmor(int id, Action<string> callback)
    {
        StartCoroutine(GetPlayerInventoryArmor_Wait(id, returnValue => { callback(returnValue); }));
    }
    private IEnumerator GetPlayerInventoryArmor_Wait(int id, Action<string> callback)
    {
        RpcResponse<string> response = InvokeServerRpc(GetPlayerInventoryArmor_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private string GetPlayerInventoryArmor_Rpc(int id)
    {
        string value = "";
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                value = player.armor;
                break;
            }
        }
        return value;
    }
    //--Inventory blueprints
    public void GetPlayerInventoryBlueprints(int id, Action<string> callback)
    {
        StartCoroutine(GetPlayerInventoryBlueprints_Wait(id, returnValue => { callback(returnValue); }));
    }
    private IEnumerator GetPlayerInventoryBlueprints_Wait(int id, Action<string> callback)
    {
        RpcResponse<string> response = InvokeServerRpc(GetPlayerInventoryBlueprints_Rpc, id);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    [ServerRPC(RequireOwnership = false)]
    private string GetPlayerInventoryBlueprints_Rpc(int id)
    {
        string value = "";
        foreach (PlayerInfo player in activePlayers)
        {
            if (player.id == id)
            {
                value = player.blueprints;
                break;
            }
        }
        return value;
    }


    //-----------------------------------------------------------------//
    //             Player Request : Modify Own Values                  //
    //-----------------------------------------------------------------//

    //--Set Player Health
    public void SetPlayerHealth(int id, string authKey, int health) 
    {
        InvokeServerRpc(SetPlayerHealth_Rpc, id, authKey, health);
    }
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerHealth_Rpc(int id, string authKey, int health) 
    {
        PlayerInfo player = GetActivePlayerByAuth(id, authKey);
        GetActivePlayerByAuth(id, authKey).health += health;
        ForceRequestInfoById(player.clientId, 2);
    }
    //--Set Player Water
    public void SetPlayerWater(int id, string authKey, int water)
    {
        InvokeServerRpc(SetPlayerWater_Rpc, id, authKey, water);
    }
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerWater_Rpc(int id, string authKey, int water)
    {
        PlayerInfo player = GetActivePlayerByAuth(id, authKey);
        player.water +=water;
        ForceRequestInfoById(player.clientId, 4);
    }
    //--Set Player Food
    public void SetPlayerFood(int id, string authKey, int food)
    {
        InvokeServerRpc(SetPlayerFood_Rpc, id, authKey, food);
    }
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerFood_Rpc(int id, string authKey, int food)
    {
        PlayerInfo player = GetActivePlayerByAuth(id, authKey);
        player.food += food;
        ForceRequestInfoById(player.clientId, 3);
    }
    //--Set Player Inventory Items
    public void SetPlayerInventoryItems(int id, string authKey, string items)
    {
        InvokeServerRpc(SetPlayerInventoryItems_Rpc, id, authKey, items);
    }
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerInventoryItems_Rpc(int id, string authKey, string items)
    {
        PlayerInfo player = GetActivePlayerByAuth(id, authKey);
        player.items += items;
        ForceRequestInfoById(player.clientId, 5);
    }
    //--Set Player Inventory Armor
    public void SetPlayerInventoryArmor(int id, string authKey, string armor)
    {
        InvokeServerRpc(SetPlayerInventoryArmor_Rpc, id, authKey, armor);
    }
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerInventoryArmor_Rpc(int id, string authKey, string armor)
    {
        PlayerInfo player = GetActivePlayerByAuth(id, authKey);
        player.armor += armor;
        ForceRequestInfoById(player.clientId, 6);
    }
    //--Set Player Inventory Blueprints
    public void SetPlayerInventoryBlueprints(int id, string authKey, string blueprints)
    {
        InvokeServerRpc(SetPlayerInventoryBlueprints_Rpc, id, authKey, blueprints);
    }
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerInventoryBlueprints_Rpc(int id, string authKey, string blueprints)
    {
        PlayerInfo player = GetActivePlayerByAuth(id, authKey);
        player.blueprints += blueprints;
        ForceRequestInfoById(player.clientId, 7);
    }
    //--Set Player Location
    public void SetPlayerLocation(int id, string authKey, Vector3 location)
    {
        InvokeServerRpc(SetPlayerLocation_Rpc, id, authKey, location);
    }
    [ServerRPC(RequireOwnership = false)]
    private void SetPlayerLocation_Rpc(int id, string authKey, Vector3 location)
    {
        PlayerInfo player = GetActivePlayerByAuth(id, authKey);
        player.location += location;
    }



    //-----------------------------------------------------------------//
    //             Player Request : Raycast Hit                        //
    //-----------------------------------------------------------------//
    public void PlayerRayCastHit(NetworkedObject player, int type) 
    {
        InvokeServerRpc(PlayerRayCastHitRpc, player, type);
    }
    [ServerRPC(RequireOwnership = false)]
    private void PlayerRayCastHitRpc(NetworkedObject player, int type) 
    {

        float distance = 1F;
        int damage = 1;
        if(type == 1) { distance = 2f; }
        if (type == 2) { distance = 8f; }
        if (type == 3) { distance = 30f; }
        if (type == 4) { distance = 50f; }

        Transform playerTransform = player.GetComponent<Transform>();
        RaycastHit hit;
        LagCompensationManager.Simulate(player.NetworkId, () => 
        {
            if (Physics.Raycast(playerTransform.position, -Vector3.up, out hit, distance))
            {
                GameObject hitObject = hit.collider.gameObject;
                Vector3 hitPosition = hitObject.transform.position;
                string tag = hitObject.tag;

                if (tag == "Player") 
                {
                    ulong playerId = hitObject.GetComponent<NetworkedObject>().NetworkId;
                    PlayerInfo playerInfo = GetActivePlayerById(playerId);
                    int health = playerInfo.health;
                    if(health - damage <= 0) 
                    {
                        //Kill Player
                    }
                    else 
                    {
                        health -= damage;
                        ForceRequestInfoById(playerId, 2);
                    }
                }
                if (tag == "Enemy")
                {

                }
                if (tag == "Friendly")
                {

                }
                if (tag == "Tree")
                {

                }

            }
        });
    }

    //-----------------------------------------------------------------//
    //             Client RPC : Force Request                          //
    //-----------------------------------------------------------------//
    private void ForceRequestInfoById(ulong clientId, int infoDepth) 
    {
        List<ulong> idList = new List<ulong>();
        idList.Add(clientId);

        if(infoDepth == 1) 
        {
            InvokeClientRpc("ForceRequestInfoAll_Rpc", idList);
        }
        if (infoDepth == 2)
        {
            InvokeClientRpc("ForceRequestInfoHealth_Rpc", idList);
        }
        if (infoDepth == 3)
        {
            InvokeClientRpc("ForceRequestInfoFood_Rpc", idList);
        }
        if (infoDepth == 4)
        {
            InvokeClientRpc("ForceRequestInfoWater_Rpc", idList);
        }
        if (infoDepth == 5)
        {
            InvokeClientRpc("ForceRequestInfoItems_Rpc", idList);
        }
        if (infoDepth == 6)
        {
            InvokeClientRpc("ForceRequestInfoArmor_Rpc", idList);
        }
        if (infoDepth == 7)
        {
            InvokeClientRpc("ForceRequestInfoBlueprints_Rpc", idList);
        }
    }
    [ClientRPC]
    private void ForceRequestInfoAll_Rpc() 
    {
        PlayerInfoManager.singleton.GetPlayer_AllInfo();
    }
    [ClientRPC]
    private void ForceRequestInfoHealth_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_Health();
    }
    [ClientRPC]
    private void ForceRequestInfoFood_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_Food();
    }
    [ClientRPC]
    private void ForceRequestInfoWater_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_Water();
    }
    [ClientRPC]
    private void ForceRequestInfoItems_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_InventoryItems();
    }
    [ClientRPC]
    private void ForceRequestInfoArmor_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_InventoryArmor();
    }
    [ClientRPC]
    private void ForceRequestInfoBlueprints_Rpc()
    {
        PlayerInfoManager.singleton.GetPlayer_InventoryBlueprints();
    }

}


public class PlayerInfo 
{
    public string name;
    public string authKey;
    public int id;
    public int health;
    public int food;
    public int water;
    public Vector3 location;
    public ulong networkId;
    public string items;
    public string armor;
    public string blueprints;
    public ulong clientId;
}