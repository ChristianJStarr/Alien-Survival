using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class CameraMatchY : MonoBehaviour
{
    public Transform target;
    public Transform camTarget;
    public Transform firstTarget;
    public Transform thirdTarget;
    public float cameraHeight;
    public int speed;
    public bool firstPerson = false;

    void Start() 
    {
        cameraHeight = target.position.y;
    }
    void Update() 
    {
        if (CrossPlatformInputManager.GetButtonDown("SwitchCamera")) 
        {
            firstPerson = !firstPerson;   
        }
    }
    void LateUpdate()
    {
        if (thirdTarget != null) 
        {
            float step = speed * Time.deltaTime;
            Vector3 targetPos = transform.position;
            Vector3 shoulderPos = camTarget.position;
            shoulderPos.y = target.position.y;
            if (firstPerson)
            {
                targetPos.z = firstTarget.position.z;
                targetPos.x = firstTarget.position.x;
                targetPos.y = firstTarget.position.y;
            }
            if (!firstPerson)
            {
                targetPos.z = thirdTarget.position.z;
                targetPos.x = thirdTarget.position.x;
                targetPos.x = firstTarget.position.x;
            }
            if (transform.position != targetPos)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            }
            if (camTarget.position != shoulderPos)
            {
                camTarget.transform.position = Vector3.MoveTowards(camTarget.position, shoulderPos, step);
            }
        }    
    }
}
