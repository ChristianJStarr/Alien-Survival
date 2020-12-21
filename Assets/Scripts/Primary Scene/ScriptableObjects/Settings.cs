using System;
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

    internal void Validate()
    {
        if (!validated)
        {
            uiVolume = 1;
            musicVolume = 1;
            ambientVolume = 1;
            effectsVolume = 1;
            quality = 2;
            terrainDistance = 500;
            objectDistance = 250;
            gameControlsOpacity = 200;
            sensitivity = new Vector2(0.5F, 0.5F);
            validated = true;
        }
    }

    //Quality 
    public int quality = 2;

    //Distance
    public float terrainDistance = 500;
    public int objectDistance = 250;

    //Controls
    public int gameControlsOpacity = 200;
    public Vector2 sensitivity = new Vector2(0.5F, 0.5F);

    //NotifyTray
    public bool showFps = false;
    public bool showPing = false;
    public bool showConsole = false;
    public bool showBattery = false;
    public bool showTime = false;

    //Camera
    public bool firstPersonMode = true;

    //Chat Box
    public byte chatBoxState = 3;

    //Control Layout

}
