using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/Settings", order = 1)]
public class Settings : ScriptableObject
{
    //Original Settings?
    public bool validated = false;


    //Volume
    public float uiVolume = 1;
    public float musicVolume = 1;
    public float ambientVolume = 1;
    public float effectsVolume = 1;

    //Quality
    public bool autoQuality = false; 
    public int quality = 2;

    //Distance
    public float terrainDistance = 500;
    public float objectDistance = 250;

    //Controls
    public int gameControlsOpacity = 200;
    public Vector2 sensitivity = new Vector2(0.5F, 0.5F);
    public float xSensitivity = .5F;
    public float ySensitivity = .5F;

    //NotifyTray
    public bool showFps = false;
    public bool showPing = false;
    public bool showConsole = false;
    public bool showBattery = false;
    public bool showTime = false;



    //Chat Box
    public byte chatBoxState = 3;

    //Control Layout

}
