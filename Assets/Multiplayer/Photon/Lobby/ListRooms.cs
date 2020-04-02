using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ListRooms : MonoBehaviourPunCallbacks
{
    public Transform _content;
    public ServerSlideScript _roomListing;
    public TextMeshProUGUI _serverAmount;
    public GameObject serverItem;
    public Button refreshButton;
    public ServerSlideScript[] slideList;
    private int amountOfRefresh = 0;

    void Start() 
    {
        slideList = serverItem.GetComponentsInChildren<ServerSlideScript>();
        for (int i = 0; i < slideList.Length; i++)
        {
            slideList[i].roomNameJoin = "Community Server  #" + (i + 1);
            slideList[i].UpdateEmpty();
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) 
    {
        base.OnRoomListUpdate(roomList);
        UpdateLists(roomList);
    }

    public ServerSlideScript GetSlide(string roomName) 
    {
        ServerSlideScript slide = null;
        foreach (ServerSlideScript stored in slideList)
        {
            if (stored.roomNameJoin == roomName) 
            {
                slide = stored;
                break;
            }
        }
        return slide;
    }

    public void UpdateSlide(RoomInfo info) 
    {
        foreach (ServerSlideScript slide in slideList)
        {
            if(slide.roomNameJoin == info.Name) 
            {
                slide.UpdateRoomInfo(info);
                break;
            }
        }
    }

    public void UpdateLists(List<RoomInfo> roomList)
    {
        foreach (ServerSlideScript slide in slideList)
        {
            slide.UpdateEmpty();
        }
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                GetSlide(info.Name).UpdateEmpty();
            }
            else
            {
                UpdateSlide(info);
            }
        }
    }

    public void RefreshRoomList()
    {
        if (amountOfRefresh < 1)
        {
            PhotonNetwork.JoinLobby();
            amountOfRefresh++;
        }
        else
        {
            refreshButton.interactable = false;
            refreshButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color32(163, 163, 163, 255);
            StartCoroutine(RefreshCooldown());
        }
    }
    IEnumerator RefreshCooldown()
    {
        yield return new WaitForSeconds(5f);
        amountOfRefresh = 0;
        refreshButton.interactable = true;
        refreshButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255);
    }
}
