using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepRot : MonoBehaviour
{
    Quaternion rotation;
    void Awake()
    {
        rotation = transform.rotation;
    }
    void LateUpdate()
    {
        transform.rotation = rotation;
    }
}
