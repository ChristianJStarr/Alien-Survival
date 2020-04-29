using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UfoMove : MonoBehaviour
{
    Vector3 target;
    Transform ufo;
    Vector3 reset;
    public int speed = 80;
    bool sendUFO;
    public float ufoDelay = 15;

    void Start()
    {
        ufo = GetComponent<Transform>();
        reset = ufo.position;
        target = new Vector3(reset.x - 500, reset.y, reset.z);
        StartCoroutine(WaitForUFO());
    }

    void Update()
    {
        if (ufo.position != target && sendUFO == true) 
        {
            float step = speed * Time.deltaTime;
            ufo.position = Vector3.MoveTowards(ufo.position, target, step);
        }
        else if(sendUFO == true)
        {
            sendUFO = false;
            ufo.position = reset;
            StartCoroutine(WaitForUFO());
        }
    }
    IEnumerator WaitForUFO() 
    {

        yield return new WaitForSeconds(ufoDelay);
        sendUFO = true;
    }
}   
