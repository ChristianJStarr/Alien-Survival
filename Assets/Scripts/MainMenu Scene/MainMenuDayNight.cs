
using UnityEngine;

public class MainMenuDayNight : MonoBehaviour
{
    public GameObject extraFireLight;
    public Light directionalLight;
    public Light shackLight;

    private void Start()
    {
        if(PlayerPrefs.GetInt("nightChance") > 15)
        {

            //Set to Day
        }
        else 
        {
            shackLight.enabled = true;
            directionalLight.enabled = false;
            //Set to Night
            extraFireLight.SetActive(true);
            RenderSettings.fogColor = new Color32(75, 57, 66, 255);
            Camera.main.backgroundColor = new Color32(91, 91, 91, 255);
        }
    }
}
