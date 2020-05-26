
using MLAPI;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class Compass : MonoBehaviour
{

    public RawImage compass;
    private Transform playerRot;
    private bool client;
    private void Start()
    {
        client = NetworkingManager.Singleton.IsClient;
        if (client) 
        {
            playerRot = FindObjectOfType<FirstPersonController>().transform;
        }
       
    }

    private void Update()
    {
        if (client && playerRot != null) 
        {
            compass.uvRect = new Rect(playerRot.localEulerAngles.y / 360f, 0, 1, 1);
        }
    }
}
