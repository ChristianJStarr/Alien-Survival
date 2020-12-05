using MLAPI;
using UnityEngine;

public class ServerUI : MonoBehaviour
{
#if UNITY_EDITOR
    public GameObject serverCameraPrefab;
    public GameObject serverUICanvas;

    // Start is called before the first frame update
    void Start()
    {

        if(NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsServer) 
        {
            Instantiate(serverCameraPrefab, transform);
            Instantiate(serverUICanvas, transform);
        }
    }
#endif
}
