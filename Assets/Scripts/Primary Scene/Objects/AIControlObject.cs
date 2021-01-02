using MLAPI;
using UnityEngine;
using UnityEngine.AI;

public class AIControlObject : NetworkedBehaviour
{
    public NavMeshAgent agent;


    //--------AI Stats-----------
    public int health = 100;
    public bool crouching = false;
    public bool jumping = false;
    public bool ikActive = false;

    //------------ANIMATOR-------------------
    public Animator animator;
    public Vector2 lastAnimationVector;
    
    //----------MOVING / ROTATING------------
    public float lookUpDownAxis;
    public Vector3 moveTarget = Vector3.zero;
    public Vector2 lookTarget = Vector3.zero;
    public CollisionFlags collisionFlags;

    //------------HOLDING OBJECT-------------
    public HoldableObject holdableObject;
    public int holdableId = 0;
    public int holdableState = 0;
    public Transform handParent;
    public Transform cameraObject;

    public Transform leftHand;
    public Transform rightHand;

    //----------------AI---------------------
    public PlayerControlObject playerAttackTarget;
    public AI_Type type = AI_Type.enemy;
    public AI_State state = AI_State.guarding;
    public bool stateChanged = false;



    public Snapshot_AI ConvertToSnapshot()
    {
        return new Snapshot_AI()
        {
            networkId = NetworkId,
            location = transform.position,
            holdId = holdableId,
            holdState = holdableState, 
            rotation = new Vector2(lookUpDownAxis, transform.localRotation.eulerAngles.y)
        };
    }

    public override void NetworkStart()
    {
#if UNITY_SERVER
            WorldAISystem.Register(this);
#elif UNITY_EDITOR
        if (IsServer)
        {
            WorldAISystem.Register(this);
        }
        else 
        {
            WorldSnapshotManager.RegisterObject(this);
        }
#else
        WorldSnapshotManager.RegisterObject(this);
#endif
    }

    public void OnDestroy()
    {
#if UNITY_SERVER
            WorldAISystem.Remove(NetworkId);    
#elif UNITY_EDITOR
        if (IsServer)
        {
            WorldAISystem.Remove(NetworkId);
        }
        else 
        {
            WorldSnapshotManager.RemoveObject(NetworkId);
        }
#else
        WorldSnapshotManager.RemoveObject(NetworkId);
#endif
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


    public void SetHandIK(Transform hand, bool left)
    {
        if (left)
        {
            leftHand = hand;
        }
        else
        {
            rightHand = hand;
        }
        ikActive = true;
    }

    void OnAnimatorIK(int index)
    {
        if (ikActive && index == 2 && animator)
        {
            if (holdableId != 0)
            {
                if (rightHand != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHand.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHand.rotation);
                }
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                }
                if (leftHand != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHand.rotation);
                }
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                }
            }
            else
            {
                ikActive = false;
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }
        }
    }


}
