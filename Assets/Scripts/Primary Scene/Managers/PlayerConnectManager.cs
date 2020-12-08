using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayerConnectManager : MonoBehaviour
{
    public GameObject cutsceneObject, videoScreen;
    public Image blackScreen;
    public VideoPlayer video;

    private float lerpSpeed = 5;
    public bool playerIsReady = false;
    private bool lerp = false;
    private bool fadeBack = false;
    private bool showCutscene = false;
    private bool callbackReceived = false;
    private bool targetTransparency = true;


    private void Start() 
    {
        cutsceneObject.SetActive(true);
        blackScreen.gameObject.SetActive(true);
        videoScreen.SetActive(true);
        InterfaceHider.Singleton.HideAllInterfaces();
        playerIsReady = true;
        if (callbackReceived) 
        {
            ConnectCallback(showCutscene);
        }
    }

    //Change Black Screen
    private void ChangeBlackScreen(bool active, bool _fadeBack = false) 
    {
        targetTransparency = active;
        lerp = true;
        fadeBack = _fadeBack;
    }

    //Color Lerping
    private void Update() 
    {
        if (lerp)
        {
            if(targetTransparency) 
            {
                if (blackScreen.color.a < .99F)
                {
                    Color color = new Color32(17, 17, 17, 255);
                    color.a = Mathf.Lerp(blackScreen.color.a, 1, lerpSpeed * Time.deltaTime);
                    blackScreen.color = color;
                }
                else if (fadeBack) 
                {
                    videoScreen.SetActive(false);
                    InterfaceHider.Singleton.ShowAllInterfaces();
                    targetTransparency = false;
                }
                else 
                {
                    blackScreen.color = new Color32(17, 17, 17, 255);
                    lerp = false;
                }
            }
            else if(!targetTransparency) 
            {
                if(blackScreen.color.a > .01F) 
                {
                    Color color = new Color32(17, 17, 17, 255);
                    color.a = Mathf.Lerp(blackScreen.color.a, 0, lerpSpeed * Time.deltaTime);
                    blackScreen.color = color;
                }
                else if(fadeBack)
                {
                    cutsceneObject.SetActive(false);
                    enabled = false;
                }
                else 
                {
                    blackScreen.color = new Color32(17, 17, 17, 0);
                    lerp = false;
                }
            }
        }
    }

    //Callback From Connecting
    public void ConnectCallback(bool cutscene) 
    {
        if (!playerIsReady) 
        {
            showCutscene = cutscene;
            callbackReceived = true;
            return;
        }
        if (cutscene) 
        {
            ChangeBlackScreen(false);
            video.Play();
            StartCoroutine(WaitForVideoToFinish(video.length));
        }
        else 
        {
            ChangeBlackScreen(true, true);
        }
    }

    //Wait for Video to Finish
    private IEnumerator WaitForVideoToFinish(double seconds) 
    {
        float video_length = (float) seconds;
        video_length -= 1;
        yield return new WaitForSeconds(video_length);
        ChangeBlackScreen(true, true);
    }
}
