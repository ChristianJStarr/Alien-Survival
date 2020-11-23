using MLAPI;
using UnityEngine;
using UnityEngine.AI;

public class AIControlObject : NetworkedBehaviour
{
    public NavMeshAgent agent;
    public Animator animator;
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


    public Snapshot_AI ConvertToSnapshot()
    {
        return new Snapshot_AI()
        {
            networkId = NetworkId,
            location = transform.position,
            rotation = new Vector2(lookUpDownAxis, transform.localRotation.eulerAngles.y)
        };
    }


    public override void NetworkStart()
    {
        if (IsServer)
        {
            WorldAISystem.Singleton.RegisterAI(this);
        }
        else 
        {
            WorldSnapshotManager.Singleton.RegisterAIObject(NetworkId, this);
            Destroy(agent);
        }
    }

    public void OnDestroy()
    {
        if (IsServer)
        {
            WorldAISystem.Singleton.RemoveAI(NetworkId);
        }
        else 
        {
            WorldSnapshotManager.Singleton.RemoveAIObject(NetworkId);
        }
    }
}
