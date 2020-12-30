using UnityEngine;

public class MainMenuWelcomeMessage : MonoBehaviour
{
    public GameObject messageScreen;
    public MainMenuScript mainMenu;
    public GameObject[] messages;
    public PlayerStats playerStats;

    private int[] dontAutoOpen = new int[] { 2 };

    private void Start()
    {
        if(PlayerPrefs.GetInt("newPlayer") == 1) 
        {
            PlayerPrefs.DeleteKey("newPlayer");
            PlayerPrefs.Save();
            messageScreen.SetActive(true);
            mainMenu.CloseAll();
            messages[0].SetActive(true);
        }
    }

    public void CloseActive() 
    {
        for (int i = 0; i < messages.Length; i++)
        {
            if (messages[i].activeSelf) 
            {
                messages[i].SetActive(false);
                SetNext(i);
                break;
            }
        }
    }
    private void SetNext(int value) 
    {
        value++;
        bool autoOpenSlide = true;
        for (int i = 0; i < dontAutoOpen.Length; i++)
        {
            if (value == dontAutoOpen[i])
            {
                autoOpenSlide = false;
                break;
            }
        }
        if (autoOpenSlide && value < messages.Length) 
        {
            messages[value].SetActive(true);
        }
        else 
        {
            messageScreen.SetActive(false);
        }
    }

    public void ShowNotify() 
    {
        mainMenu.CloseAll();
        messageScreen.SetActive(true);
        for (int i = 0; i < messages.Length; i++)
        {
            if (messages[i].activeSelf)
            {
                messages[i].SetActive(false);
            }
        }
        messages[2].SetActive(true);
    }
}
