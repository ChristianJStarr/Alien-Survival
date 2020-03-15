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

    public override void OnRoomListUpdate(List<RoomInfo> roomList) 
    {
        Debug.Log("Netwok - RoomList Update Called. Number of Rooms: " + roomList.Count);
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            Debug.Log(info);
            ServerSlideScript roomListing = Instantiate(_roomListing, _content);
            if (roomListing != null)
            {
                roomListing.SetRoomInfo(info);
            }
            
        }
        _serverAmount.text = "SERVERS (" + roomList.Count + "/" + roomList.Count + ")";
    }    

}
