using MLAPI;
using UnityEngine;

public class ServerSetAnimatorState : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] private string triggerString;

    public void Start() 
    {
        if(NetworkingManager.Singleton != null && NetworkingManager.Singleton.IsServer) 
        {
            animator.SetTrigger(triggerString);
        }
        Destroy(this);
    }
}
