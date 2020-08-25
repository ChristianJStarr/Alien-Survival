using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/Settings", order = 1)]
public class Settings : ScriptableObject
{
    //Volume
    public float uiVolume = 1;
    public float musicVolume = 1;
    public float ambientVolume = 1;
    public float effectsVolume = 1;

    //Quality
    public bool autoQuality = false; 
    public int quality = 2;
    public int shadow = 2;
    public int aliasing = 2;
    public int postpro = 2;

    //Distance
    public float terrainDistance = 500;
    public float objectDistance = 250;

    //Controls
    public int gameControlsOpacity = 200;
    public float xSensitivity = .5F;
    public float ySensitivity = .5F;

    //Debug
    public bool showFps = false;
    public bool showPing = false;
    public bool showConsole = false;

    //Control Layout
        
}
