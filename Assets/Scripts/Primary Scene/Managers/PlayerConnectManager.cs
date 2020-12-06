using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class PlayerConnectManager : MonoBehaviour
{
    public GameObject blackScreen, videoScreen;
    public VideoPlayer video;
    public bool playerIsReady = false;


    private void Start() 
    {
        blackScreen.SetActive(false);
        InterfaceHider.Singleton.HideAllInterfaces();
    }

    public void ConnectCallback(bool cutscene) 
    {
        playerIsReady = true;
        if (cutscene) 
        {
            videoScreen.SetActive(true);
            video.Play();
            StartCoroutine(WaitForVideoToFinish(video.clip.length));
        }
        else 
        {
            blackScreen.SetActive(false);
        }
    }

    private IEnumerator WaitForVideoToFinish(double time) 
    {
        yield return new WaitForSeconds((float) time);
        blackScreen.SetActive(false);
        videoScreen.SetActive(false);
        InterfaceHider.Singleton.ShowAllInterfaces();

    }

    public void CutSceneFinished() 
    {
    }
}
