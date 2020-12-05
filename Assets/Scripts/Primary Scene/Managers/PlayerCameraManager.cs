using MLAPI;
using System.Collections;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    public GameObject playerCamera;
    public GameObject playerViewCamera;

    private void Start()
    {
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient)
        {
            if (!SpawnCameras()) 
            {
                StartCoroutine(WaitForPlayer());
            }
        }
    }


    private IEnumerator WaitForPlayer() 
    {
        WaitForSeconds wait = new WaitForSeconds(1F);
        while (true) 
        {
            yield return wait;
            if (SpawnCameras()) break;
        }
    }

    private bool SpawnCameras() 
    {
        PlayerControlObject playerObject = WorldSnapshotManager.Singleton.GetLocalPlayerObject();
        if (playerObject == null) return false;
        PlayerCameraObject playerMount = playerObject.GetComponent<PlayerCameraObject>();
        if (playerMount == null) return false;
        if (!playerMount.camerasSpawned) 
        {
            Instantiate(playerCamera, playerMount.mainCameraSpawnAnchor);
            Instantiate(playerViewCamera, playerMount.viewCameraSpawnAnchor);
            playerMount.deathCam.enabled = true;
            playerMount.camerasSpawned = true;
        }
        if (LoadAwake.Singleton == null) return false;
        LoadAwake.Singleton.playerHasCamera = true;
        return true;
    }
}
