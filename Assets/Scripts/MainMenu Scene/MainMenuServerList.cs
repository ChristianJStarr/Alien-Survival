using UnityEngine;
using TMPro;
using System.Collections;
/// <summary>
/// Main Menu Server List Handler. 
/// </summary>
public class MainMenuServerList : MonoBehaviour
{
    /// <summary>
    /// Web Server in use.
    /// </summary>
    private WebServer webServer;
    /// <summary>
    /// Server Count on server screen.
    /// </summary>
    public TextMeshProUGUI serverCount;
    /// <summary>
    /// Container for server slides.
    /// </summary>
    public Transform listContainer;
    /// <summary>
    /// Prefab Object of server slide.
    /// </summary>
    public GameObject serverItem;
    /// <summary>
    /// Get Server List on Start
    /// </summary>
    private void Start()
    {
        webServer = GetComponent<WebServer>();
        serverCount.text = "(0/0) SERVERS";
        GetServers();
    }
    /// <summary>
    /// Get active server list. If successfull, UpdateList(Server[] servers) is called.
    /// </summary>
    public void GetServers() 
    {
        webServer.ServerListRequest(onRequestFinished =>
        {
            if (onRequestFinished != null) 
            {
                UpdateList(onRequestFinished.servers);
            }
            else 
            {
                Debug.Log("Network - Web - Unable to get server list.");
            }
        });
    }
    /// <summary>
    /// Update the UI slides to active server list.
    /// </summary>
    /// <param name="servers">Server Array</param>
    private void UpdateList(Server[] servers) 
    {
        int ct = servers.Length;
        serverCount.text = "(" + ct + "/" + ct + ") SERVERS";
        foreach (MainMenuServerSlide slide in listContainer.GetComponentsInChildren<MainMenuServerSlide>()) 
        {
            Destroy(slide.gameObject);
        }
        foreach (Server server in servers)
        {
            Validate(server);
        }
    }

    private void Validate(Server server) 
    {
        StartCoroutine(StartPing(server));
    }
    private IEnumerator StartPing(Server server) 
    {
        bool offline = false;
        int count = 0;
        WaitForSeconds f = new WaitForSeconds(0.05F);
        Ping ping = new Ping(server.serverIP);
        while (!ping.isDone ) 
        {
            if(count >= 10) 
            {
                offline = true;
                break;
            }
            count++;
            yield return f;
            
        }
        
        if(!offline) 
        {
            server.ping = ping.time;
            MainMenuServerSlide slide = Instantiate(serverItem, listContainer).GetComponent<MainMenuServerSlide>();
            slide.RefreshValues(server);
        }
    }
}
        