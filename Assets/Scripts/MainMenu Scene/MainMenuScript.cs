using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Main Menu Scene. Primary Script
/// </summary>
public class MainMenuScript : MonoBehaviour
{
    //What: Main Menu Scene Controller. 
    //Where: MainMenu Scene / UI
    public PlayerStats playerStats; //Stored player data.
    public GameObject mainScreen; //Main Screen.
    public GameObject loadScreen; //Loading Screen.
    public GameObject profileMenu; //Profile Screen.
    public GameObject settingsMenu; //Settings Screen.
    public GameObject onlineMenu; //Server Screen.
    public Slider loadSlider; //Loading Screen slider.
    public Camera cam; //Scene Camera.
    static Transform camReset; //Camera default positon to reset to.
    //Camera locations for switching screens effect.
    public Vector3 playTargetCord;
    public Quaternion playTargetRot;
    public Vector3 profTargetCord;
    public Quaternion profTargetRot;
    //Defaults for above positions and rotations.
    private Vector3 resetTargetCord;
    private Quaternion resetTargetRot;
    private Vector3 camTargetposition;
    private Quaternion camTargetrotation;
    /// <summary>
    /// Main Menu Start Function. Set camera transforms.
    /// </summary>
    void Start() 
    {
        camReset = cam.transform;
        resetTargetCord = camReset.position;
        resetTargetRot = camReset.localRotation;
        camTargetposition = resetTargetCord;
        camTargetrotation = resetTargetRot;
    }
    /// <summary>
    /// Main Menu Update Function. Keep up with camera targets.
    /// </summary>
    void Update()
    {
        float step = 15 * Time.deltaTime;
        if (cam.transform.position != camTargetposition) 
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, camTargetposition,step);
        }
        if (cam.transform.localRotation.y != camTargetrotation.y)
        {
            cam.transform.localRotation = Quaternion.RotateTowards(cam.transform.localRotation, camTargetrotation, step);
        }
    }
    /// <summary>
    /// Activate Menu: Play Menu
    /// </summary>
    public void PlayMenu() 
    {
        mainScreen.SetActive(false);
        onlineMenu.SetActive(true);
        camTargetposition = playTargetCord;
        camTargetrotation = playTargetRot;
        
    }
    /// <summary>
    /// Activate Menu: Profile Menu
    /// </summary>
    public void ProfileMenu() 
    {
        mainScreen.SetActive(false);
        profileMenu.SetActive(true);
        camTargetposition = profTargetCord;
        camTargetrotation = profTargetRot;
    }
    /// <summary>
    /// Activate Menu: Settings Menu
    /// </summary>
    public void SettingsMenu() 
    {
        mainScreen.SetActive(false);
        settingsMenu.SetActive(true);
        camTargetposition = profTargetCord;
        camTargetrotation = profTargetRot;
    }
    /// <summary>
    /// Close Menus. Display Main Menu.
    /// </summary>
    public void CloseMenu() 
    {
        onlineMenu.SetActive(false);
        profileMenu.SetActive(false);
        settingsMenu.SetActive(false); 
        mainScreen.SetActive(true);
        camTargetposition = resetTargetCord;
        camTargetrotation = resetTargetRot;
    }
    /// <summary>
    /// Show loading screen. Load the Primary Scene
    /// </summary>
    public void LoadGame() 
    {
        mainScreen.SetActive(false);
        onlineMenu.SetActive(false);
        profileMenu.SetActive(false);
        settingsMenu.SetActive(false);
        loadScreen.SetActive(true);
        camTargetposition = resetTargetCord;
        camTargetrotation = resetTargetRot;
        StartCoroutine(LoadRoutine());
    }
    /// <summary>
    /// Preform the load routine. 
    /// </summary>
    /// <returns>Coroutine</returns>
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
    /// <summary>
    /// Log Player Out. Return to Load Scene.
    /// </summary>
    public void LogOut() 
    {
        //Remove Stored PlayerPrefs
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("password");
        PlayerPrefs.DeleteKey("guest-a");
        PlayerPrefs.DeleteKey("guest-b");
        PlayerPrefs.DeleteKey("authKey");
        PlayerPrefs.DeleteKey("userId");
        playerStats.Wipe();
        mainScreen.SetActive(false);
        onlineMenu.SetActive(false);
        profileMenu.SetActive(false);
        settingsMenu.SetActive(false);
        loadScreen.SetActive(true);
        StartCoroutine(LogOutRoutine());
    }
    /// <summary>
    /// Preform the log out routine.
    /// </summary>
    /// <returns>Coroutine</returns>
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
}
