using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class IKDebug : MonoBehaviour
{
    private Animator animator;
    private Transform rightHand;
    private Transform leftHand;
    public Transform cameraObject;
    private Rigidbody[] rigids;

    private void Start() 
    {
        animator = GetComponent<Animator>();
        rigids = GetComponentsInChildren<Rigidbody>();
        Debug.Log(rigids.Length);
    }

    private void Update()
    {
        rightHand = GameObject.FindGameObjectWithTag("RHandTarget").transform;
        leftHand = GameObject.FindGameObjectWithTag("LHandTarget").transform;
        RotatePlayerLocally();
    }

    private void RotatePlayerLocally()
    {
        Vector2 lookAxis = new Vector2(CrossPlatformInputManager.GetAxis("Mouse Y"), CrossPlatformInputManager.GetAxis("Mouse X"));
        lookAxis *= 0.03F * 8;

        Quaternion m_CharacterTargetRot = transform.rotation * Quaternion.Euler(0f, lookAxis.y, 0f);
        if (transform.rotation != m_CharacterTargetRot)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, m_CharacterTargetRot, 3 * Time.fixedDeltaTime);
        }

        Quaternion m_CameraTargetRot = ClampRotationAroundXAxis(cameraObject.localRotation * Quaternion.Euler(-lookAxis.x, 0f, 0f)); //Clamp This
        if (cameraObject.localRotation != m_CameraTargetRot)
        {
            cameraObject.localRotation = Quaternion.Lerp(cameraObject.localRotation, m_CameraTargetRot, 3 * Time.fixedDeltaTime);
        }
    }

    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.z /= q.w;
        q.y /= q.w;
        q.w = 1.0F;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, -70, 70);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q.normalized;
    }



    void OnAnimatorIK(int index)
    {
        if (index == 0 && animator)
        {
            if (rightHand != null)
            {

                Quaternion handRotation = Quaternion.LookRotation(rightHand.position - transform.position);
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                //animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHand.position);
                //animator.SetIKRotation(AvatarIKGoal.RightHand, handRotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }
            if (leftHand != null)
            {
                Quaternion handRotation = Quaternion.LookRotation(leftHand.position - transform.position);
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                //animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHand.position);
                //animator.SetIKRotation(AvatarIKGoal.LeftHand, handRotation);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }
        }
    }

}
