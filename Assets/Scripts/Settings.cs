﻿using System.Collections;
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
    public int textureQuality = 3;
    public int shadowQuality = 3;
    public int reflectionQuality = 3;
    public int objectQuality = 3;

    //Sensitivity
    public float xSensitivity = 1;
    public float ySensitivity = 1;

    //Distance
    public float terrainDistance = 1;
    public float objectDistance = 1;
}
