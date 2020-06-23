using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRotater : MonoBehaviour
{
    public RectTransform rect;
    public int rotation = -5;
    private Vector3 rotate;
    private void Start()
    {
        rotate = new Vector3(0,0, rotation);
    }

    private void Update()
    {
        rect.Rotate(rotate);
    }
}
