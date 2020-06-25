using UnityEngine;

public class DebugIk : MonoBehaviour
{

    public Animator animator;
    public bool ikActive = true;


    private Transform rightHand;
    private Transform leftHand;

    private void Start()
    {
        rightHand = GameObject.FindGameObjectWithTag("RHandTarget").transform;
        leftHand = GameObject.FindGameObjectWithTag("LHandTarget").transform;
    }

    public void UpdateTargets() 
    {
        rightHand = GameObject.FindGameObjectWithTag("RHandTarget").transform;
        leftHand = GameObject.FindGameObjectWithTag("LHandTarget").transform;
    }
    void OnAnimatorIK(int index)
    {
        if(index == 2) 
        {
            if (animator)
            {
                if (ikActive)
                {
                    Debug.Log("running ik");
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
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                }
            }
        }
    }
        
}
