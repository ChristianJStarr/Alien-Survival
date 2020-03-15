using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class ServerSlideScript : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TextMeshProUGUI roomName;
    [SerializeField]
    private TextMeshProUGUI roomMode;
    [SerializeField]
    private TextMeshProUGUI roomPlayers;
    ServerCoins serverCoins;
    public string roomNameJoin;

    void Start() 
    {
        serverCoins = ServerCoins.Instance();
    }

    public void SetRoomInfo(RoomInfo roomInfo) 
    {
        roomName.text = roomInfo.Name.Substring(0, roomInfo.Name.IndexOf("-")).Trim();
        roomMode.text = "Normal Mode V1.13";
        roomPlayers.text = "(" + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers + ")";
        roomNameJoin = roomInfo.Name;
    }
    public void JoinThisRoom() 
    {
        if (roomNameJoin != null && serverCoins.RemoveCoin(25)) 
        {
            Debug.Log("Network - Joining Room: " + roomNameJoin);
            PhotonNetwork.JoinRoom(roomNameJoin);
        }
    }
    
}
