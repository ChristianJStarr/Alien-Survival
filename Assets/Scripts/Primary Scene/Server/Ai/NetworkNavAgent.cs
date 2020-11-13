using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

public class NetworkNavAgent : NetworkedBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    public bool EnableProximity = false;
    
    [Range(0,200)]
    public float ProximityRange = 200F;


    public AnimationCurve distanceFalloff = AnimationCurve.Constant(0, 200, 20);

    [Range(0,120), Tooltip("Corrections Per Second")]
    public int CorrectionDelay = 5;
    
    [Range(0,1), Tooltip("Correction Percentage")]
    public float DriftCorrectionPercentage = 0.1f;
    public bool WarpOnDestinationChange = false;
    public string channel = "AIMovement";

    
    private Vector3 lastDestination = Vector3.zero;
    private float lastCorrectionTime = 0f;
   
    
    
    
    private void Update()
    {
        if (!IsServer)
            return;

        if (agent.destination != lastDestination)
        {
            lastDestination = agent.destination;
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {

                    writer.WriteSinglePacked(agent.destination.x);
                    writer.WriteSinglePacked(agent.destination.y);
                    writer.WriteSinglePacked(agent.destination.z);

                    writer.WriteSinglePacked(agent.velocity.x);
                    writer.WriteSinglePacked(agent.velocity.y);
                    writer.WriteSinglePacked(agent.velocity.z);

                    writer.WriteSinglePacked(transform.position.x);
                    writer.WriteSinglePacked(transform.position.y);
                    writer.WriteSinglePacked(transform.position.z);


                    if (!EnableProximity)
                    {
                        InvokeClientRpcOnEveryonePerformance(OnNavMeshStateUpdate, stream, channel);
                    }
                    else
                    {
                        List<ulong> proximityClients = new List<ulong>();
                        foreach (KeyValuePair<ulong, NetworkedClient> client in NetworkingManager.Singleton.ConnectedClients)
                        {
                            if (client.Value.PlayerObject == null || Vector3.Distance(client.Value.PlayerObject.transform.position, transform.position) <= ProximityRange)
                                proximityClients.Add(client.Key);
                        }
                        InvokeClientRpcPerformance(OnNavMeshStateUpdate, proximityClients, stream, channel);
                    }
                }
            }
        }

        if (NetworkingManager.Singleton.NetworkTime - lastCorrectionTime >= (1F / CorrectionDelay))
        {
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteSinglePacked(agent.velocity.x);
                    writer.WriteSinglePacked(agent.velocity.y);
                    writer.WriteSinglePacked(agent.velocity.z);

                    writer.WriteSinglePacked(transform.position.x);
                    writer.WriteSinglePacked(transform.position.y);
                    writer.WriteSinglePacked(transform.position.z);


                    if (!EnableProximity)
                    {
                        InvokeClientRpcOnEveryonePerformance(OnNavMeshCorrectionUpdate, stream, channel);
                    }
                    else
                    {
                        List<ulong> proximityClients = new List<ulong>();
                        foreach (KeyValuePair<ulong, NetworkedClient> client in NetworkingManager.Singleton.ConnectedClients)
                        {
                            if (client.Value.PlayerObject == null || Vector3.Distance(client.Value.PlayerObject.transform.position, transform.position) <= ProximityRange)
                                proximityClients.Add(client.Key);
                        }
                        InvokeClientRpcPerformance(OnNavMeshCorrectionUpdate, proximityClients, stream, channel);
                    }
                }
            }
            lastCorrectionTime = NetworkingManager.Singleton.NetworkTime;
        }
    }

    [ClientRPC]
    private void OnNavMeshStateUpdate(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            float xDestination = reader.ReadSinglePacked();
            float yDestination = reader.ReadSinglePacked();
            float zDestination = reader.ReadSinglePacked();

            float xVel = reader.ReadSinglePacked();
            float yVel = reader.ReadSinglePacked();
            float zVel = reader.ReadSinglePacked();

            float xPos = reader.ReadSinglePacked();
            float yPos = reader.ReadSinglePacked();
            float zPos = reader.ReadSinglePacked();

            Vector3 destination = new Vector3(xDestination, yDestination, zDestination);
            Vector3 velocity = new Vector3(xVel, yVel, zVel);
            Vector3 position = new Vector3(xPos, yPos, zPos);

            if (WarpOnDestinationChange)
                agent.Warp(position);
            else
                agent.Warp(Vector3.Lerp(transform.position, position, DriftCorrectionPercentage));

            agent.SetDestination(destination);
            agent.velocity = velocity;
        }
    }

    [ClientRPC]
    private void OnNavMeshCorrectionUpdate(ulong clientId, Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            float xVel = reader.ReadSinglePacked();
            float yVel = reader.ReadSinglePacked();
            float zVel = reader.ReadSinglePacked();

            float xPos = reader.ReadSinglePacked();
            float yPos = reader.ReadSinglePacked();
            float zPos = reader.ReadSinglePacked();

            Vector3 velocity = new Vector3(xVel, yVel, zVel);
            Vector3 position = new Vector3(xPos, yPos, zPos);

            agent.Warp(Vector3.Lerp(transform.position, position, DriftCorrectionPercentage));
            agent.velocity = velocity;
        }
    }
}
