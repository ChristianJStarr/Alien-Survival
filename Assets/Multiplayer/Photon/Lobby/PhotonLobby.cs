using Photon.Realtime;
using Photon.Pun;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonLobby : MonoBehaviourPunCallbacks
{

    public static PhotonLobby lobby;
    public TextMeshProUGUI isConnectedText;
    public ServerCoins serverCoins;
    private string roomExact;

    private void Awake() 
    {
        lobby = this;
    }

    void Start()
    {
        IsConnectedText(false);
        PhotonNetwork.ConnectUsingSettings();  
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Network - Connected to Master State: " + PhotonNetwork.NetworkClientState);
        base.OnConnectedToMaster();

        IsConnectedText(true);
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }
    private void IsConnectedText(bool value) 
    {
        if (value) 
        {
            isConnectedText.text = "CONNECTED";
            isConnectedText.color = new Color32(52, 255, 79, 255);
        }
        else 
        {
            isConnectedText.text = "NOT CONNECTED";
            isConnectedText.color = new Color32(255, 52, 52, 255);
        }
    }

    
    public override void OnJoinedLobby()
    {
        Debug.Log("Network - Joined Lobby State: " + PhotonNetwork.NetworkClientState);
        base.OnJoinedLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Network - Disconnected State: " + PhotonNetwork.NetworkClientState);
        base.OnDisconnected(cause);

        IsConnectedText(false);
    }


    public void CreateNewRoom(string roomName) 
    {
        string name = roomName + "-" + Random.Range(1000, 10000);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 10 };
        PhotonNetwork.CreateRoom(name, roomOps);
        Debug.Log("Network - Creating New Room State: " + PhotonNetwork.NetworkClientState);
        roomExact = name;
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Network - Failed Creating New Room State: " + PhotonNetwork.NetworkClientState);
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Network - Created Room State: " + PhotonNetwork.NetworkClientState);
        PhotonNetwork.LeaveLobby();
        Debug.Log("Network - Leaving Lobby: " + PhotonNetwork.NetworkClientState);
    }
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        Debug.Log("Network - Left Lobby: " + PhotonNetwork.NetworkClientState);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Unable to join room State: " + PhotonNetwork.NetworkClientState);
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Network - Joined Room");
        SceneManager.LoadScene(1);
    }
}
