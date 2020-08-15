using TMPro;
using UnityEngine;

public class FpsCounter : MonoBehaviour
{
    public GameObject fpsPanel;
    public float refresh = 20;
    private float timer, avgFramerate;
    public TextMeshProUGUI countText;
    public Settings settings;
    private bool showFps = false;
    

    private void Start()
    {
        showFps = settings.showFps; //Get showFps bool.
        fpsPanel.SetActive(showFps); //Activate/Deactivate fpsPanel
    }
    
    
    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;//Subscribe to Settings Change Event.
    }
    
    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;//unSubscribe to Settings Change Event.
    }

    //Change Settings.
    private void Change() 
    {
        showFps = settings.showFps;
        fpsPanel.SetActive(showFps);
    }
    //Render FPS
    private void Update()
    {
        if (showFps) 
        {
            if (countText == null) return;
            if (Time.unscaledTime > timer)
            {
                avgFramerate += ((Time.deltaTime / Time.timeScale) - avgFramerate) * 0.03f;
                countText.text = ((int)(1F / avgFramerate)) + " FPS";
                timer = Time.unscaledTime + refresh;
            } 
        }
    }
}