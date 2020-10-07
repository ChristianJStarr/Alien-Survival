using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    public Settings settings; // Game Settings data.
    public Slider ui, menu, ambient, effects, xSense, ySense, opacity, terrainDis, objectDis; //All settings sliders
    public ToggleGroup quality, shadows, aliasing, postpro, showFps, showPing, showTime, showBattery; //All settings toggle groups.
    public GameObject qualityContainer, manualContainer;
    public Button qualityModeButton_Manual, qualityModeButton_Auto;

    public delegate void OnGameSettingsChangedDelegate();
    public static event OnGameSettingsChangedDelegate ChangedSettings;

    private RectTransform qualityContainerRect;

    public TextMeshProUGUI qualityModeAutoText;
    public TextMeshProUGUI qualityModeManualText;
    public ScrollRect scrollRect;

    private bool realignScroll = false;


    private PauseControl pauseControl;
    private MainMenuScript mainMenuScript;


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
        SetToggle(shadows, settings.shadow);
        SetToggle(aliasing, settings.aliasing);

        //Get Controls
        xSense.value = settings.xSensitivity;
        ySense.value = settings.ySensitivity;
        opacity.value = settings.gameControlsOpacity;

        //Get Distance
        terrainDis.value = settings.terrainDistance;
        objectDis.value = settings.objectDistance;

        //SetDebug
        SetToggleBool(showFps, settings.showFps);
        SetToggleBool(showPing, settings.showPing);
        SetToggleBool(showTime, settings.showTime);
        SetToggleBool(showBattery, settings.showBattery);
        scrollRect.verticalNormalizedPosition = 1;

        if (!settings.validated) 
        {
            settings = new Settings();
            settings.validated = true;
        }
    }


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

    public void QualityModeAuto() 
    {

        Debug.Log(scrollRect.verticalNormalizedPosition);
        SetMode(true);
        settings.autoQuality = true;

        Debug.Log(scrollRect.verticalNormalizedPosition);
        scrollRect.verticalNormalizedPosition = 1;
    }
    
    public void QualityModeManual() 
    {
        Debug.Log(scrollRect.verticalNormalizedPosition);
        SetMode(false);
        settings.autoQuality = false;
        Debug.Log(scrollRect.verticalNormalizedPosition);
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

    //Called from Terrain Distance Slider in settings. Keeps Object Max Distance >= Terrain Distance
    public void TerrainDistance() 
    {
        objectDis.maxValue = terrainDis.value;
        if(objectDis.value > terrainDis.value) 
        {
            objectDis.value = terrainDis.value;
        }
    }


    public void BackButton() 
    {
        if(SceneManager.GetActiveScene().name == "Primary") 
        {
            if(pauseControl == null) 
            {
                pauseControl = FindObjectOfType<PauseControl>();
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
            settings.shadow = Convert.ToInt32(shadows.ActiveToggles().FirstOrDefault().name);
            settings.aliasing = Convert.ToInt32(aliasing.ActiveToggles().FirstOrDefault().name);
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

        //Distance
        settings.terrainDistance = terrainDis.value;
        settings.objectDistance = objectDis.value;

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
