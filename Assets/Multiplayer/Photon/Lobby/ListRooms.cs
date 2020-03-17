using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ListRooms : MonoBehaviourPunCallbacks
{
    public Transform _content;

    public ServerSlideScript _roomListing;
    public TextMeshProUGUI _serverAmount;
    public GameObject serverItem;

    private ServerSlideScript[] _roomListings;

    public override void OnRoomListUpdate(List<RoomInfo> roomList) 
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (!info.RemovedFromList)
            {
                ServerSlideScript roomListing = Instantiate(_roomListing, _content);
                if (roomListing != null)
                {
                    roomListing.SetRoomInfo(info);
                }
            }     
        }
        
        UpdateLists(roomList);
    }
    public void UpdateLists(List<RoomInfo> roomList) 
    {
        _roomListings = serverItem.GetComponentsInChildren<ServerSlideScript>();
        for (int i = 0; i < _roomListings.Length; i++)
        {
            for (int a = 0; a < roomList.Count; a++)
            {
                if (_roomListings[i].roomNameJoin == roomList[a].Name) 
                {
                    if (roomList[a].RemovedFromList) 
                    {
                        _roomListings[i].roomExists = false;
                    }
                    else 
                    {
                        _roomListings[i].roomExists = true;
                    }
                }
            }
        }
        int servers = 0;
        for (int i = 0; i < _roomListings.Length; i++)
        {
            if (_roomListings[i].roomExists == false) 
            {
                Destroy(_roomListings[i].gameObject);
                _roomListings[i] = null;
            }
            else
            {
                servers++;
            }
        }
        if (_serverAmount.text != "") 
        {
            _serverAmount.text = "SEARCH...  (" + servers + "/" + servers + ")";
        }
        
    }
}
