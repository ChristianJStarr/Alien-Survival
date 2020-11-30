using MLAPI;
using System;
using System.Collections;
using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public Light directionalLight;
    private Camera mainCamera;

    int partOfDay = 0;

    //Sun & Sky Targets
    private float sunIntensityTarget;
    private Color32 fogColorTarget;
    private Color32 skyColorTarget;
    
    //Configuration
    private bool allowSunFade = false;
    private bool allowSkyFade = false;
    private bool allowFogFade = false;
    private int fadeStep = 1;
    private int secondsInHour = 120;



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
        WaitForSeconds wait = new WaitForSeconds(10);
        while (true)
        {
            int hour = GetCurrentGameTimeHour();
            if (partOfDay != GetPartOfDay(hour)) 
            {
                partOfDay = GetPartOfDay(hour);
                ChangeSky(hour, false);
            }
            yield return wait;
        }
    }

    public void ForceCheckTime() 
    {
        int hour = GetCurrentGameTimeHour();
        if (partOfDay != GetPartOfDay(hour))
        {
            partOfDay = GetPartOfDay(hour);
            ChangeSky(hour, true);
        }
    }

    //Change the Sky to GameHour (force = noLerp)
    private void ChangeSky(int hour, bool force) 
    {
        
        if (force) 
        {
            float sunIntensity = GetSunIntensity(partOfDay);
            if (directionalLight.intensity != sunIntensity)
            {
                directionalLight.intensity = sunIntensity;
            }

            Color32 skyColor = GetSkyColor(partOfDay);
            if(mainCamera.backgroundColor != skyColor) 
            {
                mainCamera.backgroundColor = skyColor;
            }

            Color32 fogColor = GetFogColor(partOfDay);
            if(RenderSettings.fogColor != fogColor)
            {
                RenderSettings.fogColor = fogColor;
            }
        }
        else 
        {
            float sunIntensity = GetSunIntensity(partOfDay);
            if (directionalLight.intensity != sunIntensity)
            {
                sunIntensityTarget = sunIntensity;
                allowSunFade = true;
            }

            Color32 skyColor = GetSkyColor(partOfDay);
            if (mainCamera != null && mainCamera.backgroundColor != skyColor)
            {
                skyColorTarget = skyColor;
                allowSkyFade = true;
            }

            Color32 fogColor = GetFogColor(partOfDay);
            if (RenderSettings.fogColor != fogColor)
            {
                fogColorTarget = fogColor;
                allowFogFade = true;
            }
        }
    }

    //Manage Sun,Sky,Fog Lerpping
    private void Update()
    {
        if (allowSunFade) { FadeSun(); }
        if (allowSkyFade) { FadeSky(); }
        if (allowFogFade) { FadeFog(); }
    }

    //Sun Lerp
    private void FadeSun()
    {
        if(directionalLight.intensity != sunIntensityTarget) 
        {
            directionalLight.intensity = Mathf.Lerp(directionalLight.intensity, sunIntensityTarget, fadeStep * Time.deltaTime);
        }
        else 
        {
            allowSunFade = false;
        }
    }
    
    //Sky Lerp
    private void FadeSky() 
    {
        if(mainCamera.backgroundColor != skyColorTarget) 
        {
            mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, skyColorTarget, fadeStep * Time.deltaTime);
        }
        else 
        {
            allowSkyFade = false;
        }
    }
    
    //Fog Lerp
    private void FadeFog()
    {
        if(RenderSettings.fogColor != fogColorTarget) 
        {
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, fogColorTarget, fadeStep * Time.deltaTime);
        }
        else 
        {
            allowFogFade = false;
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
        if (hour >= 7 && hour <= 10)
        {
            return 1;
        }
        else if (hour >= 11 && hour <= 15)
        {
            return 2;
        }
        else if (hour >= 16 && hour <= 18)
        {
            return 3;
        }
        else if (hour >= 19 && hour <= 21)
        {
            return 4;
        }
        else if (hour >= 22 || hour <= 3)
        {
            return 5;
        }
        else if (hour >= 4 && hour <= 6)
        {
            return 6;
        }
        else
        {
            return 0;
        }
    }

    //Get target Fog Color
    private Color32 GetFogColor(int partOfDay) 
    {
        if (partOfDay == 1)//Morning 
        {
            return new Color32(189, 219, 218, 255);
        }
        else if (partOfDay == 2)//Afternoon 
        {
            return new Color32(211, 211, 211, 255);
        }
        else if (partOfDay == 3)//Mid-Evening
        {
            return new Color32(109, 138, 159, 255);
        }
        else if (partOfDay == 4)//Evening
        {
            return new Color32(144, 144, 144, 255);
        }
        else if (partOfDay == 5)//Night 
        {
            return new Color32(73, 73, 73, 255);
        }
        else //Early Morning 
        {
            return new Color32(107, 144,141, 255);
        }
    }
    
    //Get target Sky Color
    private Color32 GetSkyColor(int partOfDay) 
    {
        if (partOfDay == 1)//Morning 
        {
            return new Color32(213, 253, 249, 255);
        }
        else if (partOfDay == 2)//Afternoon 
        {
            return new Color32(255, 255, 255, 255);
        }
        else if (partOfDay == 3)//Mid-Evening
        {
            return new Color32(217, 217, 217, 255);
        }
        else if (partOfDay == 4)//Evening
        {
            return new Color32(188, 188, 188, 255);
        }
        else if (partOfDay == 5)//Night 
        {
            return new Color32(91, 91, 91, 255);
        }
        else //Early Morning 
        {
            return new Color32(159, 204, 200, 255);
        }
    }

    //Get target Sun Intensity
    private float GetSunIntensity(int partOfDay) 
    {
        if(partOfDay == 1)//Morning 
        {
            return 0.7F;
        }
        else if (partOfDay == 2)//Afternoon 
        {
            return 1.25F;
        }
        else if (partOfDay == 3)//Mid-Evening
        {
            return 0.8F;
        }
        else if (partOfDay == 4)//Evening
        {
            return .5F;
        }
        else if (partOfDay == 5)//Night 
        {
            return 0F;
        }
        else//Early Morning 
        {
            return 0.2F;
        }
    }

    //Get Current Game Time in Hours
    private int GetCurrentGameTimeHour()
    {
        if (NetworkingManager.Singleton == null) return 0;
        return TimeSpan.FromSeconds(NetworkingManager.Singleton.NetworkTime).Hours;
    }

}
