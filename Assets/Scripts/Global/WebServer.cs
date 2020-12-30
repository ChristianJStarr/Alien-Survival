using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebServer : MonoBehaviour
{
    // Web Server Host URL
    private string Host = "https://www.game.aliensurvival.com";
    //Files
    private string loginFile = "login.php";
    private string statsFile = "stats.php";
    private string serversFile = "servers.php";
    //Stats
    public PlayerStats playerStats;

    //Token
    private string client_verifyToken = "3c893731ab7cdd266b0affdbb0535bfe";
    
    #region Client : Login Request
    public void LoginRequest(string user_token, string user_authkey, Action<LoginRequestData> returnData)
    {
        StartCoroutine(WebServerCredential(user_token, user_authkey, null, returnValue =>
        {
           returnData(returnValue);
        }));
    }
    public void SignupRequest(string user_token, string user_authkey, string user_name, Action<LoginRequestData> returnData)
    {
        StartCoroutine(WebServerCredential(user_token, user_authkey, user_name, returnValue =>
        {
            returnData(returnValue);
        }));
    }
    private IEnumerator WebServerCredential(string user_token, string user_authkey, string user_name, Action<LoginRequestData> returnData)
    {
        LoginRequestData requestData = new LoginRequestData();
        WWWForm form = new WWWForm();
        form.AddField("verify", client_verifyToken);
        if(user_name != null) 
        {
            form.AddField("username", user_name);
        }
        form.AddField("token", user_token);
        form.AddField("authKey", user_authkey);
        UnityWebRequest web = UnityWebRequest.Post(Host + "/" + loginFile, form);
        yield return web.SendWebRequest();
        if (web.downloadHandler.text.StartsWith("TRUE"))
        {
            string[] data = web.downloadHandler.text.Split(',');
            if (user_name == null) { user_name = data[1]; }
            int user_id = Convert.ToInt32(data[2]);
            requestData.credentials = new UserCredentialData();
            requestData.credentials.Set(user_name, user_token, user_authkey, user_id);
            requestData.successful = true;
        }
        else if(web.downloadHandler.text == "SIGNUP")
        {
            requestData.needsToSignup = true;
            requestData.successful = false;
        }
        else if (web.downloadHandler.text == "WRONG")
        {
            requestData.successful = false;
            requestData.errorCode = 1;
        }
        else if (web.downloadHandler.text == "INVALID")
        {
            requestData.successful = false;
            requestData.errorCode = 2;
        }
        else if(web.downloadHandler.text == "EXISTS") 
        {
            requestData.successful = false;
            requestData.errorCode = 3;
        }
        else
        {
            requestData.successful = false;
            requestData.errorCode = 4;
        }
        returnData(requestData);
    }
#endregion

    #region Client : Stat Request
    public void StatRequest(int userId, string authKey, Action<StatRequestData> returnData)
    {
        StartCoroutine(WebServerStatistics(userId, authKey, requestData =>
        {
            returnData(requestData);
        }));
    }
    private IEnumerator WebServerStatistics(int userId, string authKey, Action<StatRequestData> returnData)
    {
        if (userId != 0 && authKey.Length > 0)
        {
            StatRequestData requestData = new StatRequestData();
            WWWForm form = new WWWForm();
            form.AddField("userId", userId);
            form.AddField("authKey", authKey);
            form.AddField("action", "request");
            form.AddField("verify", client_verifyToken);
            UnityWebRequest web = UnityWebRequest.Post(Host + "/" + statsFile, form);
            yield return web.SendWebRequest();
            string webData = web.downloadHandler.text;
            if (webData.StartsWith("TRUE"))
            {
                requestData.successful = true;
                requestData.stats = UserStatsData.Generate(webData);
            }
            else
            {
                requestData.successful = false;
                if (webData == "WRONG")
                {
                    requestData.errorCode = 1;
                }
                else if (webData == "NONE")
                {
                    requestData.errorCode = 2;
                }
            }
            returnData(requestData);
        }
    }
#endregion

    #region Client : Server List Request
    public void ServerListRequest(Action<ListRequestData> returnData)
    {
        StartCoroutine(WebServerListRequest(requestData =>
        {
            returnData(requestData);
        }));
    }
    private IEnumerator WebServerListRequest(Action<ListRequestData> returnData)
    {
        ListRequestData requestData = new ListRequestData();
        WWWForm form = new WWWForm();
        form.AddField("action", "request");
        form.AddField("verify", client_verifyToken);
        UnityWebRequest web = UnityWebRequest.Post(Host + "/" + serversFile, form);
        yield return web.SendWebRequest();
        string webData = web.downloadHandler.text;
        if (!webData.StartsWith("NONE") && webData.Length > 0)
        {
            requestData.successful = true;
            requestData.servers = ServerListData.Generate(JsonHelper.FromJson<Server>("{ \"server\": " + webData + "}"));
        }
        else if (webData.StartsWith("NONE"))
        {
            requestData.successful = true;
        }
        else
        {
            requestData.successful = false;
            requestData.errorCode = 1;
        }
        returnData(requestData);
    }
#endregion

    #region Client : Store Purchase
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
        form.AddField("verify", client_verifyToken);
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
        }
    }
    #endregion

    #region Server : ALL
#if UNITY_EDITOR
        private string server_verifyToken = "c06276de863dc56c405e8d986b7269af";
    #region Server : Set Server List
    public void ServerListSet(Server server, Action<bool> onRequestFinished)
    {
        Debug.Log("Setting Server List");
        StartCoroutine(WebServerListSet(server, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    private IEnumerator WebServerListSet(Server server, Action<bool> success)
    {
        WWWForm form = new WWWForm();
        form.AddField("server_name", server.server_name);
        form.AddField("server_description", server.server_description);
        form.AddField("server_map", server.server_map);
        form.AddField("server_mode", server.server_mode);
        form.AddField("server_Ip", server.server_Ip);
        form.AddField("server_Port", server.server_Port);
        form.AddField("server_players", server.server_players);
        form.AddField("server_maxPlayers", server.server_maxPlayers);
        form.AddField("verify", server_verifyToken);
        form.AddField("action", "update");
        form.AddField("recent", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Pacific Standard Time").ToString());
        UnityWebRequest web = UnityWebRequest.Post(Host + "/" + serversFile, form);
        yield return web.SendWebRequest();
        if (web.downloadHandler.text.StartsWith("TRUE"))
        {
            Debug.Log("ServerList Success");
            success(true);
        }
        else
        {
            Debug.Log("ServerList Failed");
            Debug.Log(web.downloadHandler.text);
            success(false);
        }
    }
    #endregion

    #region Server : Set Server Count
    public void ServerListPlayerCount(string name, int count, Action<bool> onRequestFinished)
    {
        StartCoroutine(WebServerMasterCount(name, count, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    private IEnumerator WebServerMasterCount(string name, int count, Action<bool> success = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("verify", server_verifyToken);
        form.AddField("server_name", name);
        form.AddField("server_players", count);
        form.AddField("recent", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Pacific Standard Time").ToString());
        form.AddField("action", "count");
        UnityWebRequest web = UnityWebRequest.Post(Host + "/" + serversFile, form);
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
    #endregion

    #region Server : Set Server Recent
    public void ServerListUpdateRecent(string serverIp, ushort serverPort, Action<bool> onRequestFinished)
    {
        StartCoroutine(WebServerMasterRecent(serverIp, serverPort, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    private IEnumerator WebServerMasterRecent(string serverIp, ushort serverPort, Action<bool> success = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("verify", server_verifyToken);
        form.AddField("server_Ip", serverIp);
        form.AddField("server_Port", serverPort);
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
            success(false);
        }
    }
    #endregion

    #region Server : Set Client Stats
    public void SetClientStats(int userId, string authKey, int expAdd, int coinsAdd, float hoursAdd, string notifyData, string storeSet, int kills, int deaths, Action<bool> onRequestFinished)
    {
        StartCoroutine(WebServerSetStatistics(userId, authKey, expAdd, coinsAdd, notifyData, hoursAdd, storeSet, kills, deaths, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    private IEnumerator WebServerSetStatistics(int userId, string authKey, int expAdd, int coinsAdd, string notifyData, float hoursAdd, string storeSet, int kills, int deaths, Action<bool> success = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("authKey", authKey);
        form.AddField("verify", server_verifyToken);
        form.AddField("exp", expAdd);
        form.AddField("coins", coinsAdd);
        form.AddField("hours", hoursAdd.ToString());
        form.AddField("store", storeSet);
        form.AddField("notify", notifyData);
        form.AddField("kills", kills);
        form.AddField("deaths", deaths);
        form.AddField("action", "update");
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
    #endregion
#endif
#if UNITY_SERVER
        private string server_verifyToken = "c06276de863dc56c405e8d986b7269af";
    #region Server : Set Server List
    public void ServerListSet(Server server, Action<bool> onRequestFinished)
    {
        Debug.Log("Setting Server List");
        StartCoroutine(WebServerListSet(server, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    private IEnumerator WebServerListSet(Server server, Action<bool> success)
    {
        WWWForm form = new WWWForm();
        form.AddField("server_name", server.server_name);
        form.AddField("server_description", server.server_description);
        form.AddField("server_map", server.server_map);
        form.AddField("server_mode", server.server_mode);
        form.AddField("server_Ip", server.server_Ip);
        form.AddField("server_Port", server.server_Port);
        form.AddField("server_players", server.server_players);
        form.AddField("server_maxPlayers", server.server_maxPlayers);
        form.AddField("verify", server_verifyToken);
        form.AddField("action", "update");
        form.AddField("recent", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Pacific Standard Time").ToString());
        UnityWebRequest web = UnityWebRequest.Post(Host + "/" + serversFile, form);
        yield return web.SendWebRequest();
        if (web.downloadHandler.text.StartsWith("TRUE"))
        {
            Debug.Log("ServerList Success");
            success(true);
        }
        else
        {
            Debug.Log("ServerList Failed");
            Debug.Log(web.downloadHandler.text);
            success(false);
        }
    }
    #endregion

    #region Server : Set Server Count
    public void ServerListPlayerCount(string name, int count, Action<bool> onRequestFinished)
    {
        StartCoroutine(WebServerMasterCount(name, count, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    private IEnumerator WebServerMasterCount(string name, int count, Action<bool> success = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("verify", server_verifyToken);
        form.AddField("server_name", name);
        form.AddField("server_players", count);
        form.AddField("recent", TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "Pacific Standard Time").ToString());
        form.AddField("action", "count");
        UnityWebRequest web = UnityWebRequest.Post(Host + "/" + serversFile, form);
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
    #endregion

    #region Server : Set Server Recent
    public void ServerListUpdateRecent(string serverIp, ushort serverPort, Action<bool> onRequestFinished)
    {
        StartCoroutine(WebServerMasterRecent(serverIp, serverPort, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    private IEnumerator WebServerMasterRecent(string serverIp, ushort serverPort, Action<bool> success = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("verify", server_verifyToken);
        form.AddField("server_Ip", serverIp);
        form.AddField("server_Port", serverPort);
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
            success(false);
        }
    }
    #endregion

    #region Server : Set Client Stats
    public void SetClientStats(int userId, string authKey, int expAdd, int coinsAdd, float hoursAdd, string notifyData, string storeSet, int kills, int deaths, Action<bool> onRequestFinished)
    {
        StartCoroutine(WebServerSetStatistics(userId, authKey, expAdd, coinsAdd, notifyData, hoursAdd, storeSet, kills, deaths, returnValue =>
        {
            onRequestFinished(returnValue);
        }));
    }
    private IEnumerator WebServerSetStatistics(int userId, string authKey, int expAdd, int coinsAdd, string notifyData, float hoursAdd, string storeSet, int kills, int deaths, Action<bool> success = null)
    {
        WWWForm form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("authKey", authKey);
        form.AddField("verify", server_verifyToken);
        form.AddField("exp", expAdd);
        form.AddField("coins", coinsAdd);
        form.AddField("hours", hoursAdd.ToString());
        form.AddField("store", storeSet);
        form.AddField("notify", notifyData);
        form.AddField("kills", kills);
        form.AddField("deaths", deaths);
        form.AddField("action", "update");
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
    #endregion
#endif
    #endregion
}

[Serializable]
public struct Server
{
    public string server_name;
    public string server_description;
    public string server_map;
    public string server_mode;
    public string server_Ip;
    public ushort server_Port;
    public int server_players;
    public int server_maxPlayers;
    public int server_ping;

}

//WebServer - List Request
public struct ListRequestData 
{
    public bool successful;
    public int errorCode;
    public ServerListData servers;
}
public struct ServerListData 
{
    public Server[] list;
    public static ServerListData Generate(Server[] temp)
    {
        return new ServerListData()
        {
            list = temp
        };
    }
}


//WebServer - Login Request
public struct LoginRequestData
{
    public bool successful;
    public bool needsToSignup;
    public int errorCode;
    public UserCredentialData credentials;
}
public struct UserCredentialData 
{
    public string user_name;
    public string user_token;
    public string user_authkey;
    public int user_id;
    
    public void Set(string name, string token, string authkey, int id) 
    {
        user_name = name;
        user_token = token;
        user_authkey = authkey;
        user_id = id;
    }
}


//WebServer - Stats Request
public struct StatRequestData 
{
    public bool successful;
    public int errorCode;
    public UserStatsData stats;
}
public struct UserStatsData 
{
    public string store_data;
    public string notify_data;
    public int exp;
    public int coins;
    public int kills;
    public int deaths;
    public float hours;
    public float percentile;

    public static UserStatsData Generate(string webData) 
    {
        string[] main_data = webData.Split('=');
        string s_exp = main_data[3] == "" ? "0" : main_data[3];
        string s_coins = main_data[4] == "" ? "0" : main_data[4];
        string s_hours = main_data[5] == "" ? "0" : main_data[5];
        string s_kills = main_data[6] == "" ? "0" : main_data[6];
        string s_deaths = main_data[7] == "" ? "0" : main_data[7];
        string s_percentile = main_data[8] == "" ? "0" : main_data[8];

        return new UserStatsData()
        {
            store_data = main_data[1],
            notify_data = main_data[2],
            exp = Convert.ToInt32(s_exp),
            coins = Convert.ToInt32(s_coins),
            kills = Convert.ToInt32(s_kills),
            deaths = Convert.ToInt32(s_deaths),
            hours = float.Parse(s_hours),
            percentile = float.Parse(s_percentile)
        };
    }
} 
