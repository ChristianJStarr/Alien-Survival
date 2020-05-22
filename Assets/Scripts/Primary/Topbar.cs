using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Topbar : MonoBehaviour
{
    private int userExp, userHealth, userWater, userFood;
    public Slider userExp_slider, userHealth_slider, userWater_slider, userFood_slider;
    public TextMeshProUGUI userExp_text, userHealth_text, pingText;
    public PlayerStats playerStats;
    public RawImage pingImage;
    public bool showPing = false;


    void Update() 
    {
        UpdateUIBar();
    }

    void UpdateUIBar() 
    {
        userExp = playerStats.playerExp;
        userHealth = playerStats.playerHealth;
        userWater = playerStats.playerWater;
        userFood = playerStats.playerFood;

        int level = GetLevel(userExp);

        userExp_slider.value = (userExp - (level * 1000)) / 10; ;
        userHealth_slider.value = userHealth;
        userWater_slider.value = userWater;
        userFood_slider.value = userFood;

        userExp_text.text = "LEVEL " + level;
        userHealth_text.text = "HP " + userHealth;
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
