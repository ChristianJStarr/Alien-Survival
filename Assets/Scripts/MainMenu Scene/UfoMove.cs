using System.Collections;
using UnityEngine;

public class UfoMove : MonoBehaviour
{
    [Tooltip("Seconds Between Ufo Appearance")]
    public float ufoDelay = 15;
    [Space]
    [Tooltip("Speed of Ufo")]
    public float speed = 0.8F;

    private Vector3 target_position;
    private Vector3 reset_position;


    void Start()
    {
        reset_position = transform.position;
        target_position = transform.position;
        target_position.x -= 500;
        target_position.z -= 50;
        StartCoroutine(UfoMovementLoop());
    }

    private IEnumerator UfoMovementLoop() 
    {
        WaitForSeconds wait = new WaitForSeconds(ufoDelay);
        while (true) 
        {
            yield return wait;
            float time = 0;
            Vector3 startPosition = transform.position;
            while (time < speed)
            {
                transform.position = Vector3.Lerp(startPosition, target_position, time / speed);
                time += Time.deltaTime;
                yield return null;
            }
            transform.position = reset_position;
        }
    }
}   
