
using UnityEngine;

public class MainMenuDayNight : MonoBehaviour
{
    public GameObject extraFireLight;
    public Light directionalLight;
    public Material shipGlow;
    public MeshRenderer ship;

    private void Start()
    {
        if(PlayerPrefs.GetInt("nightChance") > 15)
        {

            //Set to Day
                    
        }
        else 
        {
            ship.material = shipGlow;
            directionalLight.enabled = false;
            //Set to Night
            extraFireLight.SetActive(true);
            RenderSettings.fogColor = new Color32(75, 57, 66, 255);
            Camera.main.backgroundColor = new Color32(91, 91, 91, 255);
        }
    }
}
