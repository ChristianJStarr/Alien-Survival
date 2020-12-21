using UnityEngine;


public class ApplySettings : MonoBehaviour
{
    public Settings settings;

    private void Start()
    {
        settings.Validate();
        SetQuality(settings.quality);
        SetFramerate(settings.quality);
    }

    //Handle Target FrameRate
    private void SetFramerate(int quality) 
    {
        int fps = 60;
        if(quality == 1) 
        {
            fps = 30;
        }
        else if(quality == 2) 
        {
            fps = 45;
        }
        Application.targetFrameRate = fps;
    }

    private void SetQuality(int quality) 
    {
        //Check if quality settings need to be changed.
        if (QualitySettings.GetQualityLevel() != settings.quality - 1)
        {
            //Change quality settings.
            QualitySettings.SetQualityLevel(settings.quality - 1, true);
        }
    }
}
