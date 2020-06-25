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
    
    
    private void Awake()
    {
        //Serialization for <Item> Object. 
        SerializationManager.RegisterSerializationHandlers<Item>((Stream stream, Item instance) =>
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(stream))
            {
                writer.WriteInt32Packed(instance.itemID);
                writer.WriteInt32Packed(instance.itemStack);
                writer.WriteInt32Packed(instance.maxItemStack);
                writer.WriteInt32Packed(instance.currSlot);
                writer.WriteInt32Packed(instance.armorType);

                writer.WriteStringPacked(instance.special);

                writer.WriteBool(instance.isCraftable);
                writer.WriteBool(instance.isHoldable);
                writer.WriteBool(instance.isArmor);
                writer.WriteBool(instance.showInInventory);
            }
        }, (Stream stream) =>
        {
            using (PooledBitReader reader = PooledBitReader.Get(stream))
            {
                Item item = new Item();
                item.itemID = reader.ReadInt32Packed();
                item.itemStack = reader.ReadInt32Packed();
                item.maxItemStack = reader.ReadInt32Packed();
                item.currSlot = reader.ReadInt32Packed();
                item.armorType = reader.ReadInt32Packed();

                item.special = reader.ReadStringPacked().ToString();

                item.isCraftable = reader.ReadBool();
                item.isHoldable = reader.ReadBool();
                item.isArmor = reader.ReadBool();
                item.showInInventory = reader.ReadBool();
                return item;
            }
        });
    }

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