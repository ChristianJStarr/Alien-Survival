using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlayerConnectManager : MonoBehaviour
{
    public GameObject cutsceneObject, videoScreen;
    public Image blackScreen;
    public VideoPlayer video;

    public bool playerIsReady = false;
    private bool showCutscene = false;
    private bool callbackReceived = false;


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

    //Change Black Screen
    private void ChangeBlackScreen(bool active, bool fadeback = false)
    {
        int opacity = 0;
        if (active) opacity = 1;
        StartCoroutine(LerpOpacity(opacity, 2, fadeback));
    }

    //Lerp BlackSreen Opacity
    private IEnumerator LerpOpacity(float opacity, float duration, bool fadeback) 
    {
        Color color = blackScreen.color;
        float time = 0;
        float startValue = color.a;
        while (time < duration)
        {
            color.a = Mathf.Lerp(startValue, opacity, time / duration);
            blackScreen.color = color;
            time += Time.deltaTime;
            yield return null;
        }
        color.a = opacity;
        blackScreen.color = color;
        if (fadeback) 
        {
            if (videoScreen.activeSelf) 
            {
                videoScreen.SetActive(false);
                InterfaceHider.Singleton.ShowAllInterfaces();
                StartCoroutine(LerpOpacity(0, duration, true));
            }
            else 
            {
                cutsceneObject.SetActive(false);
                enabled = false;
            }
        }
    }
}
