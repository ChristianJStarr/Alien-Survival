using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplySettings : MonoBehaviour
{

    public Settings settings;

    void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "LoadScene") //Load settings config before intensive scenes.
        {
            LoadSettings();
        }
    }

    public void LoadSettings() 
    {
        QualitySettings.SetQualityLevel(settings.quality, true);
    }

}
