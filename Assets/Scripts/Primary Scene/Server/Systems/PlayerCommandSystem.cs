using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCommandSystem : MonoBehaviour
{
    private bool systemEnabled = false;

    private Queue<PlayerCommand> queue = new Queue<PlayerCommand>();
    public Dictionary<ulong, PlayerControlObject> players = new Dictionary<ulong, PlayerControlObject>();


    //Movement Config
    public float stickToGroundForce = 5;
    public float jumpSpeed = 1;
    public float gravityMultiplier = 1;
    public float lookMaximumX = -150;
    public float lookMinimumX = 150;
    public bool rotateSmoothing = true;
    public float lookSmoothTime = 1;
    public float moveSmoothTime = 1;

    //Movement Vars
    private Vector3 moveDir = Vector3.zero;
    
    public bool StartSystem() 
    {
        systemEnabled = true;
        return true;
    }
    
    public bool StopSystem() 
    {
        systemEnabled = false;
        return true;
    }

    private void Update() 
    {
        if(systemEnabled && queue.Count > 0) 
        {
            for (int i = 0; i < queue.Count; i++)
            {
                PostCommand(queue.Dequeue());
            }
        }
    }

    //Queue Command
    public void AddCommand(PlayerCommand command) 
    {
        queue.Enqueue(command);
    }
    
    //Initiate Command Execution
    private void PostCommand(PlayerCommand command)
    {
        if (players.ContainsKey(command.clientId))
        {
            ExecuteCommand(command);
        }
        else
        {
            PlayerControlObject controlObject = NetworkingManager.Singleton.ConnectedClients[command.clientId].PlayerObject.GetComponent<PlayerControlObject>();
            if (controlObject != null)
            {
                players.Add(command.clientId, controlObject);
                ExecuteCommand(command);
            }

        }

    }

    //Execute the Command
    private void ExecuteCommand(PlayerCommand command, PlayerControlObject controlObject = null)
    {
        DebugMsg.Notify("Executing Command: MOVE:" + command.moveX + "," + command.moveY + " LOOK:" + command.lookX + "," + command.lookY, 3); ;

        if (controlObject == null)
        {
            controlObject = players[command.clientId];
        }
        CharacterController characterController = controlObject.characterController;

        //Rotate Character
        Quaternion characterTargetRot = Quaternion.Euler(0f, command.lookY * command.sensitivityY, 0f);
        Quaternion cameraTargetRot = ClampRotationAroundXAxis(Quaternion.Euler(-(command.lookX * command.sensitivityX), 0f, 0f));

        if (rotateSmoothing)
        {
            controlObject.transform.localRotation = Quaternion.Slerp(controlObject.transform.localRotation, characterTargetRot, moveSmoothTime * Time.deltaTime);
            controlObject.cameraObject.localRotation = Quaternion.Slerp(controlObject.cameraObject.localRotation, cameraTargetRot,
                lookSmoothTime * Time.deltaTime);
        }
        else
        {
            controlObject.transform.localRotation = characterTargetRot;
            controlObject.cameraObject.localRotation = cameraTargetRot;
        }


        //Move Character

        float speed = 1; //Fixed speed modifyer. Use for something later?
        Vector2 m_Input = new Vector2(command.moveX, command.moveY);
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = controlObject.transform.forward * m_Input.y + controlObject.transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(controlObject.transform.position, characterController.radius, Vector3.down, out hitInfo,
                           characterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;
        moveDir.x = desiredMove.x * speed;
        moveDir.z = desiredMove.z * speed;

        if (characterController.isGrounded)
        {
            moveDir.y = -stickToGroundForce;
            if (command.jump && !command.crouch && !controlObject.jumping)
            {
                moveDir.y = jumpSpeed;
                controlObject.jumping = true;
            }
            if (command.crouch)
            {
                controlObject.crouching = !controlObject.crouching;
            }
        }
        else
        {
            moveDir += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
        }
        controlObject.collisionFlags = characterController.Move(moveDir * Time.fixedDeltaTime);
    }

    //TOOL: Clamp Rotation Around X Axis
    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, lookMinimumX, lookMaximumX);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        return q;
    }

}

