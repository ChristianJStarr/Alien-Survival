using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ExpBar : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public Slider slider;
    


    public void SetExp(int value)
    {
        int level = GetLevelFromExp(value);
        levelText.text = "LEVEL " + level;
        int min = level - 1;
        min *= min * 100;
        int max = (level * level) * 100;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
    }
    

    public static int GetLevelFromExp(int exp) 
    {
        exp /= 100;
        exp = (int) Mathf.Sqrt(exp);
        exp += 1;
        return exp;
    }
}
