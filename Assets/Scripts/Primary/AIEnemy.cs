
using BreadcrumbAi;
using MLAPI;
using UnityEngine;

public class AIEnemy : MonoBehaviour
{

    private void Start()
    {
        if (NetworkingManager.Singleton.IsClient) 
        {
            Destroy(GetComponent<Ai>());
        }
    }

}
