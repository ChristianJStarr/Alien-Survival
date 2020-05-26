using UnityEngine;
using TMPro;
/// <summary>
/// Main Menu Server List Handler. 
/// </summary>
public class MainMenuServerList : MonoBehaviour
{
    /// <summary>
    /// Web Server in use.
    /// </summary>
    public WebServer webServer;
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
            MainMenuServerSlide slide = Instantiate(serverItem, listContainer).GetComponent<MainMenuServerSlide>();
            slide.RefreshValues(server);
        }
    }
}
        