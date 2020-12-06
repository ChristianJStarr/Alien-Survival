using System;
using UnityEngine;

[Serializable]
public class SpawnpointObject : MonoBehaviour
{
    [SerializeField] public WorldObject worldObject;
    [SerializeField] public GameObject worldGameObject;
    [SerializeField] public int spawn_objectId;
    [SerializeField] public int spawn_id;
    [SerializeField] public int spawn_type = 0;
    [SerializeField] public int spawn_level = 0;
    [SerializeField] public float lastSpawntime;
    [SerializeField] public bool isNearPlayer = false;


    //void OnDrawGizmos()
    //{
    //    if (spawn_objectId == 1)
    //    {
    //        Gizmos.DrawIcon(transform.position, "sv_icon_dot11_pix16_gizmo", true);
    //    }
    //    else if(spawn_objectId == 2)
    //    {
    //        Gizmos.DrawIcon(transform.position, "sv_icon_dot12_pix16_gizmo", true);
    //    }
    //    else if (spawn_objectId == 3)
    //    {
    //        Gizmos.DrawIcon(transform.position, "sv_icon_dot14_pix16_gizmo", true);
    //    }
    //    else if (spawn_objectId == 4)
    //    {
    //        Gizmos.DrawIcon(transform.position, "sv_icon_dot15_pix16_gizmo", true);
    //    }
    //    else if (spawn_objectId == 5)
    //    {
    //        Gizmos.DrawIcon(transform.position, "sv_icon_dot10_pix16_gizmo", true);
    //    }
    //}
}
