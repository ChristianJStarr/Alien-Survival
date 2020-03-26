using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateRoom : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI nameBox;
    public PhotonLobby photonLobby;
    public PlayerLogin playerLogin;


    public void UpdateBox() 
    {
        if (nameBox.text.Length > 5) 
        {
            button.interactable = true;
        }
        else if(button.interactable == true) 
        {
            button.interactable = false;
        }
    }
    public void CreateARoom() 
    {
        if (nameBox.text.Length > 5 && playerLogin.CanRemoveCoin(50)) 
        {
            photonLobby.CreateNewRoom(nameBox.text);
            nameBox.text = "";
        }
    }
}
