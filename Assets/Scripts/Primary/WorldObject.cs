using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WorldObject : NetworkedBehaviour
{
    //-----------------------------------------------------------------//
    //                            World Object                         //
    //-----------------------------------------------------------------//

    public NetworkedObject networkObject;    
        
    // 1 - Tree
    // 2 - Rock
    // 3 - Ore
    //Object Type
    public int objectType;
    //Object ID
    public int objectId;
    //Object Amount
    public int objectAmount;
    //Object Destroyed
    public float objectDestroyedTime;

    //Data Recieved
    private bool dataRecieved = false;

    //-----------------------------------------------------------------//
    //              Client Request for World Object Data               //
    //-----------------------------------------------------------------//

    //Initialize Request for World Object Data
    public void RequestObjectData(Action<WorldObjectData> dataCallback = null) 
    {
        StartCoroutine(GetObjectDataWait(objectData =>
        {
            dataCallback(objectData);
        }));
    }

    //Wait and Return or Timeout
    private IEnumerator GetObjectDataWait(Action<WorldObjectData> objectData = null) 
    {
        dataRecieved = false;
        bool waitingForData = false;
        //Run Data Request
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteBool(true);
                InvokeServerRpcPerformance(RequestClickableDataRpc, writeStream);
                waitingForData = true;
            }
        }
        int timeout = 30;
        while (waitingForData)
        {
            yield return new WaitForSeconds(.1F);
            if (dataRecieved)
            {
                dataRecieved = false;
                objectData(new WorldObjectData()
                {
                    objectType_data = objectType,
                    objectId_data = objectId,
                    objectAmount_data = objectAmount,
                    objectDestroyedTime_data = objectDestroyedTime
                });
                break;
            }
            timeout--;
            if (timeout <= 0)
            {
                break;
            }
        }
    }

    //Server Send
    [ServerRPC(RequireOwnership = false)]
    private void RequestClickableDataRpc(ulong clientId, Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            if (reader.ReadBool()) 
            {
                using (PooledBitStream writeStream = PooledBitStream.Get())
                {
                    using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
                    {
                        writer.WriteInt32Packed(objectType);
                        writer.WriteInt32Packed(objectId);
                        writer.WriteInt32Packed(objectAmount);
                        InvokeClientRpcOnClientPerformance(SendClickableDataRpc, clientId, writeStream);
                    }
                }
            }
        }
    }

    //Client Recieve
    [ClientRPC]
    private void SendClickableDataRpc(ulong clientId, Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            objectType = reader.ReadInt32Packed();
            objectId = reader.ReadInt32Packed();
            objectAmount = reader.ReadInt32Packed();
            dataRecieved = true;
        }
    }

    //-----------------------------------------------------------------//
    //                       Server Requests                           //
    //-----------------------------------------------------------------//

    

}


public class WorldObjectData 
{
    public int objectType_data;
    public int objectId_data;
    public int objectAmount_data;
    public float objectDestroyedTime_data;
}
