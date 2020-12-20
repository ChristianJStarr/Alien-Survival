using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public Settings settings; // Game Settings data.
    public Slider ui, menu, ambient, effects, xSense, ySense, opacity; //All settings sliders
    public ToggleGroup quality, showFps, showPing, showTime, showBattery; //All settings toggle groups.
    public delegate void OnGameSettingsChangedDelegate();
    public static event OnGameSettingsChangedDelegate ChangedSettings;
    


    void Start() 
    {
#if !UNITY_SERVER
        //Get Volume
        ui.value = settings.uiVolume;
        menu.value = settings.musicVolume;
        ambient.value = settings.ambientVolume;
        effects.value = settings.effectsVolume;
        //Get Quality
        SetToggle(quality, settings.quality);
        //Get Controls
        xSense.value = settings.xSensitivity;
        ySense.value = settings.ySensitivity;
        opacity.value = settings.gameControlsOpacity;
        //SetDebug
        SetToggleBool(showFps, settings.showFps);
        SetToggleBool(showPing, settings.showPing);
        SetToggleBool(showTime, settings.showTime);
        SetToggleBool(showBattery, settings.showBattery);
#endif
    }

    //Sets a toggle button inside of toggle group by int.
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

    //Sets a toggle button inside of toggle group by bool.
    private void SetToggleBool(ToggleGroup group, bool value) 
    {
        string valueString = "hide";
        if (value) { valueString = "show"; }
        foreach (Toggle item in group.GetComponentsInChildren<Toggle>())
        {
            group.SetAllTogglesOff();
            if (item.name == valueString)
            {
                item.isOn = true;
                break;
            }
        }
    }

    //Back BTN
    public void BackButton() 
    {
        if(SceneManager.GetActiveScene().name == "Primary") 
        {
            UI_PauseControl pauseControl = FindObjectOfType<UI_PauseControl>();
            if(pauseControl != null) pauseControl.ButtonSettings();
        }
        else 
        {
            MainMenuScript mainMenuScript = FindObjectOfType<MainMenuScript>();
            if (mainMenuScript != null) mainMenuScript.CloseMenu();
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
        if (settings.autoQuality)
        {
            
            //Init auto settings
        }
        else
        {
            settings.quality = Convert.ToInt32(quality.ActiveToggles().FirstOrDefault().name);
            //Change quality level settings.
            QualitySettings.SetQualityLevel(settings.quality - 1, true);

        }

        //Controls
        settings.xSensitivity = xSense.value;
        settings.ySensitivity = ySense.value;
        settings.gameControlsOpacity = (int)opacity.value;

        //Object Distance
        settings.objectDistance = GetObjectDistance();

        //Debug
        if (showFps.ActiveToggles().FirstOrDefault().name == "show")
        {
            settings.showFps = true;
        }
        else if (showFps.ActiveToggles().FirstOrDefault().name == "hide")
        {
            settings.showFps = false;
        }
        if (showPing.ActiveToggles().FirstOrDefault().name == "show")
        {
            settings.showPing = true;
        }
        else if (showPing.ActiveToggles().FirstOrDefault().name == "hide")
        {
            settings.showPing = false;
        }
        if (showTime.ActiveToggles().FirstOrDefault().name == "show")
        {
            settings.showTime = true;
        }
        else if (showTime.ActiveToggles().FirstOrDefault().name == "hide")
        {
            settings.showTime = false;
        }
        if (showBattery.ActiveToggles().FirstOrDefault().name == "show")
        {
            settings.showBattery = true;
        }
        else if (showBattery.ActiveToggles().FirstOrDefault().name == "hide")
        {
            settings.showBattery = false;
        }
        ChangedSettings?.Invoke();
        BackButton();
    }

    //Get Object Distance
    public int GetObjectDistance() 
    {
        if (settings.quality == 4)
        {
            return 1000;
        }
        else if (settings.quality == 3)
        {
            return 700;
        }
        else if (settings.quality == 2)
        {
            return 500;
        }
        else
        {
            return 300;
        }
    }
}
