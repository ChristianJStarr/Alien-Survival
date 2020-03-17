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
    public ServerCoins serverCoins;

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
        Debug.Log("Creating Room");
        if (nameBox.text.Length > 5 && serverCoins.CanRemoveCoin(50)) 
        {
            Debug.Log("Photon creating room");
            photonLobby.CreateNewRoom(nameBox.text);
            nameBox.text = "";
        }
    }
}
