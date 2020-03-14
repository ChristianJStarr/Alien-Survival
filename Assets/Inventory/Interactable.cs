using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(ColorOnHover))]
public class Interactable : MonoBehaviour
{

    public float radius = 3f;
    public Transform interactionTransform;

    bool isFocus = false;
    Transform player;   

    bool hasInteracted = false;

    void Update()
    {
        
        if (isFocus)
        {
            if (player.position != null && interactionTransform.position != null)
            {
                float distance = Vector3.Distance(player.position, interactionTransform.position);
                if (!hasInteracted && distance <= radius)
                {
                    hasInteracted = true;
                    Interact();
                }
            }
            else 
            {
            }
        }
    }

    public void OnFocused(Transform playerTransform)
    {
        isFocus = true;
        hasInteracted = false;
        player = playerTransform;
    }

    public void OnDefocused()
    {
        isFocus = false;
        hasInteracted = false;
        player = null;
    }

    public virtual void Interact()
    {
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(interactionTransform.position, radius);
    }

}