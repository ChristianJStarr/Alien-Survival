using MLAPI;
using UnityEngine;

public class PlayerCameraManager : MonoBehaviour
{
    public GameObject playerCamera;
    public GameObject playerViewCamera;

    private void Start()
    {
        if (NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsClient)
        {
            PlayerCameraObject playerMount = NetworkingManager.Singleton.ConnectedClients[NetworkingManager.Singleton.LocalClientId].PlayerObject.GetComponent<PlayerCameraObject>();
            if(playerMount != null) 
            {
                Instantiate(playerCamera, playerMount.mainCameraSpawnAnchor);
                Instantiate(playerViewCamera, playerMount.viewCameraSpawnAnchor);
                if(LoadAwake.Singleton != null) 
                {
                    LoadAwake.Singleton.playerHasCamera = true;
                }
            }
        }
    }
}
