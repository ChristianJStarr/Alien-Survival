using System;
using UnityEngine;

[Serializable]
public class SpawnpointObject : MonoBehaviour
{
    [SerializeField] public GameObject worldObject;
    public int spawn_objectId;
    [SerializeField] public int spawn_id;
    [SerializeField] public int spawn_type = 0;
    [SerializeField] public int spawn_level = 0;
    [SerializeField] public float lastSpawntime;
    public bool isNearPlayer = false;



    void OnDrawGizmos()
    {
        if (isNearPlayer)
        {
            Gizmos.DrawIcon(transform.position, "winbtn_mac_max@2x", true);
        }
        else
        {
            Gizmos.DrawIcon(transform.position, "winbtn_mac_close@2x", true);
        }
    }
}
