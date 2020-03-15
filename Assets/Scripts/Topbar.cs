using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Topbar : MonoBehaviour
{
    public TextMeshProUGUI networkStatus;


    void Start() 
    {
        IsConnectedText(PhotonNetwork.InRoom);
    }


    private void IsConnectedText(bool value)
    {
        if (value)
        {
            networkStatus.text = "CONNECTED:" + PhotonNetwork.CurrentRoom.Name + " (" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + ")";
            networkStatus.color = new Color32(52, 255, 79, 255);
        }
        else
        {
            networkStatus.text = "NOT CONNECTED";
            networkStatus.color = new Color32(255, 52, 52, 255);
        }
    }

}
