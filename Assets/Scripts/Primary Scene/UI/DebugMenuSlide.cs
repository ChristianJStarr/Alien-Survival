using TMPro;
using UnityEngine;

public class DebugMenuSlide : MonoBehaviour
{
    public TextMeshProUGUI mainText;
    public GameObject selection;
    private DebugMenu debugMenu;
    public int id;
    public string playerName;
    public bool isItem = false;


    //Update Slide Values
    public void UpdateValues(string name, int newId, DebugMenu menu) 
    {
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
