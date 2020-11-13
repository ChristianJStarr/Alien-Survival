using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopToolTypeSlide : MonoBehaviour
{
    public Image background;
    public TextMeshProUGUI typeText;

    public void UpdateType(int type, int amount) 
    {
        string prefix = "";
        string typeName = "";
        if(type == 1) 
        {
            typeName = " Water";
            prefix = "+";
            background.color = new Color32(116, 243, 227, 136);
        }
        if (type == 2)
        {
            typeName = " Food";
            prefix = "+";
            background.color = new Color32(243, 194, 116, 136);
        }
        if (type == 3)
        {
            typeName = " Health";
            prefix = "+";
            background.color = new Color32(243, 126, 116, 136);
        }
        if (type == 4)
        {
            typeName = " Health";
            background.color = new Color32(243, 126, 116, 136);
            prefix = "-";
        }

        typeText.text = prefix + amount.ToString() + typeName;
    }
}
