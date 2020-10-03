
using UnityEngine;

public class MainMenuDayNight : MonoBehaviour
{
    public GameObject extraFireLight;
    public Light directionalLight;
    public Light shackLight;
    public Light shackLight2;
    public GameObject shackRays;

    public bool forceNight = false;

    private void Start()
    {
        if (forceNight) 
        {
            SetToNight();
        }
        else if(PlayerPrefs.GetInt("nightChance") > 15)
        {
            SetToDay();
        }
        else 
        {
            SetToNight();
        }
    }

    //Set to Day
    private void SetToDay() 
    {
        shackLight.enabled = false;
        shackLight2.enabled = false;
        shackRays.SetActive(false);
        directionalLight.enabled = true;
        extraFireLight.SetActive(false);
        RenderSettings.fogColor = new Color32(75, 57, 66, 255);
        Camera.main.backgroundColor = new Color32(91, 91, 91, 255);
    }


    //Set to Night
    private void SetToNight() 
    {
        shackLight.enabled = true;
        shackLight2.enabled = true;
        directionalLight.enabled = false;
        shackRays.SetActive(true);
        extraFireLight.SetActive(true);
        RenderSettings.fogColor = new Color32(75, 57, 66, 255);
        Camera.main.backgroundColor = new Color32(91, 91, 91, 255);
    }
}
