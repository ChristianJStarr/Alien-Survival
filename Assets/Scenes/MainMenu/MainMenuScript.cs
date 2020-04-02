using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    public PlayerStats playerStats;

    public GameObject mainScreen;
    public GameObject loadScreen;
    public GameObject profileMenu;
    public GameObject settingsMenu;
    public GameObject onlineMenu;
    public GameObject createMenu;
    public Slider loadSlider;
    public PhotonRoom photonRoom;
    public Camera cam;
    static Transform camReset;

    public Vector3 playTargetCord;
    public Quaternion playTargetRot;
    public Vector3 profTargetCord;
    public Quaternion profTargetRot;
    
    Vector3 resetTargetCord;
    Quaternion resetTargetRot;
    Vector3 camTargetposition;
    Quaternion camTargetrotation;
    
    void Start() 
    {
        camReset = cam.transform;
        resetTargetCord = camReset.position;
        resetTargetRot = camReset.localRotation;
        camTargetposition = resetTargetCord;
        camTargetrotation = resetTargetRot;
        photonRoom = FindObjectOfType<PhotonRoom>();
    }
    void Update()
    {
        float step = 15 * Time.deltaTime;
        float step2 = 15 * Time.deltaTime;
        if (cam.transform.position != camTargetposition) 
        {
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, camTargetposition,step);
        }
        if (cam.transform.localRotation.y != camTargetrotation.y)
        {
            cam.transform.localRotation = Quaternion.RotateTowards(cam.transform.localRotation, camTargetrotation, step2);
        }
    }
    public void PlayMenu() 
    {
        mainScreen.SetActive(false);
        onlineMenu.SetActive(true);
        camTargetposition = playTargetCord;
        camTargetrotation = playTargetRot;
        
    }
    public void CreateMenu()
    {
        mainScreen.SetActive(false);
        createMenu.SetActive(true);
        camTargetposition = playTargetCord;
        camTargetrotation = playTargetRot;
            
    }
    public void ProfileMenu() 
    {
        mainScreen.SetActive(false);
        profileMenu.SetActive(true);
        camTargetposition = profTargetCord;
        camTargetrotation = profTargetRot;
    }
    public void SettingsMenu() 
    {
        mainScreen.SetActive(false);
        settingsMenu.SetActive(true);
        camTargetposition = profTargetCord;
        camTargetrotation = profTargetRot;
    }
    public void CloseMenu() 
    {

            onlineMenu.SetActive(false);

            profileMenu.SetActive(false);
  
            settingsMenu.SetActive(false);

            createMenu.SetActive(false);
            
            mainScreen.SetActive(true);

        camTargetposition = resetTargetCord;
        camTargetrotation = resetTargetRot;
    }

    public void LoadGame() 
    {
        mainScreen.SetActive(false);
        onlineMenu.SetActive(false);
        profileMenu.SetActive(false);
        settingsMenu.SetActive(false);
        createMenu.SetActive(false);
        loadScreen.SetActive(true);
        StartCoroutine(LoadRoutine());
    }
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

    public void LogOut() 
    {
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("password");
        PlayerPrefs.DeleteKey("guest-a");
        PlayerPrefs.DeleteKey("authKey");
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.DeleteKey("guest-b");
        PlayerPrefs.DeleteKey("coins");
        PlayerPrefs.DeleteKey("exp");
        PlayerPrefs.DeleteKey("hours");
        mainScreen.SetActive(false);
        onlineMenu.SetActive(false);
        profileMenu.SetActive(false);
        settingsMenu.SetActive(false);
        createMenu.SetActive(false);
        loadScreen.SetActive(true);
        photonRoom.LeaveGame();
        StartCoroutine(LogOutRoutine());
    }
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
