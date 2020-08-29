using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WorldObjectData_1", menuName = "ScriptableObjects/WorldObjectData")]
public class WorldObjectData : ScriptableObject
{
    public string objectName = "";
    public int objectId = 0;


    //Gather Specs
    public int gatherItemId = 0;
    public int gatherAmount = 0;
    public int gatherTotal = 0;

    //Tool to Gather
    public int toolId;

    //Respawn
    public int respawnTime = 0;
}
