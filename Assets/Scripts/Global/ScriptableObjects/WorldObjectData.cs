using System;
using UnityEngine;


[CreateAssetMenu(fileName = "WorldObjectData_1", menuName = "ScriptableObjects/WorldObjectData")]
[Serializable]
public class WorldObjectData : ScriptableObject
{
    [SerializeField]public int objectId = 0;
    [SerializeField] public int objectType = 0;
    [SerializeField] public int objectLevel = 0;
    [SerializeField] public GameObject objectPrefab;
    //Gather Specs
    [SerializeField] public int gatherItemId = 0; //Gather Item ID
    [SerializeField] public int gatherAmount = 0; //Per Gather Amount
    [SerializeField] public int toolId;//Tool to Gather
}
