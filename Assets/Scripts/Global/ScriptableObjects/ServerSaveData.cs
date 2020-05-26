using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ServerSaveData", menuName = "ScriptableObjects/ServerSaveData", order = 1)]
public class ServerSaveData : ScriptableObject
{
    public List<PlayerInfo> playerData;
}
