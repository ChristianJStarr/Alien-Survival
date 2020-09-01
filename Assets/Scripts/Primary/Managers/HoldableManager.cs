using MLAPI;
using MLAPI.Messaging;
using MLAPI.Serialization.Pooled;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class HoldableManager : NetworkedBehaviour
{
    public Transform holdableParent;

    //Managers
    private NetworkingManager networkManager;
    private PlayerActionManager actionManager;

    //Time
    public int FixedSendDistance = 20;
    public float FixedSendsPerSecond = 20f;
    private float lastSendTime;

    //Current: Held Object
    private int currentHeldObject = 0;

    //Arrays
    public GameObject[] holdablePrefabs;
    private List<HoldableObject> holdableObjects = new List<HoldableObject>();



    private void Start()
    {
        networkManager = NetworkingManager.Singleton;
        actionManager = PlayerActionManager.singleton;
    }
    private void Update()
    {
        if (IsOwner)
        {
            if (networkManager.NetworkTime - lastSendTime >= (1f / FixedSendsPerSecond))
            {
                lastSendTime = networkManager.NetworkTime;
                Client_PulloutHoldable(currentHeldObject);
            }
        }
    }



    //USE: Holdable
    public void UseHoldable(int holdableId)
    {
        if (currentHeldObject == holdableId)
        {
            if (IsOwner)
            {
                Client_UseHoldable(holdableId);
            }
            for (int i = 0; i < holdableObjects.Count; i++)
            {
                if (holdableObjects[i] != null && holdableObjects[i].id == holdableId && holdableObjects[i].gameObject.activeSelf)
                {
                    holdableObjects[i].animator.SetTrigger("Use");
                    break;
                }
            }
        }
    }

    //PULLOUT: Holdable
    public void PulloutHoldable(int holdableId) 
    {
        if(currentHeldObject != holdableId)
        {
            currentHeldObject = holdableId;
            if (IsOwner)
            {
                Client_PulloutHoldable(holdableId);
            }
            if (holdableId != 0) //Pullout
            {
                PutAwayHoldables();
                //Check if Holdable is Pooled

                for (int i = 0; i < holdableObjects.Count; i++)
                {
                    if (holdableObjects[i] != null && holdableObjects[i].id == holdableId)
                    {
                        holdableObjects[i].gameObject.SetActive(true);
                        return;
                    }
                }
                //Else SpawnHoldable
                if (holdablePrefabs[holdableId - 1] != null)
                {
                    GameObject holdable = Instantiate(holdablePrefabs[holdableId - 1], holdableParent);
                    HoldableObject holdableObject = holdable.GetComponent<HoldableObject>();
                    holdableObjects.Add(holdableObject);
                }
            }
            else //Put Away 
            {
                PutAwayHoldables();
            }
        }
    }

    //Put Away Holdable
    private void PutAwayHoldables() 
    {
        foreach (HoldableObject holdable in holdableObjects)
        {
            if (holdable.gameObject.activeSelf)
            {
                StartCoroutine(PutAwayHoldableDelay(holdable));
            }
        }
        if(holdableObjects.Count > 5) 
        {
            for (int i = 0; i < holdableObjects.Count; i++)
            {
                if(holdableObjects[i] != null) 
                {
                    Destroy(holdableObjects[i].gameObject);
                }
            }
            holdableObjects.Clear();
        }
    }
    private IEnumerator PutAwayHoldableDelay(HoldableObject holdable) 
    {
        holdable.animator.SetTrigger("PutAway");
        yield return new WaitForSeconds(.5F);
        holdable.gameObject.SetActive(false);
    }

    



    //-----------------//
    //   Network Sync  //
    //-----------------//

    //Pullout
    private void Client_PulloutHoldable(int holdableId)
    {
        using (PooledBitStream stream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(stream))
            {
                writer.WriteInt32Packed(holdableId);
                InvokeServerRpcPerformance(PulloutHoldable_ServerRpc, stream);
            }
        }
    }

    [ServerRPC]
    private void PulloutHoldable_ServerRpc(ulong clientId, Stream stream)
    {
        ulong[] connectedClients = networkManager.ConnectedClients.Keys.ToArray();
        for (int i = 0; i < connectedClients.Length; i++)
        {
            if (connectedClients[i] != clientId && Vector3.Distance(transform.position, networkManager.ConnectedClients[connectedClients[i]].PlayerObject.transform.position) < FixedSendDistance)
            {
                InvokeClientRpcOnClientPerformance(PulloutHoldable_ClientRpc, connectedClients[i], stream);
            }
        }
    }

    [ClientRPC]
    private void PulloutHoldable_ClientRpc(ulong clientId, Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            
        }
    }

    //Use
    private void Client_UseHoldable(int holdableId)
    {
        using (PooledBitStream stream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(stream))
            {
                writer.WriteInt32Packed(holdableId);
                InvokeServerRpcPerformance(UseHoldable_ServerRpc, stream);
            }
        }
    }

    [ServerRPC]
    private void UseHoldable_ServerRpc(ulong clientId, Stream stream)
    {
        ulong[] connectedClients = networkManager.ConnectedClients.Keys.ToArray();
        for (int i = 0; i < connectedClients.Length; i++)
        {
            if (connectedClients[i] != clientId && Vector3.Distance(transform.position, networkManager.ConnectedClients[connectedClients[i]].PlayerObject.transform.position) < FixedSendDistance)
            {
                InvokeClientRpcOnClientPerformance(UseHoldable_ClientRpc, connectedClients[i], stream);
            }
        }
    }

    [ClientRPC]
    private void UseHoldable_ClientRpc(ulong clientId, Stream stream)
    {
        using(PooledBitReader reader = PooledBitReader.Get(stream)) 
        {
            UseHoldable(reader.ReadInt32Packed());
        }
    }

}
