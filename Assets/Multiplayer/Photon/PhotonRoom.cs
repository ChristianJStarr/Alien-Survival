using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static PhotonRoom room;
    private PhotonView PV;

    private PlayerLogin playerLogin;
    public int multiplayerScene;
    public int currentScene;

    public MainMenuScript mainMenuScript;
    Slider loadSlider;

    void Awake()
    {
        if (PhotonRoom.room == null)
        {
            PhotonRoom.room = this;
        }
        else
        {
            if (PhotonRoom.room != this)
            {
                Destroy(PhotonRoom.room.gameObject);
                PhotonRoom.room = this;
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        PV = GetComponent<PhotonView>();
        playerLogin = PlayerLogin.Instance();
    }

    public override void OnEnable() 
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Network - Joined Room");
        if (PhotonNetwork.IsMasterClient) 
        {
            Debug.Log("Removed 50 SP");
            playerLogin.RemoveCoin(50);
        }
        else 
        {
            Debug.Log("Removed 25 SP");
            playerLogin.RemoveCoin(25);
        }

        StartGame();
    }
    void StartGame() 
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        PhotonNetwork.LoadLevel(multiplayerScene);
    }
    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode) 
    {
        currentScene = scene.buildIndex;
        if (currentScene == multiplayerScene) 
        {
            CreatePlayer();   
        }
        if (currentScene == 0) 
        {
            PhotonNetwork.Disconnect();
            Destroy(this.gameObject);
        }

    }
    private void CreatePlayer() 
    {
        Transform spawn = GameObject.Find("SpawnPoint").transform;
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetworkPlayer"), spawn.position, Quaternion.identity, 0);
    }

}
