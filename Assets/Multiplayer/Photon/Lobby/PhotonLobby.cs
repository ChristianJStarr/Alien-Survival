using Photon.Realtime;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PhotonLobby : MonoBehaviourPunCallbacks
{

    public static PhotonLobby lobby;
    public PlayerStats playerStats;
    public TextMeshProUGUI isConnectedText;
    

    private string roomExact;
    

    private void Awake() 
    {
        lobby = this;
    }

    void Start()
    {
        IsConnectedText(false);
        if(!PhotonNetwork.IsConnected)
        PhotonNetwork.ConnectUsingSettings();  
    }
    
    public override void OnConnectedToMaster()
    {
        //Debug.Log("Network - Connected to Master State: " + PhotonNetwork.NetworkClientState);
        base.OnConnectedToMaster();
        PhotonNetwork.AutomaticallySyncScene = true;
        IsConnectedText(true);
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        PhotonNetwork.NickName = playerStats.playerName;
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
        //Debug.Log("Network - Joined Lobby State: " + PhotonNetwork.NetworkClientState);
        base.OnJoinedLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //Debug.Log("Network - Disconnected State: " + PhotonNetwork.NetworkClientState);
        base.OnDisconnected(cause);

        IsConnectedText(false);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Network - Failed Creating New Room State: " + PhotonNetwork.NetworkClientState);
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        //Debug.Log("Network - Created Room State: " + PhotonNetwork.NetworkClientState);
        PhotonNetwork.LeaveLobby();
       // Debug.Log("Network - Leaving Lobby: " + PhotonNetwork.NetworkClientState);
    }
    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        //Debug.Log("Network - Left Lobby: " + PhotonNetwork.NetworkClientState);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Unable to join room State: " + PhotonNetwork.NetworkClientState);
    } 
}