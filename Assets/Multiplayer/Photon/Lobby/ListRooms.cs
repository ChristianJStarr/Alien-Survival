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

    private int amountOfRefresh = 0;


    public override void OnRoomListUpdate(List<RoomInfo> roomList) 
    {
        base.OnRoomListUpdate(roomList);
        UpdateLists(roomList);
    }

    public void UpdateLists(List<RoomInfo> roomList)
    {
        List<RoomInfo> _addQ = new List<RoomInfo>();
        List<RoomInfo> _dontQ = new List<RoomInfo>();
        List<ServerSlideScript> _slideList = new List<ServerSlideScript>(serverItem.GetComponentsInChildren<ServerSlideScript>(true));
        Debug.Log(roomList.Count + "    " + _slideList.Count);
        foreach (RoomInfo info in roomList) 
        {
            if (_slideList.Count == 0) 
            {
                if (!info.RemovedFromList) 
                {
                    _addQ.Add(info);
                }
            }
            else 
            {
                foreach (ServerSlideScript slide in _slideList)
                {
                    if (!info.RemovedFromList)
                    {
                        if (slide.roomNameJoin == info.Name)
                        {
                            _dontQ.Add(info);

                            if (slide.roomCount == info.PlayerCount)
                            {
                                Debug.Log(info.Name + " slide exists. has proper count, does not need to be removed");
                            }
                            else if (slide.roomCount != info.PlayerCount)
                            {
                                Debug.Log(info.Name + " slide exists. does not have proper count.");
                                slide.SetRoomInfo(info);
                            }
                        }
                        else
                        {
                            if (!_addQ.Contains(info))
                            {
                                _addQ.Add(info);
                            }
                        }
                    }
                    else
                    {
                        if (slide.roomNameJoin == info.Name)
                        {
                            Debug.Log("Destroying room slide " + slide.roomNameJoin);
                            Destroy(slide.gameObject);
                        }
                    }
                }
            }
        }   
        if (_dontQ != null) 
        {
            foreach (RoomInfo room in _dontQ)
            {
                if (_addQ.Contains(room))
                {
                    _addQ.Remove(room);
                }
            }
        }
        if (_addQ != null) 
        {
            foreach (RoomInfo que in _addQ)
            {
                ServerSlideScript roomListing = Instantiate(_roomListing, _content);
                if (roomListing != null)
                {
                    roomListing.SetRoomInfo(que);
                }
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
