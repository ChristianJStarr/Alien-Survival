using MLAPI;
using UnityEngine;

public class PlayerControlObject : NetworkedBehaviour
{
    public CharacterController characterController;
    public Transform cameraObject;

    public CollisionFlags collisionFlags;
    public bool crouching = false;
    public bool jumping = false;

    public Vector3[] positions;

    public int lastSnapshot;

}