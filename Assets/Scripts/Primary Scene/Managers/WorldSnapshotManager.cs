using MLAPI;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class WorldSnapshotManager : NetworkedBehaviour
{
    public static WorldSnapshotManager Singleton;

    //Managers
    public HoldableManager holdableManager;
    public PlayerCommandManager playerCommandManager;
    public WorldObjectManager worldObjectManager;


    //Control Object Dictionaries
    public Dictionary<ulong, PlayerControlObject> players = new Dictionary<ulong, PlayerControlObject>();
    public Dictionary<ulong, AIControlObject> ai = new Dictionary<ulong, AIControlObject>();
    private bool objectsHaveChanged = true;
    private QuickAccess_Snapshot oldSnapshot;

    //JOBS - Handles
    private JobHandle objLerpJobHandle, camLerpJobHandle, predictPositionHandle, predictRotationHandle;

    //JOBS - Movement / Rotation Lerping
    private TransformAccessArray objectTransformArray,cameraTransformArray;
    private ObjectLerpJob objLerpJob;
    private CameraLerpJob camLerpJob;
    private NativeArray<Vector3> nativeMoveTargets;
    private NativeArray<Vector2> nativeAnimations;
    private NativeArray<float> nativeLookXTargets;
    private NativeArray<float> nativeLookYTargets;

    //JOBS - Future Position Prediction
    Predict predictJob;
    JobHandle predictHandle;
    NativeArray<Vector3> aPositionsNative, bPositionsNative;
    NativeArray<Vector2> aRotationsNative, bRotationsNative;

    //Mass Object Lerping
    private PlayerControlObject[] playerObjects = new PlayerControlObject[0];
    private AIControlObject[] aiObjects;
    private Transform[] l_objTransforms;
    private Transform[] l_camTransforms;
    private Vector3[] l_moveTargets;
    private float[] l_lookXTargets;
    private float[] l_lookYTargets;
    private Vector2[] l_animations;

    //Local Player Object Network ID
    private ulong selfNetworkId;

    //Configuration
    private float lerpSpeed = 5;
    private int moveCorrectionDistance = 5;


    private void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        if (IsServer)
        {
            Destroy(this);
            return;
        }
        holdableManager = HoldableManager.Singleton;
    }

    private void FixedUpdate()
    {
        UpdateTask_LerpAllObjects(); //Lerp All Objects to their Move & Look Targets
    }


    public PlayerControlObject GetLocalPlayerObject() 
    {
        PlayerControlObject instance = null;
        if (players.ContainsKey(selfNetworkId)) 
        {
            instance = players[selfNetworkId];
        }
        return instance;
    }

    public static void RegisterObject(PlayerControlObject controlObject)
    {
        if (Singleton != null)
        {
            Singleton.RegisterObjectTask_Player(controlObject);
        }
    }
    public static void RegisterObject(AIControlObject controlObject)
    {
        if (Singleton != null)
        {
            Singleton.RegisterObjectTask_AI(controlObject);
        }
    }
    public static void RemoveObject(ulong networkId)
    {
        if (Singleton != null)
        {
            Singleton.RemoveObjectTask(networkId);
        }
    }

    //Register Player Control Object
    private void RegisterObjectTask_Player(PlayerControlObject controlObject) 
    {
        objectsHaveChanged = true;
        ulong networkId = controlObject.NetworkId;
        if (players.ContainsKey(networkId))
        {
            players[networkId] = controlObject;
        }
        else
        {
            players.Add(networkId, controlObject);
        }
        if (controlObject.OwnerClientId == NetworkingManager.Singleton.LocalClientId)
        {
            selfNetworkId = networkId;
            playerCommandManager.Register(controlObject);
        }
    }
    //Register AI Control Object
    private void RegisterObjectTask_AI(AIControlObject controlObject)
    {
        objectsHaveChanged = true;
        ulong networkId = controlObject.NetworkId;
        if (ai.ContainsKey(networkId))
        {
            ai[networkId] = controlObject;
        }
        else
        {
            ai.Add(networkId, controlObject);
        }
    }
    //Remove Object from Snapshot Manager
    private void RemoveObjectTask(ulong networkId) 
    {
        objectsHaveChanged = true;
        if (players.ContainsKey(networkId)) 
        {
            players.Remove(networkId);
        }
        else if (ai.ContainsKey(networkId)) 
        {
            ai.Remove(networkId);
        }
    }
 
    //Process Icomming Snapshot
    public void ProcessSnapshot(Snapshot snapshot)
    {
        if (oldSnapshot == null)
        {
            oldSnapshot = new QuickAccess_Snapshot() { snapshotId = 0, networkTime = 0 };
        }

        int aiLength = snapshot.ai.Length; //Length of AI
        int playerLength = snapshot.players.Length; // Length of Players
        int fullLength = aiLength + playerLength; //Full Length of both Arrays
        
        //Apply Snapshot to AI & Players
        if(fullLength > 0) 
        {
            int write = 0; // Write Counter

            //Allocate Position & Rotation Arrays
            Vector3[] positionsA = new Vector3[fullLength];
            Vector3[] positionsB = new Vector3[fullLength];
            Vector2[] rotationsA = new Vector2[fullLength];
            Vector2[] rotationsB = new Vector2[fullLength];

            //Write Player Locations & Rotations
            for (int i = 0; i < playerLength; i++)
            {
                ulong networkId = snapshot.players[i].networkId;
                if (networkId != selfNetworkId)
                {
                    positionsA[write] = snapshot.players[i].location;
                    rotationsA[write] = snapshot.players[i].rotation;
                    if (oldSnapshot.players.ContainsKey(networkId))
                    {
                        positionsB[write] = oldSnapshot.players[networkId].location;
                        rotationsB[write] = oldSnapshot.players[networkId].rotation;
                    }
                    else
                    {
                        positionsB[write] = positionsA[write];
                        rotationsB[write] = rotationsA[write];
                    }
                    write++;
                }
            }

            //Write AI Locations & Rotations
            for (int i = 0; i < aiLength; i++)
            {
                ulong networkId = snapshot.ai[i].networkId;
                positionsA[write] = snapshot.ai[i].location;
                rotationsA[write] = snapshot.ai[i].rotation;
                if (oldSnapshot.ai.ContainsKey(networkId))
                {
                    positionsB[write] = oldSnapshot.ai[networkId].location;
                    rotationsB[write] = oldSnapshot.ai[networkId].rotation;
                }
                else
                {
                    positionsB[write] = positionsA[write];
                    rotationsB[write] = rotationsA[write];
                }
                write++;
            }


            //Prepare Native Arrays & Execute IForJob
            aPositionsNative = new NativeArray<Vector3>(positionsA, Allocator.TempJob);
            bPositionsNative = new NativeArray<Vector3>(positionsB, Allocator.TempJob);
            aRotationsNative = new NativeArray<Vector2>(rotationsA, Allocator.TempJob);
            bRotationsNative = new NativeArray<Vector2>(rotationsB, Allocator.TempJob);
            predictJob = new Predict()
            {
                positionA = aPositionsNative,
                positionB = bPositionsNative,
                rotationA = aRotationsNative,
                rotationB = bRotationsNative
            };
            predictHandle = predictJob.Schedule(fullLength, predictHandle); //Schedule predict job
            predictHandle.Complete(); //Wait for job to end

            write = 0; //Reset write counter

            //Apply Prediction to Player Control Objects
            for (int i = 0; i < snapshot.players.Length; i++)
            {
                ulong networkId = snapshot.players[i].networkId;
                if (players.ContainsKey(networkId))
                {
                    PlayerControlObject controlObject = players[networkId];
                    if (networkId != selfNetworkId)
                    {
                        controlObject.transform.position = snapshot.players[i].location;
                        controlObject.moveTarget = aPositionsNative[write];
                        controlObject.lookTarget = new Vector2(aRotationsNative[write].x, aRotationsNative[write].y);
                        write++;
                    }
                    else //Is local player object
                    {
                        //Movement Correction
                        if (Vector3.Distance(controlObject.transform.position, snapshot.players[i].location) > moveCorrectionDistance)
                        {
                            controlObject.ApplyCorrection(snapshot.players[i].location);
                        }
                    }
                    //Holdable Object
                    if (controlObject.holdableId != snapshot.players[i].holdId)
                    {
                        controlObject.holdableId = snapshot.players[i].holdId;
                        HoldableManager.Singleton.HeldObjectChanged(controlObject);
                    }
                    if (controlObject.holdableObject != null && controlObject.holdableObject.state != snapshot.players[i].holdState)
                    {
                        //Holdable State Changed
                        controlObject.holdableObject.state = snapshot.players[i].holdState;
                        controlObject.holdableObject.Use();
                    }
                }
                else if (networkId != selfNetworkId)
                {
                    write++;
                }
            }

            //Apply Prediction to AI Control Objects
            for (int i = 0; i < snapshot.ai.Length; i++)
            {
                ulong networkId = snapshot.ai[i].networkId;
                if (ai.ContainsKey(networkId))
                {
                    AIControlObject controlObject = ai[networkId];
                    controlObject.transform.position = snapshot.ai[i].location;
                    controlObject.moveTarget = aPositionsNative[write];
                    controlObject.lookTarget = new Vector2(aRotationsNative[write].x, aRotationsNative[write].y);
                    //Holdable Object
                    if (controlObject.holdableId != snapshot.ai[i].holdId)
                    {
                        controlObject.holdableId = snapshot.ai[i].holdId;
                        HoldableManager.Singleton.HeldObjectChanged(controlObject);
                    }
                    if (controlObject.holdableObject != null && controlObject.holdableObject.state != snapshot.ai[i].holdState)
                    {
                        //Holdable State Changed
                        controlObject.holdableObject.state = snapshot.ai[i].holdState;
                        controlObject.holdableObject.Use();
                    }
                }
                write++;
            }

            //Dispose of NativeArrays
            aPositionsNative.Dispose();
            bPositionsNative.Dispose();
            aRotationsNative.Dispose();
            bRotationsNative.Dispose();
        }

        //Update World Objects
        if (players.ContainsKey(selfNetworkId) && snapshot.worldObjects.Length > 0)
        {
            worldObjectManager.UpdateWorldObjects(snapshot.worldObjects, players[selfNetworkId].transform.position);
        }

        //Convert & Save this Snapshot
        oldSnapshot = Snapshot.ConvertQuick(snapshot);
    }


    //Lerp All Control Objects Positions
    private void UpdateTask_LerpAllObjects()
    {
        int count = players.Count - 1; //Exclude Self
        count += ai.Count;
        if (count > 0)
        {
            int writeCount = 0;
            if (objectsHaveChanged) 
            {
                List<PlayerControlObject> tempPlayer = players.Values.ToList();
                tempPlayer.Remove(players[selfNetworkId]);
                playerObjects = tempPlayer.ToArray();
                aiObjects = ai.Values.ToArray();
                l_objTransforms = new Transform[count];
                l_camTransforms = new Transform[count];
                l_moveTargets = new Vector3[count];
                l_lookXTargets = new float[count];
                l_lookYTargets = new float[count];
                l_animations = new Vector2[count];
                for (int i = 0; i < playerObjects.Length; i++)
                {
                    if (writeCount < count)
                    {
                        l_objTransforms[writeCount] = playerObjects[i].transform;
                        l_camTransforms[writeCount] = playerObjects[i].cameraObject;
                        l_moveTargets[writeCount] = playerObjects[i].moveTarget;
                        l_lookXTargets[writeCount] = playerObjects[i].lookTarget.x;
                        l_lookYTargets[writeCount] = playerObjects[i].lookTarget.y;
                        l_animations[writeCount] = playerObjects[i].lastAnimationVector;
                        writeCount++;
                    }
                }
                for (int i = 0; i < aiObjects.Length; i++)
                {
                    if (writeCount < count)
                    {
                        l_objTransforms[writeCount] = aiObjects[i].transform;
                        l_camTransforms[writeCount] = aiObjects[i].cameraObject;
                        l_moveTargets[writeCount] = aiObjects[i].moveTarget;
                        l_lookXTargets[writeCount] = aiObjects[i].lookTarget.x;
                        l_lookYTargets[writeCount] = aiObjects[i].lookTarget.y;
                        l_animations[writeCount] = aiObjects[i].lastAnimationVector;
                        writeCount++;
                    }
                }
                objectsHaveChanged = false;
            }
            else 
            {
                for (int i = 0; i < playerObjects.Length; i++)
                {
                    if (writeCount < count)
                    {
                        l_moveTargets[writeCount] = playerObjects[i].moveTarget;
                        l_lookXTargets[writeCount] = playerObjects[i].lookTarget.x;
                        l_lookYTargets[writeCount] = playerObjects[i].lookTarget.y;
                        l_animations[writeCount] = playerObjects[i].lastAnimationVector;
                        writeCount++;
                    }
                }
                for (int i = 0; i < aiObjects.Length; i++)
                {
                    if (writeCount < count)
                    {
                        l_moveTargets[writeCount] = aiObjects[i].moveTarget;
                        l_lookXTargets[writeCount] = aiObjects[i].lookTarget.x;
                        l_lookYTargets[writeCount] = aiObjects[i].lookTarget.y;
                        l_animations[writeCount] = aiObjects[i].lastAnimationVector;
                        writeCount++;
                    }
                }
            }


            objectTransformArray = new TransformAccessArray(l_objTransforms);
            cameraTransformArray = new TransformAccessArray(l_camTransforms);
            nativeMoveTargets = new NativeArray<Vector3>(l_moveTargets, Allocator.TempJob);
            nativeLookXTargets = new NativeArray<float>(l_lookXTargets, Allocator.TempJob);
            nativeLookYTargets = new NativeArray<float>(l_lookYTargets, Allocator.TempJob);
            nativeAnimations = new NativeArray<Vector2>(l_animations, Allocator.TempJob);

            objLerpJob = new ObjectLerpJob();
            objLerpJob.deltaTime = Time.fixedDeltaTime;
            objLerpJob.moveTargets = nativeMoveTargets;
            objLerpJob.lookYTargets = nativeLookYTargets;
            objLerpJob.lerpSpeed = lerpSpeed;
            objLerpJob.animation = nativeAnimations;
            objLerpJobHandle = objLerpJob.Schedule(objectTransformArray);


            objLerpJobHandle.Complete();

            camLerpJob = new CameraLerpJob();
            camLerpJob.deltaTime = Time.fixedDeltaTime;
            camLerpJob.lookXTargets = nativeLookXTargets;
            camLerpJob.lerpSpeed = lerpSpeed;
            camLerpJobHandle = camLerpJob.Schedule(cameraTransformArray);

            camLerpJobHandle.Complete();

            writeCount = 0;
            for (int i = 0; i < playerObjects.Length; i++)
            {
                if (writeCount < count )
                {
                    if(playerObjects[i].NetworkId != selfNetworkId) 
                    {
                        playerObjects[i].Animate(nativeAnimations[writeCount]);
                    }
                    writeCount++;
                }
            }
            for (int i = 0; i < aiObjects.Length; i++)
            {
                if (writeCount < count)
                {
                    aiObjects[i].Animate(nativeAnimations[writeCount]);
                    writeCount++;
                }
            }

            objectTransformArray.Dispose();
            nativeMoveTargets.Dispose();
            nativeLookYTargets.Dispose();
            cameraTransformArray.Dispose();
            nativeLookXTargets.Dispose();
            nativeAnimations.Dispose();
        }
    }
}



public struct ObjectLerpJob : IJobParallelForTransform
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public NativeArray<Vector3> moveTargets;
    [ReadOnly] public NativeArray<float> lookYTargets;
    [ReadOnly] public float lerpSpeed;
    public NativeArray<Vector2> animation;

    public void Execute(int i, TransformAccess transform)
    {
        if(moveTargets[i] != Vector3.zero && transform.position != moveTargets[i]) 
        {
            Vector3 distance = transform.localPosition;
            transform.position = Vector3.Lerp(transform.position, moveTargets[i], deltaTime * lerpSpeed);
            distance -= transform.localPosition;
            distance /= deltaTime;
            distance.x = Mathf.Clamp(distance.x, -1, 2);
            distance.z = Mathf.Clamp(distance.z, -1, 1);
            animation[i] = Vector2.Lerp(animation[i], new Vector2(distance.x, distance.z), deltaTime * lerpSpeed * 5);
        }
        if (lookYTargets[i] != 0 && transform.rotation.eulerAngles.y != lookYTargets[i])
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(0, lookYTargets[i], 0)), deltaTime * lerpSpeed);
        }
    }
}

public struct CameraLerpJob : IJobParallelForTransform
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public NativeArray<float> lookXTargets;
    [ReadOnly] public float lerpSpeed;
    public void Execute(int i, TransformAccess transform)
    {
        if (lookXTargets[i] != 0 && transform.localRotation.eulerAngles.x != lookXTargets[i])
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3(lookXTargets[i], 0, 0)), deltaTime * lerpSpeed);
        }
    }
}

public struct Predict : IJobFor 
{
    public NativeArray<Vector3> positionA;
    [ReadOnly] public NativeArray<Vector3> positionB;
    public NativeArray<Vector2> rotationA;
    [ReadOnly] public NativeArray<Vector2> rotationB;
    public void Execute(int i) 
    {
        if (rotationA[i] != rotationB[i])
        {
            rotationA[i] += rotationA[i] - rotationB[i];
        }
        if (positionA[i] != positionB[i]) 
        {
            positionA[i] += positionA[i] - positionB[i];
        }
    }

}