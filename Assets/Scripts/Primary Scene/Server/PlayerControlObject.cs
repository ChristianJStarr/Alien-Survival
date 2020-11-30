using MLAPI;
using UnityEngine;

public class PlayerControlObject : NetworkedBehaviour
{
    public CharacterController characterController;
    public Transform cameraObject;
    public Transform cameraPoint;


    public Animator animator;
    public Vector2 lastAnimationVector;

    //----------------STATES-----------------
    public bool use = false;
    public bool crouching = false;
    public bool jumping = false;

    //----------MOVING / ROTATING------------
    public Vector3 moveTarget = Vector3.zero;
    public Vector2 lookTarget = Vector3.zero;

    private bool needsMoveCorrection = false;
    public Vector3 correctionMove = Vector3.zero;

    public CollisionFlags collisionFlags;

    //------------HOLDING OBJECT-------------
    public int holdableId = 0;
    public HoldableObject holdableObject;
    public Transform handParent;

    //------------SELECTED SLOT---------------
    public int selectedSlot = 0;
    public Item selectedItem;

    //----------------TIMES-----------------
    public float useDelayTime = 0;



    //Convert this ControlObject to Snapshot_Player Format
    public Snapshot_Player ConvertToSnapshot()
    {
        return new Snapshot_Player()
        {
            networkId = NetworkId,
            location = transform.position,
            holdId = holdableId,
            rotation = new Vector2(cameraObject.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y)
        };
    }

    //Start Called on Spawn
    public override void NetworkStart()
    {
        if (IsClient)
        {
            WorldSnapshotManager.RegisterObject(this);
        }
    }

    //Destroy
    public void OnDestroy()
    {
        if (IsClient)
        {
            WorldSnapshotManager.RemoveObject(NetworkId);
        }
    }

    private void FixedUpdate()
    {
        if (needsMoveCorrection) 
        {
            CorrectionMoveTask();
        }
    }

    //Rotate this Object from Axis
    public void Rotate(Vector2 lookAxis)
    {
        Quaternion m_CharacterTargetRot = Quaternion.Euler(0f, lookAxis.y, 0f);
        if (transform.rotation != m_CharacterTargetRot)
        {
            transform.rotation = m_CharacterTargetRot;
        }
        Quaternion m_CameraTargetRot = Quaternion.Euler(lookAxis.x, 0f, 0f);
        if (cameraObject.localRotation != m_CameraTargetRot)
        {
            cameraObject.localRotation = m_CameraTargetRot;
        }
    }

    //Move this Object from Axis
    public void Move(Vector2 moveAxis, bool jump, bool crouch)
    {
        if (needsMoveCorrection) return;
        Vector3 desiredMove = transform.forward * moveAxis.y + transform.right * moveAxis.x;
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, characterController.radius, Vector3.down, out hitInfo, characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized * 5;
        Debug.Log("Desired: " + desiredMove.ToString());
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

    //Animate from Axis
    public void Animate(Vector2 animateAxis)
    {
        lastAnimationVector = animateAxis;
        if (animator != null)
        {
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

    //Apply Rotation Correction
    public void ApplyCorrection(Vector3 position) 
    {
        correctionMove = position;
        needsMoveCorrection = true;
    }

    private void CorrectionMoveTask()
    {
        if (correctionMove != Vector3.zero)
        {
            float distance = Vector3.Distance(correctionMove, transform.position);
            if (distance > 2) 
            {
                transform.position = correctionMove;
            }
            else if(distance > 0.01F)
            {
                transform.position = Vector3.Lerp(transform.position, correctionMove, 10 * Time.fixedDeltaTime);
                if (transform.position == correctionMove)
                {
                    correctionMove = Vector3.zero;
                    needsMoveCorrection = false;
                }
            }
            else 
            {
                correctionMove = Vector3.zero;
                needsMoveCorrection = false;
            }

        }
        else { needsMoveCorrection = false; }
    }
}