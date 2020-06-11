﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
/// <summary>
/// Web Server Handler. 
/// </summary>
public class WebServer : MonoBehaviour
{
    /// <summary>
    /// Web Server Host URL
    /// </summary>
    private string Host = "https://www.game.aliensurvival.com"; 
    /// <summary>
    /// Login/Signup File Name
    /// </summary>
    private string loginFile = "login.php"; 
    /// <summary>
    /// Player Stats File Name
    /// </summary>
    private string statsFile = "stats.php"; 
    /// <summary>
    /// Server List File Name
    /// </summary>
    private string serversFile = "servers.php"; 
    /// <summary>
    /// Check if already requesting.
    /// </summary>
    /// <summary>
    /// Make a Login Request. Returns bool depending on success of login. If successful, authKey and userId are stored in PlayerPrefs.
    /// </summary>
    /// <param name="username">Player Username</param>
    /// <param name="password">Player Password MD5 Format</param>
    /// <param name="onRequestFinished">Bool Callback</param>
    public void LoginRequest(string username, string password, Action<bool> onRequestFinished)
    {


        StartCoroutine(WebServerCredential(true, username, password , returnValue => 
        {
            onRequestFinished(returnValue);
        }));
    }
    /// <summary>
    /// Make a Signup Request. Returns bool depending on success of login. If successful, authKey and userId are stored in PlayerPrefs.
    /// </summary>
    /// <param name="username">Player Username</param>
    /// <param name="password">Player Password MD5 Format</param>
    /// <param name="authKey">Player Authentication Key</param>
    /// <param name="onRequestFinished"> Bool Callback</param>
    public void SignupRequest(string username, string password, string authKey, Action<bool> onRequestFinished) 
    {

        StartCoroutine(WebServerCredential(false, username, password, returnValue =>
        {
            onRequestFinished(returnValue);
        }, authKey));
    }
    /// <summary>
    /// Make a Stats Request. If successful, returns PlayerStats will not be null and will contain current stats.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="authKey"></param>
    /// <param name="onRequestFinished"></param>
    public void StatRequest(int userId, string authKey, Action<PlayerStats> onRequestFinished)
    {


        StartCoroutine(WebServerStatistics(true, userId, authKey, returnValue => { }, returnStats =>
        {
            onRequestFinished(returnStats);
        }));
    }
    /// <summary>
    /// Save Current Stats. Returns bool depending on success of save.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="authKey"></param>
    /// <param name="stats"></param>
    /// <param name="onRequestFinished"></param>
    public void StatSend(int userId, string authKey, PlayerStats stats, Action<bool> onRequestFinished)
    {


        StartCoroutine(WebServerStatistics(true, userId, authKey, returnValue =>
        {
            onRequestFinished(returnValue);
        }, returnStats => { }, stats));
    }
    /// <summary>
    /// Make a Login Request. Returns bool depending on success of login. If successful, authKey and userId are stored in PlayerPrefs.
    /// </summary>
    /// <param name="onRequestFinished"></param>
    public void ServerListRequest(Action<ServerList> onRequestFinished)
    {


        StartCoroutine(WebServerMaster(null, returnValue => 
        {
            onRequestFinished(returnValue);
        }));
    }
    /// <summary>
    /// Make a Signup Request. Returns bool depending on success of login. If successful, authKey and userId are stored in PlayerPrefs.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="onRequestFinished"></param>
    public void ServerListSend(Server server, Action<bool> onRequestFinished) 
    {


        StartCoroutine(WebServerMaster(server, null, returnValue =>
        {
            onRequestFinished(returnValue);
            
        }));
    }
   /// <summary>
   /// Update the Player Count on the Server List.
   /// </summary>
   /// <param name="name">Server Name</param>
   /// <param name="count">New Player Count</param>
   /// <param name="onRequestFinished">Callback bool</param>
    public void ServerListPlayerCount(string name, int count, Action<bool> onRequestFinished)
    {


        StartCoroutine(WebServerMasterCount(name,count, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    /// <summary>
    /// Handles login or signup requests. From Users mysql db.
    /// </summary>
    /// <param name="login"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="success"></param>
    /// <param name="authKey"></param>
    /// <returns></returns>
    private IEnumerator WebServerCredential(bool login, string username, string password, Action<bool> success=null, string authKey=null) 
    {
        if (login) 
        {
            WWWForm form = new WWWForm();
            form.AddField("username", username);
            form.AddField("password", password);
            form.AddField("action", "login");
            UnityWebRequest web = UnityWebRequest.Post(Host + "/" + loginFile, form);
            yield return web.SendWebRequest();
            if (web.downloadHandler.text.StartsWith("TRUE"))
            {
                string[] data = web.downloadHandler.text.Split(',');
                string newAuthKey = data[1];
                int userId = Convert.ToInt32(data[2]);
                PlayerPrefs.SetString("authKey", newAuthKey);
                PlayerPrefs.SetInt("userId", userId);
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.SetString("password", password);
                PlayerPrefs.Save();
                success(true);

            }
            else if (web.downloadHandler.text == "Wrong")
            {
                Debug.Log("Network - Web - Login: Wrong Password");
                success(false);
            }
            else if (web.downloadHandler.text == "No User")
            {
                Debug.Log("Network - Web - Login: No User with that Username");
                success(false);
            }
            else 
            {
                Debug.Log("Network - Web - Login: " + web.downloadHandler.text);
                success(false);
            }
        }
        else if(!string.IsNullOrEmpty(authKey))
        {
            WWWForm form = new WWWForm();
            form.AddField("username", username);
            form.AddField("password", password);
            form.AddField("authKey", authKey);
            form.AddField("action", "signup");
            UnityWebRequest web = UnityWebRequest.Post(Host + "/" + loginFile, form);
            yield return web.SendWebRequest();
            if (web.downloadHandler.text.StartsWith("TRUE"))
            {
                string[] data = web.downloadHandler.text.Split(',');
                int userId = Convert.ToInt32(data[1]);
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.SetString("password", password);
                PlayerPrefs.SetString("authKey", authKey);
                PlayerPrefs.SetInt("userId", userId);
                PlayerPrefs.Save();
                success(true);
            }
            else if (web.downloadHandler.text == "Taken")
            {
                Debug.Log("Network - Web - Signup: Username Taken");
                success(false);
            }
            else 
            {
                Debug.Log("Network - Web - Signup: Error " + web.downloadHandler.text);
                success(false);
            }
        }
        else 
        {
            Debug.Log("Network - Web - Signup: No Authkey provided");
            yield return new WaitForSeconds(1F);
            success(false);
        }
    }
    /// <summary>
    /// Handles stats get or set. From Users mysql db.
    /// </summary>
    /// <param name="get"></param>
    /// <param name="userId"></param>
    /// <param name="authKey"></param>
    /// <param name="success"></param>
    /// <param name="statsReturn"></param>
    /// <param name="stats"></param>
    /// <returns></returns>
    private IEnumerator WebServerStatistics(bool get, int userId, string authKey, Action<bool> success = null, Action<PlayerStats> statsReturn = null, PlayerStats stats = null)
    {
        if (get)
        {
            WWWForm form = new WWWForm();
            form.AddField("userId", userId);
            form.AddField("authKey", authKey);
            form.AddField("action", "get");
            UnityWebRequest web = UnityWebRequest.Post(Host + "/" + statsFile, form);
            yield return web.SendWebRequest();
            if (web.downloadHandler.text.StartsWith("TRUE"))
            {
                string[] floatData = web.downloadHandler.text.Split(',');
                string exp = floatData[1];
                string coins = floatData[2];
                string hours = floatData[3];
                if (exp == "") { exp = "0"; }
                if (coins == "") { coins = "50"; }
                if (hours == "") { hours = "0.01"; }


                PlayerStats newStats = new PlayerStats();
                newStats.playerExp = Convert.ToInt32(exp);
                newStats.playerCoins = Convert.ToInt32(coins);
                newStats.playerHours = float.Parse(hours);

                statsReturn(newStats);
                success(true);
            }
            else
            {
                success(false);
            }
        }
        else if(stats != null)
        {
            WWWForm form = new WWWForm();
            form.AddField("userId", userId);
            form.AddField("authKey", authKey);
            form.AddField("exp", stats.playerExp);
            form.AddField("coins", stats.playerCoins);
            form.AddField("hours", stats.playerHours.ToString());
            form.AddField("action", "set");
            UnityWebRequest web = UnityWebRequest.Post(Host + "/" + statsFile, form);
            yield return web.SendWebRequest();
            if (web.downloadHandler.text.StartsWith("TRUE"))
            {
                success(true);
            }
            else
            {
                success(false);
            }
        }
    }
    /// <summary>
    /// Handles serverlist get or update."master server". Array of Server objects json parsed from mysql db.
    /// </summary>
    /// <param name="server"></param>
    /// <param name="serverSuccess"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    private IEnumerator WebServerMaster(Server server, Action<ServerList> serverSuccess=null, Action<bool> success = null)
    {

        if (server == null)
        {
            WWWForm form = new WWWForm();
            form.AddField("action", "request");
            UnityWebRequest web = UnityWebRequest.Post(Host + "/" + serversFile, form);
            yield return web.SendWebRequest();

            if (web.downloadHandler.text.StartsWith("TRUE"))
            {
                string[] data = web.downloadHandler.text.Split('`');
                ServerList serverList = new ServerList();
                string json = "{ \"server\": " + data[1] + "}";
                serverList.servers = JsonHelper.FromJson<Server>(json);
                serverSuccess(serverList);
            }
            else
            {
                Debug.Log("Network - Web - Master Server Error: " + web.downloadHandler.text);
                serverSuccess(null);
            }
        }
        else
        {
            WWWForm form = new WWWForm();
            form.AddField("name", server.name);
            form.AddField("description", server.description);
            form.AddField("map", server.map);
            form.AddField("mode", server.mode);
            form.AddField("serverIP", server.serverIP);
            form.AddField("serverPort", server.serverPort);
            form.AddField("player", server.player);
            form.AddField("maxPlayer", server.maxPlayer);
            form.AddField("action", "update");
            UnityWebRequest web = UnityWebRequest.Post(Host + "/" + serversFile, form);
            yield return web.SendWebRequest();
            if (web.downloadHandler.text.StartsWith("TRUE"))
            {                
                success(true);
            }
            else
            {
                Debug.Log("Network - Web - Master Server Send Error: " + web.downloadHandler.text);
                success(false);
            }
        }
    }
    /// <summary>
    /// Change the player count on the server list.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="count"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    private IEnumerator WebServerMasterCount(string name,int count, Action<bool> success = null)
    {
            WWWForm form = new WWWForm();
            form.AddField("name", name);
            form.AddField("player", count);
            form.AddField("action", "playerCount");
            UnityWebRequest web = UnityWebRequest.Post(Host + "/" + serversFile, form);
            yield return web.SendWebRequest();
            if (web.downloadHandler.text.StartsWith("TRUE"))
            {
                success(true);
            }
            else
            {
                Debug.Log("Network - Web - Master Server Error: " + web.downloadHandler.text);
                success(false);
            }
    }
}
/// <summary>
/// Server List Object. Contains Server[]
/// </summary>
[Serializable]
public class ServerList
{
    public Server[] servers;
}
/// <summary>
/// Server Object
/// </summary>
[Serializable]
public class Server
{
    /// <summary>
    /// Server Name
    /// </summary>
    public string name = "Server Name";
    /// <summary>
    /// Server Description
    /// </summary>
    public string description = "Server Description";
    /// <summary>
    /// Server Map Type
    /// </summary>
    public string map = "Default Map";
    /// <summary>
    /// Server Game Mode
    /// </summary>
    public string mode = "Game Mode";
    /// <summary>
    /// Server IP Address
    /// </summary>
    public string serverIP = "0.0.0.0";
    /// <summary>
    /// Server Port
    /// </summary>
    public ushort serverPort = 44444;
    /// <summary>
    /// Server Player Count
    /// </summary>
    public int player = 0;
    /// <summary>
    /// Server Max Players
    /// </summary>
    public int maxPlayer = 0;
    /// <summary>
    /// Server Ping
    /// </summary>
    public int ping = 0;
}
