using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    public Settings settings; // Game Settings data.
    public Slider ui, menu, ambient, effects, xSense, ySense, opacity, terrainDis, objectDis; //All settings sliders
    public ToggleGroup quality, shadows, aliasing, postpro; //All settings toggle groups.


    void Start() 
    {
        //Get Volume
        ui.value = settings.uiVolume;
        menu.value = settings.musicVolume;
        ambient.value = settings.ambientVolume;
        effects.value = settings.effectsVolume;

        //Get Quality
        SetToggle(quality, settings.quality);
        SetToggle(shadows, settings.shadow);
        SetToggle(aliasing, settings.aliasing);

        //Get Controls
        xSense.value = settings.xSensitivity;
        ySense.value = settings.ySensitivity;
        opacity.value = settings.gameControlsOpacity;

        //Get Distance
        terrainDis.value = settings.terrainDistance;
        objectDis.value = settings.objectDistance;
    }


    //Sets a toggle button inside of toggle group.
    private void SetToggle(ToggleGroup group, int value) 
    {
       foreach(Toggle item in group.GetComponentsInChildren<Toggle>()) 
        {
            group.SetAllTogglesOff();
            if(Convert.ToInt32(item.name) == value) 
            {
                item.isOn = true;
                break;
            }
        }
    }


    //Called from Terrain Distance Slider in settings. Keeps Object Max Distance >= Terrain Distance
    public void TerrainDistance() 
    {
        objectDis.maxValue = terrainDis.value;
        if(objectDis.value > terrainDis.value) 
        {
            objectDis.value = terrainDis.value;
        }
    }
    

    //Apply Settings Values
    public void ApplySettings() 
    {
    //Store settings menu items in settings scriptable object.
        //Volume
        settings.uiVolume = ui.value;
        settings.musicVolume = menu.value;
        settings.ambientVolume = ambient.value;
        settings.effectsVolume = effects.value;

        //Quality
        settings.quality = Convert.ToInt32(quality.ActiveToggles().FirstOrDefault().name);
        settings.shadow = Convert.ToInt32(shadows.ActiveToggles().FirstOrDefault().name);
        settings.aliasing = Convert.ToInt32(aliasing.ActiveToggles().FirstOrDefault().name);

        //Controls
        settings.xSensitivity = xSense.value;
        settings.ySensitivity = ySense.value;
        settings.gameControlsOpacity = (int)opacity.value;

        //Distance
        settings.terrainDistance = terrainDis.value;
        settings.objectDistance = objectDis.value;

        //Apply the settings that were stored.
        //Change render pipeline asset.
        RenderPipelineAsset newAsset = GetAsset();
        if (GraphicsSettings.renderPipelineAsset != newAsset)
        {
            GraphicsSettings.renderPipelineAsset = newAsset;
        }
        //Change quality level settings.
        QualitySettings.SetQualityLevel(settings.quality - 1, true);
        
        //Have objects update to new values.

        MusicManager music = FindObjectOfType<MusicManager>(); //Find music manager, if any.
        if(music != null) //If music manager exists in scene, update its settings.
         {
            music.Change();
        }

        TouchPad touch = FindObjectOfType<TouchPad>(); //Find touch pad, if any.
        if (touch != null) //If touch pad exists in scene, update its settings.
        {
            touch.Change();
        }

        ControlControl controlControl = FindObjectOfType<ControlControl>(); //Find controls, if any.
        if (controlControl != null) //If control control exists in scene, update its settings.
        {
            controlControl.Change();
        }

        MainMenuCampfireFlicker campfireFlicker = FindObjectOfType<MainMenuCampfireFlicker>();
        if(campfireFlicker != null) 
        {
            campfireFlicker.Change();
        }
    }

    //Get correct pipeline asset based off current settings.
    public RenderPipelineAsset GetAsset() 
    {
        var cameraData = Camera.main.GetUniversalAdditionalCameraData();
        int set_1 = settings.shadow;
        int set_2 = settings.aliasing;
        int set_3 = settings.postpro;
        string value_1 = "OFF";
        string value_2 = "OFF";
        string value_3 = "OFF";

        if (set_1 == 1) { value_1 = "OFF"; }
        if (set_1 == 2) { value_1 = "HARD"; }
        if (set_1 == 3) { value_1 = "SOFT"; }

        if (set_2 == 1) { value_2 = "OFF"; }
        if (set_2 == 2) { value_2 = "2X"; }
        if (set_2 == 3) { value_2 = "4X"; }

        if (set_3 == 1) 
        {
            value_3 = "LDR";
            cameraData.renderPostProcessing = false;
        }
        if (set_3 == 2)
        {
            value_3 = "LDR";
            cameraData.renderPostProcessing = true;
        }
        if (set_3 == 3)
        {
            value_3 = "HDR";
            cameraData.renderPostProcessing = true;
        }
        string filename = value_1 + "-" + value_2 + "-" + value_3;
        return Resources.Load("Data/URPAssets/" + filename) as RenderPipelineAsset;
    }
}
