using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class MainMenuServerList : MonoBehaviour
{
#if !UNITY_SERVER
    public TMP_InputField searchField;
    public TMP_Dropdown sortDropdown;
    public WebServer webServer;
    public TextMeshProUGUI serverCount;
    public Transform listContainer;
    public RectTransform serverCountParent;
    public GameObject serverItem, noServersAlert, refreshIcon;

    public Image hideFull_Button, hideEmpty_Button;
    public Color32 buttonToggleColor;
    private Color32 buttonRegularColor;


    private List<MainMenuServerSlide> server_slides = new List<MainMenuServerSlide>();
    private Queue<MainMenuServerSlide> offline_slides = new Queue<MainMenuServerSlide>();

    private string search_string = "";
    private int sortValue = 0;

    private bool hideEmpty = false;
    private bool hideFull = false;


    private void Start()
    {
        buttonRegularColor = hideFull_Button.color;
        sortDropdown.onValueChanged.AddListener(delegate { SortUpdated(sortDropdown); });
        Refresh();
    }

    public void HideEmpty()
    {
        hideEmpty = !hideEmpty;
        if (hideEmpty)
        {
            hideEmpty_Button.color = buttonToggleColor;
        }
        else
        {
            hideEmpty_Button.color = buttonRegularColor;
        }
        ToggleVisibility();
    }

    public void HideFull()
    {
        hideFull = !hideFull;
        if (hideFull)
        {
            hideFull_Button.color = buttonToggleColor;
        }
        else 
        {
            hideFull_Button.color = buttonRegularColor;
        }
        ToggleVisibility();
    }



    //Refresh Server List
    public void Refresh()
    {
        refreshIcon.SetActive(true);
        webServer.ServerListRequest(requestData =>
        {
            if (requestData.successful && requestData.servers.list != null)
            {
                noServersAlert.SetActive(false);
                StartCoroutine(Refresh_Task(requestData.servers.list));
            }
            else
            {
                for (int i = server_slides.Count - 1; i >= 0; i--)
                {
                    MainMenuServerSlide slide = server_slides[i];
                    slide.gameObject.SetActive(false);
                    offline_slides.Enqueue(slide);
                    server_slides.RemoveAt(i);
                }
                ChangeCountText(0, 0);
                noServersAlert.SetActive(true);
                refreshIcon.SetActive(false);
            }
        });
    }
    private IEnumerator Refresh_Task(Server[] servers)
    {
        int server_count = servers.Length;
        ChangeCountText(0, 0);
        for (int i = server_slides.Count - 1; i >= 0; i--)
        {
            MainMenuServerSlide slide = server_slides[i];
            slide.gameObject.SetActive(false);
            offline_slides.Enqueue(slide);
            server_slides.RemoveAt(i);
        }
        int online_inc = 0;
        for (int i = 0; i < server_count; i++)
        {
            bool offline = false;
            int count = 0;
            WaitForSeconds wait = new WaitForSeconds(0.05F);
            Ping ping = new Ping(servers[i].server_Ip);
            while (!ping.isDone)
            {
                if (count >= 10)
                {
                    offline = true;
                    break;
                }
                count++;
                yield return wait;
            }
            if (!offline)
            {
                online_inc++;
                MainMenuServerSlide slide = null;
                if (offline_slides.Count > 0)
                {
                    slide = offline_slides.Dequeue();
                    slide.gameObject.SetActive(true);
                }
                else
                {
                    slide = Instantiate(serverItem, listContainer).GetComponent<MainMenuServerSlide>();
                }
                slide.RefreshValues(servers[i]);
                server_slides.Add(slide);
                ChangeCountText(online_inc, online_inc);
            }
        }
        if (online_inc == 0)
        {
            ChangeCountText(0,0);
            noServersAlert.SetActive(true);
        }

        ToggleVisibility();
        yield return new WaitForSeconds(1);
        refreshIcon.SetActive(false);
    }

    //Search
    public void SearchUpdated()
    {
        search_string = searchField.text.ToLower();
        if (search_string.Length == 0)
        {
            ToggleVisibility();
        }
    }
    public void SearchSubmit()
    {
        ToggleVisibility();
    }
    
    //Sort Functions
    private void SortSlides()
    {
        //Player Count High / Low
        if (sortValue == 0)
        {
            server_slides = server_slides.OrderByDescending(o => o.storedServer.server_players).ToList();
        }
        //Player Count Low / High
        else if (sortValue == 1)
        {
            server_slides = server_slides.OrderBy(o => o.storedServer.server_players).ToList();
        }
        //Ping Count High / Low
        else if (sortValue == 2)
        {
            server_slides = server_slides.OrderByDescending(o => o.storedServer.server_ping).ToList();
        }
        //Ping Count Low / High
        else if (sortValue == 3)
        {
            server_slides = server_slides.OrderBy(o => o.storedServer.server_ping).ToList();
        }
        for (int i = 0; i < server_slides.Count; i++)
        {
            server_slides[i].gameObject.transform.SetSiblingIndex(i);
        }
    }
    public void SortUpdated(TMP_Dropdown change)
    {
        sortValue = change.value;
        ToggleVisibility();
    }


    private void ToggleVisibility()
    {
        bool searching = search_string.Length > 0;
        int activeCount = 0;
        int server_count = server_slides.Count;
        for (int i = 0; i < server_count; i++)
        {
            MainMenuServerSlide slide = server_slides[i];
            if (slide)
            {
                bool showSlide = true;

                if (searching && (slide.storedServer.server_name.ToLower().Contains(search_string) || slide.storedServer.server_mode.ToLower().Contains(search_string) || slide.storedServer.server_map.ToLower().Contains(search_string)))
                {
                    showSlide = true;
                }
                else if (searching) 
                {
                    showSlide = false;
                }

                if (hideFull && slide.storedServer.server_players == slide.storedServer.server_maxPlayers) 
                {
                    showSlide = false;
                }
                if (hideEmpty && slide.storedServer.server_players == 0)
                {
                    showSlide = false;
                }


                if (showSlide) 
                {
                    slide.gameObject.SetActive(true);
                    activeCount++;
                }
                else 
                {
                    slide.gameObject.SetActive(false);
                }
            }
        }
        ChangeCountText(activeCount, server_count);
        SortSlides();
    }

    private void ChangeCountText(int current, int max) 
    {
        serverCount.text = string.Format("SERVERS ({0}/{1})", current, max);
        LayoutRebuilder.ForceRebuildLayoutImmediate(serverCountParent);
    }


#endif
}
