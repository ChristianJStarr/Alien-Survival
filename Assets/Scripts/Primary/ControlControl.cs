using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlControl : MonoBehaviour
{
    //What: UI Controls Controller
    //Where: Primary Scene / Interface

    public GameObject cover; //A transparent button cover.
    public GameObject leftScreen;//Left screen container.
    public GameObject rightScreen;//Right screen container.
    public Settings settings;//Current game settings.

    private Image[] left; //All image components in leftScreen.
    private Image[] right; //All image components in rightScreen.
    private int btn_opacity; //Button Opacity
    private int text_opacity; //Button Text Opacity

    public Image JoyBkg;
    public Image JoyStick;
    public Image UseLeft;
    public Image UseRight;
    public Image Button1;
    public Image Button2;
    public Image Button3;
    public Image Button4;
    public Image Button5;


    void Start() 
    {
        left = leftScreen.GetComponentsInChildren<Image>(); //Get all Image components in leftScreen.
        right = rightScreen.GetComponentsInChildren<Image>(); //Get all Image components in rightScreen.
        Change();
    }

    //Change Button Opacity. Standard settingsMenu Change().
    public void Change()
    {
        //Set opacity to stored settings. 0-255.
        SetOpacity(settings.gameControlsOpacity); 
    }

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
        Button5.color = color;
    }
    private void ChangeTextColor(Color color) 
    {
    
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
        ChangeTextColor(textColor);
        cover.GetComponent<Image>().raycastTarget = !value;
    }
}
