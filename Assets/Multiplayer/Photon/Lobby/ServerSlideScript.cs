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
    [SerializeField]
    private TextMeshProUGUI roomPing;
    PlayerLogin playerLogin;
    public GameObject roomNotify;
    public string roomNameJoin;
    public MainMenuScript mainMenu;

    void Start() 
    {
        int ping = 100;
        if (PhotonNetwork.GetPing() != 0)
        {
            ping = PhotonNetwork.GetPing();
        }
        playerLogin = PlayerLogin.Instance();
        mainMenu = FindObjectOfType<MainMenuScript>();
        roomMode.text = "Normal Mode V1.13";
        roomPing.text = Random.Range(ping - 3, ping + 3) + "ms";
        roomPlayers.text = "(0/20)";
    }
    public void UpdateRoomInfo(RoomInfo roomInfo) 
    {
        int ping = 100;
        if (PhotonNetwork.GetPing() != 0)
        {
            ping = PhotonNetwork.GetPing();
        }
        roomPing.text = Random.Range(ping - 3, ping + 3) + "ms";
        roomPlayers.text = "(" + roomInfo.PlayerCount + "/20)";
    }
    public void UpdateEmpty() 
    {
        int ping = 100;
        if(PhotonNetwork.GetPing() != 0) 
        {
            ping = PhotonNetwork.GetPing();
        }
        roomName.text = roomNameJoin;
        roomPing.text = Random.Range(ping - 3, ping + 3) + "ms";
        roomPlayers.text = "(0/20)";
    }
    public void JoinThisRoom() 
    {   
        if (roomNameJoin != null && playerLogin.CanRemoveCoin(25)) 
        {
            mainMenu.LoadGame();
            RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 20 };
            //Debug.Log("Network - Joining Room: " + roomNameJoin);
            PhotonNetwork.JoinOrCreateRoom(roomNameJoin, roomOps, TypedLobby.Default);
        }
    }
}
