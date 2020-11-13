using UnityEngine;

public class HeadLook : MonoBehaviour
{
    private Animator avatar;
    public Transform lookAtObj;
    public float lookAtWeight;

    private void Start()
    {
        avatar = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if(layerIndex == 1)
        {
            avatar.SetLookAtWeight(lookAtWeight);
            if (avatar)
            {
                if (lookAtObj != null)
                {
                    avatar.SetLookAtPosition(lookAtObj.position);
                }
            }
        }
    }
}
