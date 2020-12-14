using MLAPI;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerCameraManager : MonoBehaviour
{
    public GameObject playerCamera;
    public GameObject playerViewCamera;
    public Settings settings;
    private bool firstPersonMode = true;
    private Vector3 fp_localTarget = new Vector3(0, 0.167F, -0.02F);
    private Vector3 tp_localTarget = new Vector3(0.191F, 0.545F, -1.149F);

    //Configuration
    private float c_CameraLerpSpeed = 0.5F;

    private Transform cameraMovementAnchor;

    #region OnSettingsChanged
    private void OnEnable() 
    {
        SettingsMenu.ChangedSettings += Change;
    }
    private void OnDisable()
    {
        SettingsMenu.ChangedSettings -= Change;
    }
    private void Change() 
    {
        ChangeCamera(settings.firstPersonMode);
    }
    #endregion



    Vector2 touchInput;

    private void Update() 
    {
        touchInput = new Vector2(CrossPlatformInputManager.GetAxis("MouseX"), CrossPlatformInputManager.GetAxis("MouseY"));
    }




















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
            if (settings.firstPersonMode) 
            {
                playerMount.mainCameraSpawnAnchor.localPosition = fp_localTarget;
                firstPersonMode = true;
            }
            else 
            {
                playerMount.mainCameraSpawnAnchor.localPosition = tp_localTarget;
                firstPersonMode = false;
            }
            cameraMovementAnchor = playerMount.mainCameraSpawnAnchor;
            Instantiate(playerCamera, playerMount.mainCameraSpawnAnchor);
            Instantiate(playerViewCamera, playerMount.viewCameraSpawnAnchor);
            playerMount.deathCam.enabled = true;
            playerMount.camerasSpawned = true;
        }
        return true;
    }

    private void ChangeCamera(bool firstPerson)
    {
        if (firstPersonMode != firstPerson)
        {
            firstPersonMode = firstPerson;
            StartCoroutine(CameraLerp(c_CameraLerpSpeed));
        }
    }

    private IEnumerator CameraLerp(float duration)
    {
        float time = 0;
        Vector3 startPosition = cameraMovementAnchor.localPosition;
        Vector3 targetPosition;
        if (firstPersonMode) 
        {
            targetPosition = fp_localTarget;
        }
        else 
        {
            targetPosition = tp_localTarget;
        }
        while (time < duration)
        {
            cameraMovementAnchor.localPosition = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        cameraMovementAnchor.localPosition = targetPosition;
    }
}
