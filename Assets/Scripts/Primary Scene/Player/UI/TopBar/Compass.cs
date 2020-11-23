
using MLAPI;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    public RawImage compass;
    private Transform playerRot;
    private float currentEulerY = 0;
    
    private void Start()
    {
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient)
        {
            playerRot = NetworkingManager.Singleton.ConnectedClients[NetworkingManager.Singleton.LocalClientId].PlayerObject.transform;
        }
        else 
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (playerRot != null && playerRot.localEulerAngles.y != currentEulerY) 
        {
            currentEulerY = playerRot.localEulerAngles.y;
            compass.uvRect = new Rect(currentEulerY / 360f, 0, 1, 1);
        }
    }
}
