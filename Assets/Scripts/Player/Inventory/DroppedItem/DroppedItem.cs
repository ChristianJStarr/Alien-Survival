using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DroppedItem : MonoBehaviour, IPunObservable
{
    public Persistant persistant;
    public int itemId = 0;
    public int itemStack = 0;
    public string itemSpecial = "";
    public string itemName = "";
    public string tip = "";
    void Start() 
    {
        if (!String.IsNullOrEmpty(persistant.objectState)) 
        {
            string[] datas = persistant.objectState.Split('R');
            itemId = Convert.ToInt32(datas[0]);
            itemStack = Convert.ToInt32(datas[1]);
            itemSpecial = datas[2];
            itemName = datas[3];
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(itemId);
            stream.SendNext(itemStack);
            stream.SendNext(itemSpecial);
            stream.SendNext(itemName);
            stream.SendNext(tip);
        }
        else
        {
            this.itemId = (int)stream.ReceiveNext();
            this.itemStack = (int)stream.ReceiveNext();
            this.itemSpecial = (string)stream.ReceiveNext();
            this.itemName = (string)stream.ReceiveNext();
            this.tip = (string)stream.ReceiveNext();
        }
    }

    public void Set(string name, int id, int stack, string special) 
    {
        itemName = name;
        itemId = id;
        itemStack = stack;
        itemSpecial = special;
        tip = name + " (" + stack + ")";
        persistant.tip = tip;
        string data = id + "R" + stack + "R" + special + "R" + name;
        persistant.objectState = data;
    }

}
