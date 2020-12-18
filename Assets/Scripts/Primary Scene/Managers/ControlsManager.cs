using UnityEngine;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviour
{
    #region Singleton
    public static ControlsManager Singleton;
    private void Awake() { Singleton = this; }
    #endregion


    public Image cover; //A transparent button cover.
    public Settings settings;//Current game settings.
    public Sprite[] useIcons;
    public ControlObject[] controlObjects;
    private int btn_opacity; //Button Opacity
    private int text_opacity; //Button Text Opacity
    private bool uiActive;


    #region SettingsMenu Changed Callback
    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;
    }
    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;
    }
    private void Change()
    {
        int opacity = settings.gameControlsOpacity;
        if(btn_opacity != opacity && text_opacity != opacity) 
        {
            SetOpacity(settings.gameControlsOpacity);
        }
    }
    #endregion

    void Start() 
    {
        Change();
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

   
    public void SetOpacity(int target_opacity) 
    {
        btn_opacity = text_opacity = target_opacity;
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
            if (!controlObjects[i].autoShowHide)
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
                    if (controlObjects[i].changeColor)
                    {
                        controlObjects[i].icon[e].color = iconColor;
                    }
                    else
                    {
                        Color32 tempColor = controlObjects[i].icon[e].color;
                        if (uiActive)
                        {
                            controlObjects[i].icon[e].color = new Color32(tempColor.r, tempColor.g, tempColor.b, (byte)text_opacity);
                        }
                        else
                        {
                            controlObjects[i].icon[e].color = new Color32(tempColor.r, tempColor.g, tempColor.b, 0);
                        }
                    }
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
            int opacity = settings.gameControlsOpacity;
            if (btn_opacity != opacity && text_opacity != opacity)
            {
                SetOpacity(settings.gameControlsOpacity);
            }
        }
        cover.raycastTarget = !value;
    }
}
