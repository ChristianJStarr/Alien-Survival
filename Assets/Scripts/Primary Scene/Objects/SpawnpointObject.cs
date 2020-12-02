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

    void OnDrawGizmos()
    {
        // Draws the Light bulb icon at position of the object.
        // Because we draw it inside OnDrawGizmos the icon is also pickable
        // in the scene view.
        if(spawn_type == 0) 
        {
            Gizmos.DrawIcon(transform.position, "winbtn_mac_inact@2x", true);
        }
        else if(spawn_type == 1)
        {
            Gizmos.DrawIcon(transform.position, "winbtn_mac_max@2x", true);
        }
        else if (spawn_type == 2)
        {
            Gizmos.DrawIcon(transform.position, "winbtn_mac_min@2x", true);
        }
        else if (spawn_type == 3)
        {
            Gizmos.DrawIcon(transform.position, "winbtn_mac_close@2x", true);
        }
    }
}
