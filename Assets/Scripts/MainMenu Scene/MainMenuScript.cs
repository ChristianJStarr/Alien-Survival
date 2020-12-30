using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public Animator alienAnimator; //Animator of Alien Model
    public WebServer webServer;
    public MainMenuStatUpdater statUpdater;
    public Transform alienCenterMass;
    public PlayerStats playerStats; //Stored player data.
    public GameObject mainScreen, loadScreen, profileMenu, settingsMenu, onlineMenu, alienStore, connectError, requestingStats;
    public Camera cam; //Scene Camera.
    public GameObject easterEggBeam;
    public TextMeshProUGUI loadTip, loadMainText;
    private TouchPhase touchPhase = TouchPhase.Ended;//Touch Phase
    public bool enableEasterEgg = false;
    private int statRequests = 0;

    public void TestEgg() 
    {
        alienAnimator.enabled = false;
        StartCoroutine(EasterEggStart());
    }

    private void Start() 
    {
        RequestStats();
    }

    private void Update()
    {
        if (enableEasterEgg) 
        {
            DetectEasterEggInput();
        }
    }

    private void DetectEasterEggInput() 
    {
        //Detect double click for easter egg.
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == touchPhase && Input.GetTouch(0).tapCount == 2)
        {
            if (mainScreen.activeSelf)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider != null && hit.collider.CompareTag("Player"))
                    {
                        alienAnimator.enabled = false;
                        StartCoroutine(EasterEggStart());
                    }
                }
            }
        }
    }

    //Open Server Menu
    public void PlayMenu() 
    {
        mainScreen.SetActive(false);
        onlineMenu.SetActive(true);
    }
    
    //Open Profile Menu
    public void ProfileMenu() 
    {
        mainScreen.SetActive(false);
        profileMenu.SetActive(true);
    }
    
    //Open Settings Menu
    public void SettingsMenu() 
    {
        mainScreen.SetActive(false);
        settingsMenu.SetActive(true);
    }
    
    //Close all Menus
    public void CloseMenu() 
    {
        onlineMenu.SetActive(false);
        profileMenu.SetActive(false);
        loadScreen.SetActive(false);
        settingsMenu.SetActive(false);
        alienStore.SetActive(false);
        connectError.SetActive(false);
        mainScreen.SetActive(true);
    }

    //Open Alien Store
    public void AlienStore()
    {
        mainScreen.SetActive(false);
        alienStore.SetActive(true);
    }

    //Close All
    public void CloseAll()
    {
        onlineMenu.SetActive(false);
        profileMenu.SetActive(false);
        loadScreen.SetActive(false);
        settingsMenu.SetActive(false);
        alienStore.SetActive(false);
        connectError.SetActive(false);
        mainScreen.SetActive(false);
    }

    //Load the Game Function
    public void LoadGame() 
    {
        mainScreen.SetActive(false);
        onlineMenu.SetActive(false);
        profileMenu.SetActive(false);
        settingsMenu.SetActive(false);
        loadScreen.SetActive(true);
    }

    //Log Out Function
    public void LogOut()
    {
        //Remove Stored PlayerPrefs
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("password");
        PlayerPrefs.DeleteKey("guest-a");
        PlayerPrefs.DeleteKey("guest-b");
        PlayerPrefs.DeleteKey("authKey");
        PlayerPrefs.DeleteKey("userId");
        mainScreen.SetActive(false);
        onlineMenu.SetActive(false);
        profileMenu.SetActive(false);
        settingsMenu.SetActive(false);
        loadScreen.SetActive(true);
        LangDataSingle data = MultiLangSystem.GetLangDataFromKey("loggingout");
        if(data != null) 
        {
            loadMainText.text = data.text;
            if(data.fontSize != 0) 
            {
                loadMainText.fontSize = data.fontSize;
            }
        }
        else 
        {
            loadMainText.text = "Logging Out Account";
        }
        SceneManager.LoadSceneAsync(0);
    }

    //Connecting Failed
    public void ConnectingFailed() 
    {
        loadScreen.SetActive(false);
        onlineMenu.SetActive(true);
        connectError.SetActive(true);
    }

    //Clost Connection Error Msg
    public void CloseConnectError() 
    {
        connectError.SetActive(false);
    }

    //Register Guest Account
    public void RegisterAccount() 
    {
        //TODO: Make this shit work. 
    }

    //Request Player Stats
    public void RequestStats()
    {
        statRequests++;
        int userId = PlayerPrefs.GetInt("userId");
        string authKey = PlayerPrefs.GetString("authKey");
        requestingStats.SetActive(true);
        webServer.StatRequest(userId, authKey, requestData =>
        {
            if (requestData.successful) 
            {
                requestingStats.SetActive(false);
                playerStats.Align(requestData);
                statRequests = 0;
                statUpdater.UpdateText();
            }
            else if(statRequests < 5)
            {
                StartCoroutine(DelayStatRequest());
            }
            else 
            {
                LogOut();
            }
        });
    }

    //Delayed Stat Request
    private IEnumerator DelayStatRequest() 
    {
        yield return new WaitForSeconds(5);
        RequestStats();
    }
    
    //Easter Egg Start
    private IEnumerator EasterEggStart() 
    {
        yield return new WaitForSeconds(5f);
        easterEggBeam.transform.position = alienCenterMass.position + (Vector3.up * 9);
        easterEggBeam.SetActive(true);
    }

}
