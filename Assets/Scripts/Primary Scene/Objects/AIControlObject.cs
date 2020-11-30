using MLAPI;
using UnityEngine;
using UnityEngine.AI;

public class AIControlObject : NetworkedBehaviour
{
    //--------AI Stats-----------
    public int health = 100;






    public NavMeshAgent agent;
    public Animator animator;
    public Vector2 lastAnimationVector;


    public CollisionFlags collisionFlags;
    
    public bool crouching = false;
    public bool jumping = false;

    public float lookUpDownAxis;

    public Vector3 moveTarget = Vector3.zero;
    public Vector2 lookTarget = Vector3.zero;

    public AI_Type type = AI_Type.enemy;
    public AI_State state = AI_State.guarding;
    public bool stateChanged = false;

    public PlayerControlObject playerAttackTarget;



    //Holdable Object
    public HoldableObject holdableObject;
    public int holdableId = 0;
    public Transform handParent;

    public Transform cameraObject;


    public Snapshot_AI ConvertToSnapshot()
    {
        return new Snapshot_AI()
        {
            networkId = NetworkId,
            location = transform.position,
            holdId = holdableId,
            rotation = new Vector2(lookUpDownAxis, transform.localRotation.eulerAngles.y)
        };
    }


    public override void NetworkStart()
    {
        if (IsServer)
        {
            WorldAISystem.Register(this);
        }
        else 
        {
            WorldSnapshotManager.RegisterObject(this);
            Destroy(agent);
        }
    }

    public void OnDestroy()
    {
        if (IsServer)
        {
            WorldAISystem.Remove(NetworkId);
        }
        else 
        {
            WorldSnapshotManager.RemoveObject(NetworkId);
        }
    }



    public void Animate(Vector2 animateAxis) 
    {
        lastAnimationVector = animateAxis;
        if (animator != null)
        {
            if(animateAxis.magnitude > 0)
            {
                animator.SetBool("shouldMove", true);
            }
            else 
            {
                animator.SetBool("shouldMove", false);
            }
            if (animator.GetFloat("vertical") != animateAxis.y)
            {
                animator.SetFloat("vertical", animateAxis.y);
            }
            if (animator.GetFloat("horizontal") != animateAxis.x)
            {
                animator.SetFloat("horizontal", animateAxis.x);
            }
        }
    }

}
