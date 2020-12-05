using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopToolTypeSlide : MonoBehaviour
{
    public Image background; // slide bkg
    public TextMeshProUGUI typeText; // slide text

    //Colors
    Color32 color_blue = new Color32(116, 243, 227, 136);
    Color32 color_orange = new Color32(243, 194, 116, 136);
    Color32 color_red = new Color32(243, 126, 116, 136);

    //Update this slides Type
    public void UpdateType(int type, int amount) 
    {
        if (type == 1)
        {
            SetValues("+", " Water", amount);
            background.color = color_blue;
        }
        else if (type == 2)
        {
            SetValues("+", " Food", amount);
            background.color = color_orange;
        }
        else if (type == 3)
        {
            SetValues("+", " Health", amount);
            background.color = color_red;
        }
        else if (type == 4)
        {
            SetValues("-", " Health", amount);
            background.color = color_red;
        }
    }

    //Set values to type text.
    private void SetValues(string prefix, string name, float amount) 
    {
        typeText.text = prefix + amount + name;
    }
}
