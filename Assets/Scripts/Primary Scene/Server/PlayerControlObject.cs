using MLAPI;
using UnityEngine;

public class PlayerControlObject : NetworkedBehaviour
{
    public CharacterController characterController;
    public Transform cameraObject;
    public Animator animator;


    //----------------STATES-----------------
    public bool crouching = false;
    public bool jumping = false;

    //----------MOVING / ROTATING------------
    public Vector3 moveTarget = Vector3.zero;
    public Vector2 lookTarget = Vector3.zero;
    public CollisionFlags collisionFlags;

    //------------HOLDING OBJECT-----------------
    private int heldObjectId = 0;

    //Convert this ControlObject to Snapshot_Player Format
    public Snapshot_Player ConvertToSnapshot()
    {
        return new Snapshot_Player()
        {
            networkId = NetworkId,
            location = transform.position,
            rotation = new Vector2(cameraObject.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y)
        };
    }

    //Start Called on Spawn
    public override void NetworkStart()
    {
        if (IsClient)
        {
            if(WorldSnapshotManager.Singleton != null)
            {
                WorldSnapshotManager.Singleton.RegisterPlayer(NetworkId, this);
            }
        }
    }

    //Destroy
    public void OnDestroy()
    {
        if (IsClient)
        {
            if(WorldSnapshotManager.Singleton != null) 
            {
                WorldSnapshotManager.Singleton.RemovePlayer(NetworkId);
            }
        }
    }


    //Rotate this Object from Axis
    public void Rotate(Vector2 lookAxis, Vector2 sensitivity)
    {
        sensitivity.x = Mathf.Clamp(sensitivity.x, 0.1F, 1);
        sensitivity.y = Mathf.Clamp(sensitivity.y, 0.1F, 1);
        lookAxis *= sensitivity * 8; //apply sensitivity

        //Player Object ( X Axis )
        Quaternion m_CharacterTargetRot = transform.rotation * Quaternion.Euler(0f, lookAxis.y, 0f);
        if (transform.rotation != m_CharacterTargetRot)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, m_CharacterTargetRot, 3 * Time.fixedDeltaTime);
        }

        //Camera Object ( Y Axis )
        Quaternion m_CameraTargetRot = cameraObject.localRotation * Quaternion.Euler(-lookAxis.x, 0f, 0f); //Clamp This
        if (cameraObject.localRotation != m_CameraTargetRot)
        {
            cameraObject.localRotation = Quaternion.Slerp(cameraObject.localRotation, m_CameraTargetRot, 3 * Time.fixedDeltaTime);
        }
    }

    //Move this Object from Axis
    public void Move(Vector2 moveAxis, bool jump, bool crouch)
    {
        Vector3 desiredMove = transform.forward * moveAxis.y + transform.right * moveAxis.x;
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo, characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized * 5;

        if (characterController.isGrounded)
        {
            desiredMove.y = -2;
            if (jump && !crouch && !jumping)
            {
                desiredMove.y = 5;
                jumping = true;
            }
            if (crouch)
            {
                crouching = !crouching;
            }
        }
        else
        {
            desiredMove += Physics.gravity * 2 * Time.fixedDeltaTime;
        }
        collisionFlags = characterController.Move(desiredMove * Time.fixedDeltaTime);
    }




}