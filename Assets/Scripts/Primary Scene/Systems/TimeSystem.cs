using MLAPI;
using System;
using System.Collections;
using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public Light directionalLight;
    private Camera mainCamera;
    
    [Header("Game Minute : Real Seconds")]
    public int gameMinutesInSecond = 1; //60 seconds per hour

    [Header("Early Morning  5am-6am")]
    public Color32 fogColor0 = new Color32(149, 133, 127, 255);
    public Color32 skyColor0 = new Color32(171, 158, 154, 255);
    [Range(0, 2)]
    public float sunIntensity0 = 0.5F;
    [Space]
    [Header("Morning  7am-10am")]
    public Color32 fogColor1 = new Color32(189, 219, 218, 255);
    public Color32 skyColor1 = new Color32(255, 255, 255, 255);
    [Range(0, 2)]
    public float sunIntensity1 = 0.7F;
    [Space]
    [Header("Afternoon  11am-3pm")]
    public Color32 fogColor2 = new Color32(189, 219, 218, 255);
    public Color32 skyColor2 = new Color32(255, 255, 255, 255);
    [Range(0, 2)]
    public float sunIntensity2 = 1.25F;
    [Space]
    [Header("Mid-Evening  4pm-6pm")]
    public Color32 fogColor3 = new Color32(144, 144, 144, 255);
    public Color32 skyColor3 = new Color32(217, 217, 217, 255);
    [Range(0, 2)]
    public float sunIntensity3 = 0.8F;
    [Space]
    [Header("Evening  7pm-8pm")]
    public Color32 fogColor4 = new Color32(38, 25, 15, 255);
    public Color32 skyColor4 = new Color32(123, 96, 76, 255);
    [Range(0, 2)]
    public float sunIntensity4 = 0.5F;
    [Space]
    [Header("Night  9pm-4am")]
    public Color32 fogColor5 = new Color32(38, 38, 38, 255);
    public Color32 skyColor5 = new Color32(31, 31, 31, 255);
    [Range(0,2)]
    public float sunIntensity5 = 0;


    private bool lerpingSun, lerpingSky, lerpingFog;
    private int partOfDay = 0;
    



    private void Start()
    {
        if(NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient) 
        {
            StartCoroutine(CheckTimeForDayChange());
        }
    }

    //Check Time Loop
    private IEnumerator CheckTimeForDayChange() 
    {
        WaitForSeconds wait = new WaitForSeconds(5);
        while (true)
        {
            int hour = GetCurrentGameTimeHour();
            if (partOfDay != GetPartOfDay(hour)) 
            {
                if (mainCamera == null) mainCamera = Camera.main;
                partOfDay = GetPartOfDay(hour);
                ChangeSky(partOfDay, false);
            }
            yield return wait;
        }
    }

    public void ForceCheckTime() 
    {
        int hour = GetCurrentGameTimeHour();
        if (partOfDay != GetPartOfDay(hour))
        {
            if (mainCamera == null) mainCamera = Camera.main;
            partOfDay = GetPartOfDay(hour);
            ChangeSky(partOfDay, true);
        }
    }

    //Change the Sky to GameHour (force = noLerp)
    private void ChangeSky(int _partOfDay, bool force) 
    {
        if (force) 
        {
            float sunIntensity = GetSunIntensity(_partOfDay);
            if (directionalLight.intensity != sunIntensity)
            {
                directionalLight.intensity = sunIntensity;
            }

            Color32 skyColor = GetSkyColor(_partOfDay);
            if(mainCamera.backgroundColor != skyColor) 
            {
                mainCamera.backgroundColor = skyColor;
            }

            Color32 fogColor = GetFogColor(_partOfDay);
            if(RenderSettings.fogColor != fogColor)
            {
                RenderSettings.fogColor = fogColor;
            }
        }
        else 
        {
            float duration = (60 / gameMinutesInSecond); //4s duration

            float sunIntensity = GetSunIntensity(_partOfDay);
            if (directionalLight.intensity != sunIntensity && !lerpingSun)
            {
                StartCoroutine(LerpSunIntensity(sunIntensity, duration));
                lerpingSun = true;
            }

            Color skyColor = GetSkyColor(_partOfDay);
            if (mainCamera != null && mainCamera.backgroundColor != skyColor && !lerpingSky)
            {
                lerpingSky = true;
                StartCoroutine(LerpSkyColor(skyColor, duration));
            }

            Color fogColor = GetFogColor(_partOfDay);
            if (RenderSettings.fogColor != fogColor && !lerpingFog)
            {
                lerpingFog = true;
                StartCoroutine(LerpFogColor(fogColor, duration));
            }
        }
    }

    //Get Part of Day from GameHour
    private int GetPartOfDay(int hour)
    {
        //-----Part of Day Key-----//
        //     0 = Early Morning   //
        //     1 = Morning         //
        //     2 = Afternoon       //
        //     3 = MidEvening      //
        //     4 = Evening         //
        //     5 = Night           //
        //-------------------------//
        if (hour >= 7 && hour <= 10) //7am - 10am  3h
        {
            return 1;
        }
        else if (hour >= 11 && hour <= 15) //11am - 3pm  4h
        {
            return 2;
        }
        else if (hour >= 16 && hour <= 18) //4pm - 6pm  2h
        {
            return 3;
        }
        else if (hour >= 19 && hour <= 20) //7pm - 8pm  1h
        {
            return 4;
        }
        else if (hour >= 21 || hour <= 4) //9pm - 4am 7hr
        {
            return 5;
        }
        else if (hour >= 5 && hour <= 6) //5am - 6am 1hr
        {
            return 0;
        }
        return 0;
    }

    //Get target Fog Color
    private Color32 GetFogColor(int partOfDay) 
    {
        if (partOfDay == 1)//Morning 
        {
            return fogColor1;
        }
        else if (partOfDay == 2)//Afternoon 
        {
            return fogColor2;
        }
        else if (partOfDay == 3)//Mid-Evening
        {
            return fogColor3;
        }
        else if (partOfDay == 4)//Evening
        {
            return fogColor4;
        }
        else if (partOfDay == 5)//Night 
        {
            return fogColor5;
        }
        else //Early Morning 
        {
            return fogColor0;
        }
    }
    
    //Get target Sky Color
    private Color32 GetSkyColor(int partOfDay) 
    {
        if (partOfDay == 1)//Morning 
        {
            return skyColor1;
        }
        else if (partOfDay == 2)//Afternoon 
        {
            return skyColor2;
        }
        else if (partOfDay == 3)//Mid-Evening
        {
            return skyColor3;
        }
        else if (partOfDay == 4)//Evening
        {
            return skyColor4;
        }
        else if (partOfDay == 5)//Night 
        {
            return skyColor5;
        }
        else //Early Morning 
        {
            return skyColor0;
        }
    }

    //Get target Sun Intensity
    private float GetSunIntensity(int partOfDay) 
    {
        if(partOfDay == 1)//Morning 
        {
            return sunIntensity1;
        }
        else if (partOfDay == 2)//Afternoon 
        {
            return sunIntensity2;
        }
        else if (partOfDay == 3)//Mid-Evening
        {
            return sunIntensity3;
        }
        else if (partOfDay == 4)//Evening
        {
            return sunIntensity4;
        }
        else if (partOfDay == 5)//Night 
        {
            return sunIntensity5;
        }
        else//Early Morning 
        {
            return sunIntensity0;
        }
    }

    //Get Current Game Time in Hours
    private int GetCurrentGameTimeHour()
    {
        float seconds = 0;
        if (NetworkingManager.Singleton == null) return 0;
        seconds = NetworkingManager.Singleton.NetworkTime * gameMinutesInSecond * 60;
        TimeSpan span = TimeSpan.FromSeconds(seconds);
        DebugMenu.UpdateTime(span);
        return span.Hours;
    }

    //Lerp the Sky Color
    private IEnumerator LerpSkyColor(Color target, float duration)
    {
        float time = 0;
        Color startValue = mainCamera.backgroundColor;
        while (time < duration)
        {
            mainCamera.backgroundColor = Color.Lerp(startValue, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        mainCamera.backgroundColor = target;
        lerpingSky = false;
    }
    //Lerp the Fog Color
    private IEnumerator LerpFogColor(Color target, float duration)
    {
        float time = 0;
        Color startValue = RenderSettings.fogColor;
        while (time < duration)
        {
            RenderSettings.fogColor = Color.Lerp(startValue, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        RenderSettings.fogColor = target;
        lerpingFog = false;
    }
    //Lerp the Sun Intensity
    private IEnumerator LerpSunIntensity(float target, float duration) 
    {
        float time = 0;
        float startValue = directionalLight.intensity;
        while (time < duration)
        {
            directionalLight.intensity = Mathf.Lerp(startValue, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        directionalLight.intensity = target;
        lerpingSun = false;
    }

}
