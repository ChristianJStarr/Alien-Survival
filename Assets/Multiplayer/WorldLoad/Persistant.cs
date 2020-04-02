using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Persistant : MonoBehaviour, IPunObservable
{
    public int objectId;
    public Transform objectTrans;
    public string objectState;
    public string tip = "";

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) 
        {
            stream.SendNext(objectId);
            stream.SendNext(objectState);
            stream.SendNext(tip);
        }
        else 
        {
            this.objectId = (int)stream.ReceiveNext();
            this.objectState = (string)stream.ReceiveNext();
            this.tip = (string)stream.ReceiveNext();
        }
    }

    void Start() 
    {
        objectTrans = GetComponent<Transform>();
    }
}
