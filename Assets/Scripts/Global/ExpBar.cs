using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpBar : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public Slider slider;
    public int exp = 0;
    public void SetExp(int value)
    {
        int level = GetLevel(value);
        levelText.text = "LEVEL " + level;

        slider.value = (value - (level * 1000)) / 10;
        exp = value;
    }
    public int GetLevel(int value)
    {
        value = value / 1000;

        if (value <= 0)
        {
            return 1;
        }
        else
        {
            return value;
        }
    }
}
