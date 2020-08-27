using UnityEngine;
using UnityEngine.UI;

public class ControlControl : MonoBehaviour
{
    //What: UI Controls Controller
    //Where: Primary Scene / Interface

    public Image cover; //A transparent button cover.
    public GameObject leftScreen;//Left screen container.
    public GameObject rightScreen;//Right screen container.
    public Settings settings;//Current game settings.

    private int btn_opacity; //Button Opacity
    private int text_opacity; //Button Text Opacity

    public Image JoyBkg, JoyStick, UseLeft, UseRight, Button1, Button2, Button3, Button4;
    public Image useLeft_Icon, useRight_Icon, Button1_Icon, Button2_Icon, Button3_Icon, Button4_Icon, JoyIconLock;

    public Sprite handSprite;
    public Sprite shootSprite;

    private void OnEnable()
    {
        SettingsMenu.ChangedSettings += Change;
    }
    
    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;
    }

    void Start() 
    {
        Change();
    }

    //Change Button Opacity. Standard settingsMenu Change().
    private void Change()
    {
        //Set opacity to stored settings. 0-255.
        SetOpacity(settings.gameControlsOpacity); 
    }

    //Swap Use Icons
    public void SwapUse(int useType)
    {
        //Hand
        if (useType == 0)
        {
            useLeft_Icon.sprite = handSprite;
            useRight_Icon.sprite = handSprite;
            Button3.enabled = false;
            Button2_Icon.enabled = false;
            Button3.GetComponent<Button>().interactable = false;
        }
        //Shoot
        else if (useType == 1) 
        {
            useLeft_Icon.sprite = shootSprite;
            useRight_Icon.sprite = shootSprite;
            Button3.enabled = true;
            Button2_Icon.enabled = true;
            Button3.GetComponent<Button>().interactable = true;
        }
    }

    //Change Color of Bkg Image
    private void ChangeImageColor(Color color) 
    {
        JoyBkg.color = color;
        JoyStick.color = color;
        UseLeft.color = color;
        UseRight.color = color;
        Button1.color = color;
        Button2.color = color;
        Button3.color = color;
        Button4.color = color;
    }
    
    //Change Color of Icon Image
    private void ChangeIconColor(Color color) 
    {
        JoyIconLock.color = color;
        useLeft_Icon.color = color;
        useRight_Icon.color = color;
        Button1_Icon.color = color;
        Button2_Icon.color = color;
        Button3_Icon.color = color;
        Button4_Icon.color = color;
    }

    //Set opacity from int.
    private void SetOpacity(int value) 
    {
        if((value * 2) >= 255)
        {
            text_opacity = 255;
        }
        else 
        {
            text_opacity = value * 2;
        }
        btn_opacity = value;
        if(JoyBkg.color.a != 0) 
        {
            ToggleVisible(true);
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

        Color color;
        Color textColor;
        if (value) 
        {
            color = new Color32(0, 0, 0, (byte)btn_opacity);
            textColor = new Color32(255, 255, 255, (byte)text_opacity);
        }
        else 
        {
            color = new Color32(255, 255, 255, 0);
            textColor = new Color32(255, 255, 255, 0);
        }
        ChangeImageColor(color);
        ChangeIconColor(textColor);
        cover.raycastTarget = !value;
    }
}
