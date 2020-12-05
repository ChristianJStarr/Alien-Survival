using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Topbar : MonoBehaviour
{
    public Slider userHealth_slider, userWater_slider, userFood_slider;
    public TextMeshProUGUI  userHealth_text, userWater_text, userFood_text;
    public UI_ExpBar expBar;


    //Update player info
    public void Incoming(int health, int exp, int water, int food) 
    {
        userHealth_slider.value = health;
        userWater_slider.value = water;
        userFood_slider.value = food;
        expBar.SetExp(exp);
        userHealth_text.text = "HP " + health;
        userWater_text.text = "WATER " + water;
        userFood_text.text = "FOOD " + food;
    }


    public void Show() 
    {
        gameObject.SetActive(true);
    }
    public void Hide() 
    {
        gameObject.SetActive(false);
    }
}
