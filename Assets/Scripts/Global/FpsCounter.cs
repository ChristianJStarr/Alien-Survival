using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    public float refresh = 20;
    private float timer, avgFramerate;
    public TextMeshProUGUI countText;
    public Settings settings;
    public GameObject toggleObject;
    private bool showFps = false;
    

    private void Start()
    {
        Change(); //Update at Startup
    }
    
    
     void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;//Subscribe to Settings Change Event.
    }
    
    void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;//unSubscribe to Settings Change Event.
    }

    //Change Settings.
    private void Change() 
    {
        Debug.Log("Changed Settings :)");
        showFps = settings.showFps;
        toggleObject.SetActive(showFps);
    }
    //Render FPS
    private void Update()
    {
        if (showFps) 
        {
            if (Time.unscaledTime > timer)
            {
                avgFramerate += ((Time.deltaTime / Time.timeScale) - avgFramerate) * 0.03f;
                countText.text = ((int)(1F / avgFramerate)) + " FPS";
                timer = Time.unscaledTime + refresh;
            } 
        }
    }
}