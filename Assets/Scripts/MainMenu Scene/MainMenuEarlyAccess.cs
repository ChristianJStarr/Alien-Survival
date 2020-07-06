using TMPro;
using UnityEngine;

public class MainMenuEarlyAccess : MonoBehaviour
{
    public TextMeshProUGUI messageText;

    private void Start()
    {
        string username = PlayerPrefs.GetString("username");
        if (username.StartsWith("Guest")) 
        {
            username = "Guest";
        }
        messageText.text = "Welcome " + username + "! Alien Survival is in Early Access, meaning we're actively working on it based on our plans and your feedback. You will experience bugs, unfinished features, problematic design decision, and many more things that disrupt your game experience.  Stay updated with recent changes and provide critical feedback at <color=#DD7070> aliensurvival.com</color>. ";
    }
}
