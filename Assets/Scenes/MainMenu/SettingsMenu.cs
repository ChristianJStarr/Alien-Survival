using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Settings settings;
    public Slider ui, menu, ambient, effects;
    public ToggleGroup texture, shadow, reflection, render;


    void Start() 
    {
        GetSettings();
    }
    private void GetSettings() 
    {
        ui.value = settings.uiVolume;
        menu.value = settings.musicVolume;
        ambient.value = settings.ambientVolume;
        effects.value = settings.effectsVolume;

        SetToggle(texture, settings.textureQuality);
        SetToggle(shadow, settings.shadowQuality);
        SetToggle(reflection, settings.reflectionQuality);
        SetToggle(render, settings.renderQuality);
    }

    private void SetToggle(ToggleGroup group, int value) 
    {
       foreach(Toggle item in group.GetComponentsInChildren<Toggle>()) 
        {
            group.SetAllTogglesOff();
            if(Convert.ToInt32(item.name) == value) 
            {
                Debug.Log(item.name);
                item.isOn = true;
                break;
            }
        }
    }

    public void ApplySettings() 
    {
        settings.uiVolume = ui.value;
        settings.musicVolume = menu.value;
        settings.ambientVolume = ambient.value;
        settings.effectsVolume = effects.value;
        
        settings.textureQuality = Convert.ToInt32(texture.ActiveToggles().FirstOrDefault().name);
        settings.shadowQuality = Convert.ToInt32(shadow.ActiveToggles().FirstOrDefault().name);
        settings.reflectionQuality = Convert.ToInt32(reflection.ActiveToggles().FirstOrDefault().name);
        settings.renderQuality = Convert.ToInt32(render.ActiveToggles().FirstOrDefault().name);

        FindObjectOfType<MusicManager>().Change();
    }
}
