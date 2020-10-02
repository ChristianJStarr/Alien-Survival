using MLAPI;
using UnityEngine;
using UnityEngine.AI;

public class AIObject : MonoBehaviour
{

    public enum Type
    {
        friendly,
        enemy
    }
    public enum State
    {
        attacking,
        wandering,
        guarding
    }

    public Type type = Type.enemy;
    public State state = State.guarding;
    [Header("Health")]
    public int health = 100;
    private int maxHealth = 100;
    [Header("Movement")]
    public int speed = 5;
    [Header("Weapon")]
    public int holdableObjectId = 1;
    public int damageAmount = 10;
    public int damageFrequency = 1;

    [SerializeField] private HoldableManager holdableManager;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent controller;
    public Transform aimPosition;
    public NetworkedObject target;

    private float lastRayTime;
    private bool canSeeTarget;

    private Vector2 velocity = Vector2.zero;
    private Vector2 smoothDeltaPosition = Vector2.zero;
    private Vector3 lastPosition;


    private void Start() 
    {
        if (!NetworkingManager.Singleton.IsServer) 
            Destroy(this);
    }



    private void Update()
    {   if (type == Type.enemy)
            AnimateMovement();

        if (state == State.guarding) //Guard
        {
            GuardLogic();
        }
        else if (state == State.attacking) //Attack
        {
            AttackLogic();
        }
        else if (state == State.wandering && (!controller.hasPath || controller.pathStatus == NavMeshPathStatus.PathComplete)) //Wander
        {
            WanderLogic();
        }
    }

    //Update Logic
    private void GuardLogic() 
    {
        AnimateTrigger("Guard");
    }

    private void AttackLogic() 
    {
        if (target != null && (target.transform.position - transform.position).magnitude < 100)
        {
            if (controller.hasPath)
            {
                if (CanSeeTarget())
                {
                    if (holdableManager.currentHeldObject != holdableObjectId)
                    {
                        holdableManager.PulloutHoldable(holdableObjectId);
                    }
                    controller.speed = 0.01f;
                }
                else if (controller.speed != speed * 1.5F)
                {
                    controller.speed = speed * 1.5F;
                }
            }
            else if (!controller.hasPath || (controller.destination - target.transform.position).magnitude > 1)//Move towarads it
            {
                if (holdableManager.currentHeldObject != holdableObjectId)
                {
                    holdableManager.PulloutHoldable(holdableObjectId);
                }
                controller.SetDestination(target.transform.position);
                controller.speed = speed * 1.5F;
            }
        }
        else
        {
            state = State.wandering;
        }
    }

    private void WanderLogic() 
    {
        if (holdableManager != null && holdableManager.currentHeldObject != 0)
        {
            holdableManager.PulloutHoldable(0);
        }
        controller.speed = speed;
        controller.SetDestination(RandomNavSphere(transform.position, Random.Range(10, 50)));
    }
    
    
    
    
    //Animation Calls
    private void AnimateTrigger(string trigger) 
    {
        if(animator != null) 
        {
          //  animator.SetTrigger(trigger);
        }
    }

    private void AnimateMovement() 
    {
        if(lastPosition != transform.position) 
        {
            animator.SetBool("shouldMove", true);
            animator.SetFloat("vertical", 1);
            lastPosition = transform.position;
        }
        else 
        {
            animator.SetBool("shouldMove", false);
        }
    }




    //UTILITIES
    public static Vector3 RandomNavSphere(Vector3 origin, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, distance, -1);
        return navHit.position;
    }
    public bool CanSeeTarget()
    {
        if (target != null) 
        {
            if(NetworkingManager.Singleton.NetworkTime - lastRayTime > 5) 
            {
                lastRayTime = NetworkingManager.Singleton.NetworkTime;
                Vector3 targetPosition = new Vector3(target.transform.position.x, target.transform.position.y + 2.5F, target.transform.position.z); //Aim at chest
                RaycastHit hit;

                if (Physics.Raycast(aimPosition.position, target.transform.position - aimPosition.position, out hit, 30))
                {
                    Debug.DrawRay(aimPosition.position, targetPosition - aimPosition.position, Color.red, 4);
                    canSeeTarget = hit.collider.CompareTag("Player_Collider_Head") || hit.collider.CompareTag("Player_Collider_Body") || hit.collider.CompareTag("Player_Collider_Legs");
                    return canSeeTarget;
                }
            }
            else 
            {
                return canSeeTarget;
            }
        }
        return false;
    }

}
