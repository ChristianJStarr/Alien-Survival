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

    public PlayerStats playerStats;
    private PlayerLogin playerLogin;
    public int multiplayerScene;
    public bool isLeaving = false;
    public int currentScene;
    private bool canCreate = false;
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
        playerLogin.RemoveCoin(25);
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
            isLeaving = true;
        }
        
    }
    public void LeaveGame() 
    {
        Destroy(gameObject);
    }
}
