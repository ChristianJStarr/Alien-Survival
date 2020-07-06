using TMPro;
using UnityEngine;

public class AlienStoreSlide : MonoBehaviour
{
    public GameObject lockCover, pointObject;


    public void SetSlide(bool isLocked) 
    {
        lockCover.SetActive(isLocked);
    }

}
