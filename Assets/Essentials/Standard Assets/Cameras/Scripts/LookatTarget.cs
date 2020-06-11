using UnityEngine;

public class LookatTarget : MonoBehaviour
{
    public Transform target;
    private void Update()
    {
        Vector3 dir = target.position - transform.position;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        lookRot.x = 0; lookRot.z = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Mathf.Clamp01(10f * Time.maximumDeltaTime));
    }

}