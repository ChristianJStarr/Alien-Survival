using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public GameObject mainScreen;
    public GameObject loadScreen;
    public GameObject profileMenu;
    public GameObject settingsMenu;
    public GameObject onlineMenu;
    public Camera cam;
    static Transform camReset;
    static Transform playTarget;
    public Vector3 playTargetCord;
    public Quaternion playTargetRot;
    
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
    }
    void Update()
    {
        float step = 15 * Time.deltaTime;
        float step2 = 15 * Time.deltaTime;
        if (cam.transform.position != camTargetposition) 
        {
            Debug.Log("2");
            cam.transform.position = Vector3.MoveTowards(cam.transform.position, camTargetposition,step);
        }
        if (cam.transform.localRotation.y != camTargetrotation.y)
        {
            cam.transform.localRotation = Quaternion.RotateTowards(cam.transform.localRotation, camTargetrotation, step2);
            Debug.Log(cam.transform.localRotation + "   :   " + camTargetrotation);
        }
    }
    public void PlayMenu() 
    {
        mainScreen.SetActive(false);
        onlineMenu.SetActive(true);
        camTargetposition = playTargetCord;
        camTargetrotation = playTargetRot;
    }
    public void ProfileMenu() 
    {
        mainScreen.SetActive(false);
        profileMenu.SetActive(true);
        camTargetposition = playTargetCord;
        camTargetrotation = playTargetRot;
    }
    public void SettingsMenu() 
    {
        mainScreen.SetActive(false);
        settingsMenu.SetActive(true);
        camTargetposition = playTargetCord;
        camTargetrotation = playTargetRot;
    }
    public void CloseMenu() 
    {

            onlineMenu.SetActive(false);

            profileMenu.SetActive(false);
  
            settingsMenu.SetActive(false);

            mainScreen.SetActive(true);

        camTargetposition = resetTargetCord;
        camTargetrotation = resetTargetRot;
    }
    public void JoinServer()
    {
        SceneManager.LoadScene(1);
        loadScreen.SetActive(true);

    }
}
