using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class DeathDrop : NetworkedBehaviour
{
    [SyncedVar]
    public string unique;
    [SyncedVar]
    public string toolTip;

    public List<Item> dropItems;
    private bool isStarted = false;
    

    //On Start
    public override void NetworkStart()
    {
        if (IsServer) 
        {
            isStarted = true;
            InvokeClientRpcOnEveryone(UpdateItemList, dropItems.ToArray());
        }
    }
    
    //Update Item List
    public void UpdateDropItems(List<Item> items = null)
    {
        if(items == null) 
        {
            if (IsServer)
            {
                if (isStarted)
                {
                    if(dropItems.Count == 0) 
                    {
                        Destroy(gameObject);
                        return;
                    }
                    InvokeClientRpcOnEveryone(UpdateItemList, dropItems.ToArray());
                }
            }
        }
        else 
        {
            if (IsServer)
            {
                dropItems = items;
                if (isStarted)
                {
                    if (dropItems.Count == 0)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    InvokeClientRpcOnEveryone(UpdateItemList, dropItems.ToArray());
                }
            }
        }
    }

    //Update List RPC
    [ClientRPC]
    private void UpdateItemList(Item[] items) 
    {
        dropItems = items.ToList();
    }

}