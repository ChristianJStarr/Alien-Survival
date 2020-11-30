using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenuSlide : MonoBehaviour
{
    public TextMeshProUGUI mainText;
    public GameObject selection;
    public Image imageIcon;
    private DebugMenu debugMenu;

    public int id;
    public string playerName;
    public bool isItem = false;


    //Update Slide Values
    public void UpdateValues(string name, int newId, DebugMenu menu, Sprite icon = null) 
    {
        if(icon != null) 
        {
            imageIcon.gameObject.SetActive(true);
            imageIcon.sprite = icon;
        }
        else 
        {
            imageIcon.gameObject.SetActive(false);
        }
        debugMenu = menu;
        id = newId;
        playerName = name;
        mainText.text = id + " - " + name;
    }


    //Slide Clicked
    public void ButtonClick()
    {
        if (!isItem)
        {
            debugMenu.SelectPlayer(id);
        }
        else 
        {
            debugMenu.SelectItem(id);
        }
    }

    //Select Slide
    public void Selected(bool value)
    {
        if(selection != null)
            selection.SetActive(value);
    }
}
