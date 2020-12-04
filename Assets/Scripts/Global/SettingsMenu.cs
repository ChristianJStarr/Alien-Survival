using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    public Settings settings; // Game Settings data.
    public Slider ui, menu, ambient, effects, xSense, ySense, opacity; //All settings sliders
    public ToggleGroup quality, showFps, showPing, showTime, showBattery; //All settings toggle groups.
    public GameObject qualityContainer, manualContainer;
    public Button qualityModeButton_Manual, qualityModeButton_Auto;

    public delegate void OnGameSettingsChangedDelegate();
    public static event OnGameSettingsChangedDelegate ChangedSettings;

    private RectTransform qualityContainerRect;

    public TextMeshProUGUI qualityModeAutoText;
    public TextMeshProUGUI qualityModeManualText;
    public ScrollRect scrollRect;

    private bool realignScroll = false;


    private UI_PauseControl pauseControl;
    private MainMenuScript mainMenuScript;

    public RenderPipelineAsset low_Pipeline, med_Pipeline, high_Pipeline, ultra_Pipeline;



    void Start() 
    {
        qualityContainerRect = qualityContainer.GetComponent<RectTransform>();

        //Get Volume
        ui.value = settings.uiVolume;
        menu.value = settings.musicVolume;
        ambient.value = settings.ambientVolume;
        effects.value = settings.effectsVolume;

        //Get Quality
        SetMode(settings.autoQuality);
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
        scrollRect.verticalNormalizedPosition = 1; 
    }

    //Set Quality Mode
    private void SetMode(bool mode) 
    {
        manualContainer.SetActive(!mode);
        qualityModeButton_Manual.interactable = !mode;
        qualityModeButton_Auto.interactable = mode;
        if (mode) 
        {
            qualityContainerRect.sizeDelta = new Vector2(qualityContainerRect.sizeDelta.x, 234);
            qualityModeAutoText.color = new Color32(204,204,204,255);
            qualityModeManualText.color = new Color32(46, 46, 46, 255);
        }
        else 
        {
            qualityContainerRect.sizeDelta = new Vector2(qualityContainerRect.sizeDelta.x, 478.2451F);
            qualityModeAutoText.color = new Color32(46, 46, 46, 255);
            qualityModeManualText.color = new Color32(204, 204, 204, 255);
        }
    }

    //Toggle Quality Mode
    public void QualityModeAuto() 
    {
        SetMode(true);
        settings.autoQuality = true;
        scrollRect.verticalNormalizedPosition = 1;
    }
    
    //Toggle Quality Mode
    public void QualityModeManual() 
    {
        SetMode(false);
        settings.autoQuality = false;
        scrollRect.verticalNormalizedPosition = 1;
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
            if(pauseControl == null) 
            {
                pauseControl = FindObjectOfType<UI_PauseControl>();
            }
            if(pauseControl != null) 
            {
                pauseControl.ButtonSettings();
            }   
        }
        else 
        {
            if (mainMenuScript == null)
            {
                mainMenuScript = FindObjectOfType<MainMenuScript>();
            }
            if (mainMenuScript != null)
            {
                mainMenuScript.CloseMenu();
            }
        }

        scrollRect.verticalNormalizedPosition = 1;
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
            //Change render pipeline asset.
            RenderPipelineAsset newAsset = GetAsset();
            if (GraphicsSettings.renderPipelineAsset != newAsset)
            {
                GraphicsSettings.renderPipelineAsset = newAsset;
            }
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
        ChangedSettings();
        BackButton();
    }

    
    //-----Get Values From Quality Level
    
    //Get Render Asset
    public RenderPipelineAsset GetAsset() 
    {
        if (settings.quality == 4)
        {
            return ultra_Pipeline;
        }
        else if (settings.quality == 3)
        {
            return high_Pipeline;
        }
        else if (settings.quality == 2)
        {
            return med_Pipeline;
        }
        else
        {
            return low_Pipeline;
        }
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
