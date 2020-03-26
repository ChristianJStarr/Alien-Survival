using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerControllerMulti : MonoBehaviour
{
    public FirstPersonController fps;
    public PhotonView PV;
    public GameObject playerCamera;
    public bool isMine = false;

    void Start() 
    {
        if (!PV.IsMine) 
        {
            isMine = false;
            Destroy(fps);
            Destroy(playerCamera);
        }
        else 
        {
            isMine = true;
            FindObjectOfType<Compass>().SetPlayer(this.gameObject);
            FindObjectOfType<InventoryScript>().SetPlayer(fps);
        }
    }
}
