using MLAPI;
using System.Collections.Generic;
using UnityEngine;

public class WorldSnapshotManager : NetworkedBehaviour
{
    public static WorldSnapshotManager Singleton;

    public PlayerCommandManager playerCommandManager;
    public Dictionary<ulong, PlayerControlObject> players = new Dictionary<ulong, PlayerControlObject>();
    public Dictionary<ulong, AIControlObject> ai = new Dictionary<ulong, AIControlObject>();
    private NetworkingManager networkingManager;
    private float lerpSpeed = 5;
    private QuickAccess_Snapshot[] snapshots;

    private ulong selfNetworkId;

    private void Update()
    {
        //LERP PLAYERS
        foreach (PlayerControlObject item in players.Values)
        {
            if (item != null && item.NetworkId != selfNetworkId)
            {
                if (item.moveTarget != Vector3.zero && item.transform.position != item.moveTarget)
                {
                    item.transform.position = Vector3.Lerp(item.transform.position, item.moveTarget, lerpSpeed * Time.deltaTime);
                }
                if (item.transform.rotation.eulerAngles.y != item.lookTarget.y)
                {
                    item.transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Lerp(item.transform.rotation.eulerAngles.y, item.lookTarget.y, lerpSpeed * Time.deltaTime), 0));
                }
                if (item.cameraObject.localRotation.eulerAngles.x != item.lookTarget.x)
                {
                    item.cameraObject.localRotation = Quaternion.Euler(new Vector3(0, Mathf.Lerp(item.cameraObject.localRotation.eulerAngles.x, item.lookTarget.x, lerpSpeed * Time.deltaTime), 0));
                }
            }
        }

        //LERP AI
        foreach (AIControlObject item in ai.Values)
        {
            if (item != null)
            {
                if (item.moveTarget != Vector3.zero && item.transform.position != item.moveTarget)
                {
                    item.transform.position = Vector3.Lerp(item.transform.position, item.moveTarget, lerpSpeed * Time.deltaTime);
                }
                if (item.transform.rotation.eulerAngles.y != item.lookTarget.y)
                {
                    item.transform.rotation = Quaternion.Slerp(item.transform.rotation, Quaternion.Euler(new Vector3(0, item.lookTarget.y, 0)), lerpSpeed * Time.deltaTime);
                }
                if(item.lookUpDownAxis != item.lookTarget.x) 
                {
                    item.lookUpDownAxis = Mathf.Lerp(item.lookUpDownAxis, item.lookTarget.x, lerpSpeed * Time.deltaTime);
                }
            }
        }

    }

    private void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        if (IsServer)
        {
            Destroy(this);
        }
        else
        {
            networkingManager = NetworkingManager.Singleton;
            snapshots = new QuickAccess_Snapshot[4];

            for (int i = 0; i < snapshots.Length; i++)
            {
                snapshots[i] = new QuickAccess_Snapshot() { snapshotId = 0, networkTime = i };
            }
        }
    }

    //Register Player in Dictionary
    public void RegisterPlayer(ulong networkId, PlayerControlObject controlObject)
    {
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

    //Remove Player from Dictionary
    public void RemovePlayer(ulong networkId)
    {
        if (players.ContainsKey(networkId))
        {
            players.Remove(networkId);
        }
    }

    //Register AI in Dictionary
    public void RegisterAIObject(ulong networkId, AIControlObject controlObject) 
    {
        if (ai.ContainsKey(networkId)) 
        {
            ai[networkId] = controlObject;
        }
        else 
        {
            ai.Add(networkId, controlObject);
        }
    }

    //Remove Player from Dictionary
    public void RemoveAIObject(ulong networkId) 
    {
        if (ai.ContainsKey(networkId)) 
        {
            ai.Remove(networkId);
        }
    }

    //Process Icomming Snapshot
    public void ProcessSnapshot(Snapshot snapshot)
    {
        if (snapshots == null || snapshots[1] == null) return;
        if (snapshot != null && snapshot.players != null)
        {
            //Apply PlayerControlObjects
            for (int i = 0; i < snapshot.players.Length; i++)
            {
                ulong networkId = snapshot.players[i].networkId;
                if (networkId != selfNetworkId) // If Not Local Client Object
                {
                    if (players.ContainsKey(selfNetworkId) && players.ContainsKey(networkId))
                    {
                        Vector3 s_position = snapshot.players[i].location;
                        PlayerControlObject controlObject = players[networkId];

                        if (Vector3.Distance(controlObject.transform.position, snapshot.players[i].location) > 2)
                        {
                            controlObject.transform.position = snapshot.players[i].location;
                        }
                        if (snapshot.players[i].location != controlObject.transform.position)
                        {
                            if (snapshots[1].players != null && snapshots[1].players.ContainsKey(networkId) &&
                                snapshots[0].players != null && snapshots[0].players.ContainsKey(networkId))
                            {
                                Vector3 s_position1 = snapshots[0].players[networkId].location;
                                Vector3 s_position2 = snapshots[1].players[networkId].location;

                                float timeA = snapshot.networkTime - snapshots[0].networkTime;
                                float timeB = snapshots[0].networkTime - snapshots[1].networkTime;

                                Vector3 velocityA = (s_position - s_position1) / timeA;
                                Vector3 velocityB = (s_position1 - s_position2) / timeB;

                                if (velocityA.x != 0) //forward speed
                                {
                                    float animateX = Mathf.Clamp(velocityA.x, -1, 2);
                                    controlObject.animator.SetFloat("Vertical", animateX);
                                }
                                if (velocityA.z != 0) //side speed
                                {
                                    float animateZ = Mathf.Clamp(velocityA.z, -1, 1);
                                    controlObject.animator.SetFloat("Horizontal", animateZ);
                                }
                                players[networkId].moveTarget = s_position + ((velocityA * 2) - velocityB) * timeA;
                            }
                            else 
                            {
                                players[networkId].moveTarget = snapshot.players[i].location;

                                controlObject.animator.SetFloat("Horizontal", 0);
                                controlObject.animator.SetFloat("Vertical", 0);
                            }
                        }
                        if (snapshot.players[i].rotation != new Vector2(controlObject.cameraObject.localRotation.eulerAngles.x, controlObject.transform.localRotation.eulerAngles.y))
                        {
                            controlObject.lookTarget = PredictRotation(networkId, snapshot.networkTime, snapshot.players[i].rotation);
                        }
                    }
                }
                else // (Local Player Rubberbanding)
                {
                    //Local Player Position Correction
                    if (Vector3.Distance(players[networkId].transform.position, PredictPosition(networkId, snapshot.networkTime, snapshot.players[i].location)) > 20)
                    {
                        players[networkId].transform.position = snapshot.players[i].location;
                    }

                    Vector2 serverRot = snapshot.players[i].rotation;
                    Vector2 currentRot = new Vector2(players[networkId].cameraObject.localRotation.eulerAngles.x, players[networkId].transform.localRotation.eulerAngles.y);


                    //Local Player Rotation Correction
                    if (Vector2.Distance(currentRot, serverRot) > 30)
                    {
                        Vector3 playerRot = players[networkId].transform.localRotation.eulerAngles;
                        playerRot.y = snapshot.players[i].rotation.y;
                        players[networkId].transform.localRotation = Quaternion.Euler(playerRot);

                        Vector3 cameraRot = players[networkId].cameraObject.localRotation.eulerAngles;
                        cameraRot.x = snapshot.players[i].rotation.x;
                        players[networkId].cameraObject.localRotation = Quaternion.Euler(cameraRot);
                    }
                }
            }
        }
        if(snapshot != null && snapshot.ai != null) 
        {
            //Apply AIControlObjects
            for (int i = 0; i < snapshot.ai.Length; i++)
            {
                ulong networkId = snapshot.ai[i].networkId;
                if (ai.ContainsKey(networkId)) 
                {
                    AIControlObject controlObject = ai[networkId];
                    Vector3 s_position = snapshot.ai[i].location;
                    
                    //Movement
                    if (Vector3.Distance(controlObject.transform.position, s_position) > 2) //Rubberbanding (Correction)
                    {
                        controlObject.transform.position = snapshot.ai[i].location;
                    }
                    if (snapshot.ai[i].location != controlObject.transform.position) //Prediction
                    {
                        if (snapshots[1].players != null && snapshots[1].players.ContainsKey(networkId) &&
                            snapshots[0].players != null && snapshots[0].players.ContainsKey(networkId))
                        {
                            Vector3 s_position1 = snapshots[0].ai[networkId].location;
                            Vector3 s_position2 = snapshots[1].ai[networkId].location;

                            float timeA = snapshot.networkTime - snapshots[0].networkTime;
                            float timeB = snapshots[0].networkTime - snapshots[1].networkTime;

                            Vector3 velocityA = (s_position - s_position1) / timeA;
                            Vector3 velocityB = (s_position1 - s_position2) / timeB;

                            if (velocityA.x != 0) //forward speed
                            {
                                float animateX = Mathf.Clamp(velocityA.x, -1, 2);
                                controlObject.animator.SetFloat("Vertical", animateX);
                            }
                            if (velocityA.z != 0) //side speed
                            {
                                float animateZ = Mathf.Clamp(velocityA.z, -1, 1);
                                controlObject.animator.SetFloat("Horizontal", animateZ);
                            }
                            controlObject.moveTarget = s_position + ((velocityA * 2) - velocityB) * timeA;
                        }
                        else
                        {
                            controlObject.moveTarget = snapshot.players[i].location;
                        }
                    }
                }
                
                //Rotation
                if (snapshot.ai[i].rotation != new Vector2(ai[networkId].lookUpDownAxis, ai[networkId].transform.rotation.eulerAngles.y))
                {
                    ai[networkId].lookTarget = PredictRotation(networkId, snapshot.networkTime, snapshot.ai[i].rotation);
                }
            }
        }

        snapshots[1] = snapshots[0];
        snapshots[0] = Snapshot.ConvertQuick(snapshot);
    }





    //Predict Object Future Location
    private Vector3 PredictPosition(ulong networkId, float networkTime, Vector3 position0)
    {
        if (players.ContainsKey(networkId)) 
        {
            
            return position0;
        }
        else if (ai.ContainsKey(networkId)) 
        {
            if (snapshots[1].ai != null && snapshots[1].ai.ContainsKey(networkId) &&
                snapshots[0].ai != null && snapshots[0].ai.ContainsKey(networkId))
            {
                Vector3 position1 = snapshots[0].ai[networkId].location;
                Vector3 position2 = snapshots[1].ai[networkId].location;

                float timeA = networkTime - snapshots[0].networkTime;
                float timeB = snapshots[0].networkTime - snapshots[1].networkTime;

                Vector3 velocityA = (position0 - position1) / timeA;
                Vector3 velocityB = (position1 - position2) / timeB;

                if (velocityA.x > 0) //forward speed
                {
                    float animate = Mathf.Clamp(velocityA.x, -1, 2);
                    Debug.Log("FORWARD SPEED:" + velocityA.z);
                }
                if (velocityA.z > 0) //side speed
                {
                    Debug.Log("SIDE SPEED:" + velocityA.x);
                }


                return position0 + ((velocityA * 2) - velocityB) * timeA;
            }
            return position0;
        }
        return position0;
    }

    //Predict Object Future Rotation
    private Vector3 PredictRotation(ulong networkId, float networkTime, Vector2 rotation0)
    {
        if (snapshots[1].players.ContainsKey(networkId))
        {
            Vector2 rotation1 = snapshots[0].players[networkId].rotation;
            Vector2 rotation2 = snapshots[1].players[networkId].rotation;

            float timeA = networkTime - snapshots[0].networkTime;
            float timeB = snapshots[0].networkTime - snapshots[1].networkTime;

            Vector2 velocityA = (rotation0 - rotation1) / timeA;
            Vector2 velocityB = (rotation1 - rotation2) / timeB;

            return rotation0 + ((velocityA * 2) - velocityB) * timeA;
        }
        return rotation0;
    }





}
