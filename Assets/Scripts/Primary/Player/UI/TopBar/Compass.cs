
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{

    public RawImage compass;
    private Transform playerRot;

    public void SetPlayer(GameObject player) 
    {
        playerRot = player.transform;
    }

    void Update()
    {
        if (playerRot != null) 
        {
            compass.uvRect = new Rect(playerRot.localEulerAngles.y / 360f, 0, 1, 1);
        }
    }
}
