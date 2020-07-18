using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// Main Menu Server List Handler. 
/// </summary>
public class MainMenuServerList : MonoBehaviour
{

    public TMP_InputField searchField;
    public TMP_Dropdown sortDropdown;
    private WebServer webServer;
    public TextMeshProUGUI serverCount;
    public Transform listContainer;
    public GameObject serverItem;
    public RectTransform refreshing;
    private List<MainMenuServerSlide> slides;
    private TouchScreenKeyboard mobileKeys;

    private string searchString = "";
    private int sortValue = 0;
    private int pings = 0;
    private bool isRefreshing = false;
    private bool isDemoLoad = false;

    private Vector2 refreshOriginal;
    private Vector2 bottomTarget;
    private RectTransform serverCountRect;
    
    
    
    private void Start()
    {
        serverCountRect = serverCount.GetComponent<RectTransform>();
        refreshOriginal = serverCountRect.anchoredPosition;
        sortDropdown.onValueChanged.AddListener(delegate { SortUpdated(sortDropdown); });
        slides = new List<MainMenuServerSlide>();
        webServer = GetComponent<WebServer>();
        serverCount.text = "SERVERS (0/0)";
        GetServers();
    }
    private void Update()
    {
        if(mobileKeys != null && mobileKeys.status == TouchScreenKeyboard.Status.Done) 
        {
            ToggleVisibility();
        }

        if((pings > 0 || isRefreshing || isDemoLoad) && !refreshing.gameObject.activeSelf) 
        {
            bottomTarget = new Vector3(-257, -29);
        }
        else if(!isDemoLoad && pings == 0 && !isRefreshing && refreshing.gameObject.activeSelf) 
        {
            bottomTarget = refreshOriginal;
        }

        if (refreshing.gameObject.activeSelf) 
        {
            refreshing.Rotate(new Vector3(0, 0, -5));
        }

        if(serverCountRect.anchoredPosition != bottomTarget) 
        {
            serverCountRect.anchoredPosition = Vector2.MoveTowards(serverCountRect.anchoredPosition, bottomTarget, 160 * Time.deltaTime);
            if (refreshing.gameObject.activeSelf) 
            {
                refreshing.gameObject.SetActive(false);
            }
        }
        else 
        {
            if(bottomTarget != refreshOriginal) 
            {
                refreshing.gameObject.SetActive(true);
            }
        }

    }


    //Search Functions
    public void SearchUpdated() 
    {
        if (searchField != null) 
        {
            searchString = searchField.text.ToLower();
            if(searchString.Length == 0) 
            {
                ToggleVisibility();
            }
        }
    }
   
    public void SearchSubmit() 
    {
        ToggleVisibility();
    }

    public void SearchSelected() 
    {
        mobileKeys = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, true);
    }

    //List Functions
    public void GetServers() 
    {
        if (!isDemoLoad) 
        {
            isRefreshing = true;
            isDemoLoad = true;
            StartCoroutine(DemoLoadDelay());
            webServer.ServerListRequest(onRequestFinished =>
            {
                if (onRequestFinished != null)
                {
                    isRefreshing = false;
                    UpdateList(onRequestFinished.servers);
                }
                else
                {
                    isRefreshing = false;
                    serverCount.text = "SERVERS (0/0)";
                    for (int i = 0; i < slides.Count; i++)
                    {
                        Destroy(slides[i].gameObject);
                    }
                    slides.Clear();
                }
            });
        }
    }

    private void UpdateList(Server[] servers) 
    {
        int ct = servers.Length;
        serverCount.text = "SERVERS (" + ct + "/" + ct + ")";
        for (int i = 0; i < slides.Count; i++)
        {
            Destroy(slides[i].gameObject);
        }
        slides.Clear();
        foreach (Server server in servers)
        {
            Validate(server);
        }
        ToggleVisibility();
    }

    private void ToggleVisibility() 
    {
        bool searching = searchString.Length > 0;
        int activeCount = 0;
        foreach (MainMenuServerSlide slide in slides)
        {
            if (searching) 
            {
                if (slide.storedServer.name.ToLower().Contains(searchString) || slide.storedServer.mode.ToLower().Contains(searchString) || slide.storedServer.map.ToLower().Contains(searchString))
                {
                    slide.gameObject.SetActive(true);
                    activeCount++;
                }
                else
                {
                    slide.gameObject.SetActive(false);
                }
            }
            else 
            {
                slide.gameObject.SetActive(true);
                activeCount++;
            }
        }
        serverCount.text = "SERVERS (" + activeCount + "/" + slides.Count + ")";
        SortSlides();
    }


    //Sort Functions
    private void SortSlides()
    {
        //Player Count High / Low
        if (sortValue == 0) 
        {
            slides = slides.OrderByDescending(o => o.storedServer.player).ToList();
        }
        //Player Count Low / High
        else if (sortValue == 1) 
        {
            slides = slides.OrderBy(o => o.storedServer.player).ToList();
        }
        //Ping Count High / Low
        else if (sortValue == 2) 
        {
            slides = slides.OrderByDescending(o => o.storedServer.ping).ToList();
        }
        //Ping Count Low / High
        else if (sortValue == 3) 
        {
            slides = slides.OrderBy(o => o.storedServer.ping).ToList();
        }
        for (int i = 0; i < slides.Count; i++)
        {
            slides[i].gameObject.transform.SetSiblingIndex(i);
        }
    }

    public void SortUpdated(TMP_Dropdown change) 
    {
        sortValue = change.value;
        ToggleVisibility();
    }

    private void Validate(Server server) 
    {
        StartCoroutine(StartPing(server));
    }
   
    private IEnumerator StartPing(Server server) 
    {
        pings++;
        bool offline = false;
        int count = 0;
        WaitForSeconds f = new WaitForSeconds(0.05F);
        Ping ping = new Ping(server.serverIP);
        while (!ping.isDone ) 
        {
            if (count >= 10) 
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
            slides.Add(slide);
            ToggleVisibility();
        }
        pings--;
    }

    private IEnumerator DemoLoadDelay() 
    {
        yield return new WaitForSeconds(4f);
        isDemoLoad = false;
    }
}
        