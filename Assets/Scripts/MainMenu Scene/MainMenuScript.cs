using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    public Animator alienAnimator; //Animator of Alien Model
    public PlayerStats playerStats; //Stored player data.
    public GameObject mainScreen; //Main Screen.
    public GameObject loadScreen; //Loading Screen.
    public GameObject profileMenu; //Profile Screen.
    public GameObject settingsMenu; //Settings Screen.
    public GameObject onlineMenu; //Server Screen.
    public GameObject alienStore; // Alien Store
    public Slider loadSlider; //Loading Screen slider.
    public Camera cam; //Scene Camera.
    public GameObject easterEggBeam;
    public TextMeshProUGUI loadTip, loadMainText;
    private TouchPhase touchPhase = TouchPhase.Ended;//Touch Phase


    void Update()
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
                    if (hit.collider != null)
                    {
                        alienAnimator.SetTrigger("EasterEgg");
                        StartCoroutine(EasterEggStart());
                    }
                }
            }
        }
    }

    //Toggle the Loading Screen
    public void LoadingScreen(bool value) 
    {
        if (value) 
        {
            LoadGame();
        }
        else 
        {
           CloseMenu();
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
        StartLoadTip();
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
        loadMainText.text = "Logging Out Account";
        StartLoadTip();
        StartCoroutine(LogOutRoutine());
    }

    //Register Guest Account
    public void RegisterAccount() 
    {
        //TODO: Make this shit work. 
    }
    

    //Load Routine
    private IEnumerator LoadRoutine()
    {
        int loadProgress = 0;
        int lastLoadProgress = 0;
        for (loadProgress = 0; loadProgress < 500; loadProgress++)
        {
            lastLoadProgress = loadProgress;
            loadProgress++;
            if (lastLoadProgress != loadProgress) { lastLoadProgress = loadProgress; loadSlider.value = loadProgress / 5; }
            yield return null;
        }

        loadProgress = 100;
        loadSlider.value = loadProgress;
    }
     
    //Log Out Routine
    private IEnumerator LogOutRoutine()
    {
        int loadProgress = 0;
        int lastLoadProgress = 0;
        AsyncOperation op = SceneManager.LoadSceneAsync(0);
        op.allowSceneActivation = false;
        while (!op.isDone)
        {
            if (op.progress < 0.9f)
            {
                loadProgress = (int)(op.progress * 100f);
            }
            else
            {
                loadProgress = 100;
                op.allowSceneActivation = true;
            }
            if (lastLoadProgress != loadProgress) { lastLoadProgress = loadProgress; loadSlider.value = loadProgress; }
            yield return null;
        }
        loadProgress = 100;
        loadSlider.value = loadProgress;
    }
    
    //Easter Egg Start
    private IEnumerator EasterEggStart() 
    {
        yield return new WaitForSeconds(5f);
        easterEggBeam.SetActive(true);
    }

    //Get Load Screen Tip Text

    private string[] loadingTips;
    private int loadingTipIndex;

    private void StartLoadTip() 
    {
        string json = (Resources.Load("loading-tips") as TextAsset).text;
        loadingTips = JsonHelper.FromJson<string>(json);
        loadingTipIndex = Random.Range(0, loadingTips.Length - 1);
        loadTip.text = loadingTips[loadingTipIndex];
        StartCoroutine(LoadTipWait());
    }

    private IEnumerator LoadTipWait()
    {
        yield return new WaitForSeconds(6);
        if (loadingTipIndex + 1 >= loadingTips.Length) 
        {
            loadingTipIndex = 0;
        }
        else 
        {
            loadingTipIndex++;
        }
        loadTip.text = loadingTips[loadingTipIndex];
        StartCoroutine(LoadTipWait());
    }

}
