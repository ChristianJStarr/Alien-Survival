using UnityEngine;
using System;

[CreateAssetMenu(fileName = "HoldableData_1", menuName = "ScriptableObjects/HoldableData")]
[Serializable]
public class HoldableObjectData : ScriptableObject
{
    [SerializeField] public int holdableId;
    [SerializeField] public GameObject prefab;
}