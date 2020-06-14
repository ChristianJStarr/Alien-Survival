using MLAPI;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class MovementSystem : MonoBehaviour
{
    List<MovementSystemObject> activeObjects;

    private void Start()
    {
        activeObjects = new List<MovementSystemObject>();
    }

    private void Update()
    {
        HandleActiveObjects();
    }


    public void MoveNetworkObject(NetworkedObject networkedObject, Vector3 position, Quaternion rotation) 
    {
        bool beingModified = false; ;
        for (int i = 0; i < activeObjects.Count; i++)
        {
            if(activeObjects[i].networkedObject.NetworkedInstanceId == networkedObject.NetworkedInstanceId) 
            {
                beingModified = true;
                MovementSystemObject newObject = new MovementSystemObject();
                newObject.networkedObject = networkedObject;
                newObject.positionTarget = position;
                newObject.rotationTarget = rotation;
                activeObjects[i] = newObject;
            }
        }
        if (!beingModified) 
        {
            MovementSystemObject newObject = new MovementSystemObject();
            newObject.networkedObject = networkedObject;
            newObject.positionTarget = position;
            newObject.rotationTarget = rotation;
            activeObjects.Add(newObject);
        }
    }

    private void HandleActiveObjects() 
    {
        for (int i = 0; i < activeObjects.Count; i++)
        {
            activeObjects[i].networkedObject.transform.position = Vector3.MoveTowards(activeObjects[i].networkedObject.transform.position, activeObjects[i].positionTarget, 1F * Time.deltaTime);
            activeObjects[i].networkedObject.transform.rotation = Quaternion.RotateTowards(activeObjects[i].networkedObject.transform.rotation, activeObjects[i].rotationTarget, 1F * Time.deltaTime);
            if (activeObjects[i].networkedObject.transform.position == activeObjects[i].positionTarget) 
            {
                if(activeObjects[i].networkedObject.transform.rotation == activeObjects[i].rotationTarget) 
                {
                    activeObjects.RemoveAt(i);
                }
            }
        }
    }

}


public class MovementSystemObject 
{
    public NetworkedObject networkedObject;
    public Vector3 positionTarget;
    public Quaternion rotationTarget;
}
