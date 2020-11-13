using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class TimeSystem : NetworkedBehaviour
{
    public Light directionalLight;
    private Camera mainCamera;
    private DateTime now;
    private TimeSpan timeNow;
    private TimeSpan gameTime;
    private float sunIntensityTarget;
    private Color32 fogColorTarget;
    private Color32 skyColorTarget;
    private bool allowSunFade = false;
    private bool allowSkyFade = false;
    private bool allowFogFade = false;
    private int minutesInDay = 48;
    private int tickRate = 5;
    private int fadeStep = 2;
    private int tickCount = 0;

    public override void NetworkStart()
    {
        if (IsServer) 
        {
            StartCoroutine(DayCycle());
        }
    }

    public void UpdateClientTime(Action<bool> callback) 
    {
        mainCamera = Camera.main;
        StartCoroutine(RequestTheTimeWait(onReturnValue =>
        {
            ChangeSky(onReturnValue, true);
            callback(true);
        }));
    }
    
    private IEnumerator RequestTheTimeWait(Action<int> callback)
    {
        RpcResponse<int> response = InvokeServerRpc(RequestTimeServer_Rpc);
        while (!response.IsDone) { yield return null; }
        callback(response.Value);
    }
    
    [ServerRPC(RequireOwnership = false)]
    public int RequestTimeServer_Rpc()
    {
        return GetTime().Hours;
    }

    private IEnumerator DayCycle() 
    {
        yield return new WaitForSeconds(120);
        StartCoroutine(DayCycle());
        DayNightTick();
    }

    private void DayNightTick() 
    {
        using (PooledBitStream stream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(stream))
            {
                writer.WriteInt32Packed(GetTime().Hours);
                InvokeClientRpcOnEveryonePerformance(DayNightTickClient_Rpc, stream);
            }
        }
    }

    [ClientRPC]
    private void DayNightTickClient_Rpc(ulong clientId, Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            ChangeSky(reader.ReadInt32Packed() ,false);
        }
    }

    private void ChangeSky(int hour, bool force) 
    {
        int partOfDay = GetPartOfDay(hour);
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
            if (mainCamera.backgroundColor != skyColor)
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

    private void Update()
    {
        if (tickCount == tickRate)
        {
            tickCount = 0;
            if (allowSunFade) { FadeSun(); }
            if (allowSkyFade) { FadeSky(); }
            if (allowFogFade) { FadeFog(); }
        }
        else 
        {
            tickCount++;
        }
    }

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
  
    public TimeSpan GetTime()
    {
        now = DateTime.Now;
        timeNow = now.TimeOfDay;
        double hours = timeNow.TotalMinutes % minutesInDay;
        double minutes = (hours % 1) * 60;
        double seconds = (minutes % 1) * 60;
        gameTime = new TimeSpan((int)hours, (int)minutes, (int)seconds);

        return gameTime;
    }

    private int GetPartOfDay(int hour)
    {
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

}
