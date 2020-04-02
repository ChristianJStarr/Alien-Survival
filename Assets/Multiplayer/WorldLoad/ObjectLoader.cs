using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ObjectLoader : MonoBehaviour
{
    private LoadPlayer loadPlayer;
    void Start() 
    {
        loadPlayer = GetComponent<LoadPlayer>();
    }
    public void JsonToAllObjects(string json) 
    {
        ObjectData[] tempData = JsonHelper.FromJson<ObjectData>(json);
        PlaceWorldObjects(tempData.OfType<ObjectData>().ToList());
    }
    public string AllObjectsToJson() 
    {
        return JsonHelper.ToJson(GetWorldObjects().ToArray());
    }
    public List<ObjectData> GetWorldObjects() 
    {
        List<ObjectData> objList = new List<ObjectData>();
        Persistant[] allObj = FindObjectsOfType<Persistant>();
        //Debug.Log("AutoSave - Found " + allObj.Length + " Persistant Object(s)");
        foreach (Persistant obj in allObj)
        {
            ObjectData objData = new ObjectData
            {
                objectId = obj.objectId,
                objectLoc = obj.objectTrans.position,
                objectRot = obj.objectTrans.rotation,
                objectState = obj.objectState,
                tip = obj.tip
            };
            objList.Add(objData);
        }
        return objList;
    }
    public void PlaceWorldObjects(List<ObjectData> objList) 
    {
        foreach (ObjectData objData in objList)
        {
            GameObject obj = PhotonNetwork.Instantiate(Path.Combine("Persistant", "object-" + objData.objectId), objData.objectLoc, objData.objectRot, 0);
            obj.GetComponent<Persistant>().objectState = objData.objectState;
            obj.GetComponent<Persistant>().tip = objData.tip;
        }
        loadPlayer.DoneLoading(true);
    }
}
[Serializable]
public class ObjectData 
{
    public int objectId;
    public Vector3 objectLoc;
    public Quaternion objectRot;
    public string objectState;
    public string tip;
}
