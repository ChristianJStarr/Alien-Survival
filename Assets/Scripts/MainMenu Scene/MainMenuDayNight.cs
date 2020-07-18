
using UnityEngine;

public class MainMenuDayNight : MonoBehaviour
{
    public GameObject extraFireLight;
    public Light directionalLight;
    public Light shackLight;
    public Light shackLight2;
    public GameObject shackRays;

    private void Start()
    {
        if(PlayerPrefs.GetInt("nightChance") > 15)
        {

            //Set to Day
        }
        else 
        {
            //Set to Night
            shackLight.enabled = true;
            shackLight2.enabled = true;
            directionalLight.enabled = false;
            shackRays.SetActive(true);
            extraFireLight.SetActive(true);
            RenderSettings.fogColor = new Color32(75, 57, 66, 255);
            Camera.main.backgroundColor = new Color32(91, 91, 91, 255);
        }
    }
}
