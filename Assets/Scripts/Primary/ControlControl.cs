using UnityEngine;
using UnityEngine.UI;

public class ControlControl : MonoBehaviour
{
    public static ControlControl Singleton;
    


    //What: UI Controls Controller
    //Where: Primary Scene / Interface

    public Image cover; //A transparent button cover.
    public Settings settings;//Current game settings.

    private int btn_opacity; //Button Opacity
    private int text_opacity; //Button Text Opacity

    public Sprite[] useIcons;
    public ControlObject[] controlObjects;

    private bool uiActive;

    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;
    }
    
    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;
    }

    void Awake() 
    {
        Singleton = this;
    }

    void Start() 
    {
        Change();
    }

    private void UpdateColor() 
    {
        Color backgroundColor;
        Color iconColor;
        if (uiActive)
        {
            backgroundColor = new Color32(0, 0, 0, (byte)btn_opacity);
            iconColor = new Color32(255, 255, 255, (byte)text_opacity);
        }
        else
        {
            backgroundColor = new Color32(255, 255, 255, 0);
            iconColor = new Color32(255, 255, 255, 0);
        }
        for (int i = 0; i < controlObjects.Length; i++)
        {
            if(!controlObjects[i].autoShowHide) 
            {
                backgroundColor = new Color32(0, 0, 0, (byte)btn_opacity);
                iconColor = new Color32(255, 255, 255, (byte)text_opacity);
            }
            if (controlObjects[i].background != null)
            {
                for (int e = 0; e < controlObjects[i].background.Length; e++)
                {
                    controlObjects[i].background[e].color = backgroundColor;
                }
            }
            if (controlObjects[i].icon != null)
            {
                for (int e = 0; e < controlObjects[i].icon.Length; e++)
                {
                    controlObjects[i].icon[e].color = iconColor;
                }
            }
            if (controlObjects[i].extraBackground != null)
            {
                int bkg_opacity = (btn_opacity / 3) * 2;
                for (int e = 0; e < controlObjects[i].extraBackground.Length; e++)
                {
                    controlObjects[i].extraBackground[e].color = new Color32(0, 0, 0, (byte)bkg_opacity);
                }
            }
        }
    }

    public void UpdateUseIcon(int useType) 
    {
        //for (int i = 0; i < controlObjects.Length; i++)
        //{
        //    if(controlObjects[i].typeId == 1 && controlObjects[i].icon.sprite != useIcons[useType - 1]) 
        //    {
        //        //controlObjects[i].icon.sprite = useIcons[useType - 1];
        //    }
        //}
    }

    
    
    
    
    //Change Button Opacity. Standard settingsMenu Change().
    private void Change()
    {
        //Set opacity to stored settings. 0-255.
        SetOpacity(settings.gameControlsOpacity); 
    }

    //Set opacity from int.
    private void SetOpacity(int value) 
    {
        //if((value * 2) >= 255)
        //{
        //    text_opacity = 255;
        //}
        //else 
        //{
        //    text_opacity = value * 2;
        //}
        text_opacity = value;
        btn_opacity = value;
        UpdateColor();
    }

    //Hide controls.
    public void Hide() 
    {
        ToggleVisible(false);
    }
    
    //Show controls.
    public void Show() 
    {
        ToggleVisible(true);
    }
    
    //Change opacity of buttons.
    private void ToggleVisible(bool value)
    {
        if(uiActive != value) 
        {
            uiActive = value;
            UpdateColor();
        }
        cover.raycastTarget = !value;
    }
}
