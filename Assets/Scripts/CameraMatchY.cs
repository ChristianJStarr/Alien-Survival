using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class CameraMatchY : MonoBehaviour
{
    public Transform target;
    public Transform leftTarget;
    public Transform rightTarget;
    public float cameraHeight;
    public int speed;
    public bool leftShoulder = false;

    void Start() 
    {
        cameraHeight = target.position.y;
    }
    void Update() 
    {
        if (CrossPlatformInputManager.GetButtonDown("SwitchCamera")) 
        {
            leftShoulder = !leftShoulder;   
        }
    }
    void LateUpdate()
    {
        if (rightTarget != null) 
        {
            float step = speed * Time.deltaTime;

            Vector3 targetPos = transform.position;
            targetPos.y = target.position.y;
            if (leftShoulder)
            {
                targetPos.x = leftTarget.position.x;
                targetPos.z = leftTarget.position.z;
            }
            if (!leftShoulder)
            {
                targetPos.x = rightTarget.position.x;
                targetPos.z = rightTarget.position.z;
            }

            if (transform.position != targetPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            }
        }    
    }
}
