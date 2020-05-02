using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects", order = 1)]
public class Settings : ScriptableObject
{
    // Volume
    public float uiVolume = 1;
    public float musicVolume = 1;
    public float ambientVolume = 1;
    public float effectsVolume = 1;

    //Graphics
    public int quality = 3;

    //Sensitivity
    public float xSensitivity = .5F;
    public float ySensitivity = .5F;

    //Distance
    public float terrainDistance = 500;
    public float objectDistance = 400;
}
