using UnityEngine;

public class CollideSensor : MonoBehaviour
{
    int _overlaps;

    public bool isOverlapping
    {
        get
        {
            return _overlaps > 0;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        _overlaps++;
    }

    void OnTriggerExit(Collider other)
    {
        _overlaps--;
    }
}