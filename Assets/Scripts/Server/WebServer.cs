using MLAPI;
using System;
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

    private NetworkingManager networkManager;
    public int logLevel = 0;
    public PlayerStats playerStats;
    private TimeZoneInfo serverTimeZone;

    public MainMenuAlienStore alienStore;

    private void Start()
    {
        networkManager = NetworkingManager.Singleton;
     
    }

    public void LoginRequest(string username, string password, Action<string> onRequestFinished)
    {


        StartCoroutine(WebServerCredential(true, username, password, returnValue =>
       {
           onRequestFinished(returnValue);
       }));
    }
    public void SignupRequest(string username, string password, string authKey, Action<string> onRequestFinished)
    {

        StartCoroutine(WebServerCredential(false, username, password, returnValue =>
        {
            onRequestFinished(returnValue);
        }, authKey));
    }
    public void StatRequest(int userId, string authKey, Action<bool> onRequestFinished)
    {
        StartCoroutine(WebServerStatistics(userId, authKey, returnValue =>
       {
           onRequestFinished(returnValue);
       }));
    }
    public void StatSend(int userId, string authKey, string authToken, int expAdd, int coinsAdd, float hoursAdd, string notifyData, string storeSet, Action<bool> onRequestFinished)
    {
        StartCoroutine(WebServerSetStatistics(userId, authKey, authToken, expAdd, coinsAdd, notifyData, hoursAdd, storeSet, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    public void ServerListRequest(Action<ServerList> onRequestFinished)
    {


        StartCoroutine(WebServerMaster(null, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    public void ServerListSend(Server server, Action<bool> onRequestFinished)
    {
        StartCoroutine(WebServerMaster(server, null, returnValue =>
        {
            onRequestFinished(returnValue);

        }));
    }
    public void ServerListUpdateRecent(string serverName, string serverIp, Action<bool> onRequestFinished)
    {
        StartCoroutine(WebServerMasterRecent(serverName, serverIp, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    public void ServerListPlayerCount(string name, int count, Action<bool> onRequestFinished)
    {


        StartCoroutine(WebServerMasterCount(name, count, returnValue =>
         {
             onRequestFinished(returnValue);
         }));
    }

    public void AlienStorePurchase(int userId, string authKey, int itemId)
    {
        StartCoroutine(WebServerStore(userId, authKey, itemId));
    }

    private IEnumerator WebServerStore(int userId, string authKey, int itemId) 
    {
        WWWForm form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("authKey", authKey);
        form.AddField("itemId", itemId);
        form.AddField("action", "purchase");
        UnityWebRequest web = UnityWebRequest.Post(Host + "/" + statsFile, form);
        yield return web.SendWebRequest();
        if (web.downloadHandler.text.StartsWith("TRUE"))
        {
            DebugMessage("Purchased Item: " + itemId, 3);
            string[] floatData = web.downloadHandler.text.Split('!');
            string storeData = floatData[1];
            string notifyData = floatData[2];
            string exp = floatData[3];
            string coins = floatData[4];
            string hours = floatData[5];
            if (hours == "") { hours = "0.01"; }
            if (exp == "") { exp = "0"; }
            if (coins == "") { coins = "0"; }
            playerStats.playerExp = Convert.ToInt32(exp);
            playerStats.playerCoins = Convert.ToInt32(coins);
            playerStats.playerHours = float.Parse(hours);
            playerStats.notifyData = notifyData;
            playerStats.storeData = storeData;
            
            if(alienStore != null) 
            {
                alienStore.UpdateStats();
            }
        }
        else
        {
            DebugMessage("AlienStore Purchase Failed. Message: " + web.downloadHandler.text, 1);
        }
    }

    private IEnumerator WebServerCredential(bool login, string username, string password, Action<string> success=null, string authKey=null) 
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
                success("TRUE");

            }
            else if (web.downloadHandler.text == "Wrong")
            {
                Debug.Log("Network - Web - Login: Wrong Password");
                success("WRONG");
            }
            else if (web.downloadHandler.text == "No User")
            {
                Debug.Log("Network - Web - Login: No User with that Username");
                success("NOUSER");
            }
            else 
            {
                Debug.Log("Network - Web - Login: " + web.downloadHandler.text);
                success("ERROR");
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
                success("TRUE");
            }
            else if (web.downloadHandler.text == "Taken")
            {
                Debug.Log("Network - Web - Signup: Username Taken");
                success("TAKEN");
            }
            else 
            {
                Debug.Log("Network - Web - Signup: Error " + web.downloadHandler.text);
                success("ERROR");
            }
        }
        else 
        {
            Debug.Log("Network - Web - Signup: No Authkey provided");
            yield return new WaitForSeconds(.1F);
            success("ERROR");
        }
    }

    private IEnumerator WebServerStatistics(int userId, string authKey, Action<bool> success = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("authKey", authKey);
        UnityWebRequest web = UnityWebRequest.Post(Host + "/" + statsFile, form);
        yield return web.SendWebRequest();
        if (web.downloadHandler.text.StartsWith("TRUE"))
        {
            string[] floatData = web.downloadHandler.text.Split('!');
            string storeData = floatData[1];
            string notifyData = floatData[2];
            string exp = floatData[3];
            string coins = floatData[4];
            string hours = floatData[5];
            if (hours == "") { hours = "0.01"; }
            if (exp == "") { exp = "0"; }
            if (coins == "") { coins = "0"; }
            playerStats.playerExp = Convert.ToInt32(exp);
            playerStats.playerCoins = Convert.ToInt32(coins);
            playerStats.playerHours = float.Parse(hours);
            playerStats.notifyData = notifyData;
            playerStats.storeData = storeData;
            DebugMessage("Statistics Request Successful.", 2);
            success(true);
        }
        else
        {
            DebugMessage("Statistics Request Failed. Message: " + web.downloadHandler.text, 1);
            success(false);
        }
    }

    private IEnumerator WebServerSetStatistics(int userId, string authKey, string authToken, int expAdd, int coinsAdd, string notifyData, float hoursAdd, string storeSet, Action<bool> success = null) 
    {
        WWWForm form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("authKey", authKey);
        form.AddField("authToken", authToken);
        form.AddField("exp", expAdd);
        form.AddField("coins", coinsAdd);
        form.AddField("hours", hoursAdd.ToString());
        form.AddField("store", storeSet);
        form.AddField("notify", notifyData);
        UnityWebRequest web = UnityWebRequest.Post(Host + "/" + statsFile, form);

        yield return web.SendWebRequest();
        
        if (web.downloadHandler.text.StartsWith("TRUE"))
        {
            DebugMessage("Successfully Set Player Statistics: " + web.downloadHandler.text, 2);
            success(true);
        }
        else
        {
            DebugMessage("Failed Setting Player Statistics: " + web.downloadHandler.text, 1);
            success(false);
        }
    }

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
                DebugMessage("Master Server Success: ServerList", 3);
                string[] data = web.downloadHandler.text.Split('`');
                ServerList serverList = new ServerList();
                string json = "{ \"server\": " + data[1] + "}";
                serverList.servers = JsonHelper.FromJson<Server>(json);
                serverSuccess(serverList);
            }
            else
            {
                DebugMessage("Master Server Error: " + web.downloadHandler.text, 1);
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
            form.AddField("recent", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Pacific Standard Time").ToString());
            UnityWebRequest web = UnityWebRequest.Post(Host + "/" + serversFile, form);
            yield return web.SendWebRequest();
            if (web.downloadHandler.text.StartsWith("TRUE"))
            {                
                success(true);
            }
            else
            {
                DebugMessage("Master Server Send Error: " + web.downloadHandler.text, 1);
                success(false);
            }
        }
    }

    private IEnumerator WebServerMasterCount(string name,int count, Action<bool> success = null)
    {
            WWWForm form = new WWWForm();
            form.AddField("name", name);
            form.AddField("player", count);
            form.AddField("recent", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Pacific Standard Time").ToString());
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

    private IEnumerator WebServerMasterRecent(string serverName, string serverIp, Action<bool> success = null)
    {
        WWWForm form = new WWWForm();
        //form.AddField("name", serverName);
        form.AddField("serverIp", serverIp);
        form.AddField("action", "recent");
        form.AddField("recent", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Pacific Standard Time").ToString());
        UnityWebRequest web = UnityWebRequest.Post(Host + "/" + serversFile, form);
        yield return web.SendWebRequest();
        if (web.downloadHandler.text.StartsWith("TRUE"))
        {
            success(true);
        }
        else
        {
            DebugMessage("Master Server Send Error: " + web.downloadHandler.text, 1);
            success(false);
        }
    }

    //Debug Message
    private void DebugMessage(string message, int level)
    {
        if (networkManager != null)
        {
            if (level <= logLevel)
            {
                if (networkManager.IsServer)
                {
                    Debug.Log("[Server] ServerConnect : " + message);
                }
                else
                {
                    Debug.Log("[Client] ServerConnect : " + message);
                }
            }
        }
    }

}

[Serializable]
public class ServerList
{
    public Server[] servers;
}

[Serializable]
public class Server
{
    public string name = "Server Name";
    public string description = "Server Description";
    public string map = "Default Map";
    public string mode = "Game Mode";
    public string serverIP = "0.0.0.0";
    public ushort serverPort = 44444;
    public int player = 0;
    public int maxPlayer = 0;
    public int ping = 0;
}
