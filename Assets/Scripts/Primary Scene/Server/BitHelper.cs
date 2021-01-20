using MLAPI.Serialization.Pooled;
using System.IO;
using UnityEngine;

public class BitHelper : MonoBehaviour
{
    //ClientCommand
    public static ClientCommand[] ConvertClientCommand(ulong _clientId, Stream stream) 
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            ClientCommand[] commands = new ClientCommand[4];
            for (int i = 0; i < 4; i++)
            {
                commands[i] = new ClientCommand()
                {
                    clientId = _clientId,
                    tick = reader.ReadInt32Packed(),
                    move_axis = reader.ReadVector2Packed(),
                    look_axis = reader.ReadVector2Packed(),
                    jump = reader.ReadBool(),
                    crouch = reader.ReadBool(),
                    use = reader.ReadBool(),
                    reload = reader.ReadBool(),
                    aim = reader.ReadBool(),
                    selected_slot = reader.ReadInt16Packed()
                };
            }
            return commands; 
        }
    }
    public static Stream ConvertClientCommand(ClientCommand[] commands) 
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                foreach (ClientCommand command in commands)
                {
                    writer.WriteInt32Packed(command.tick);
                    writer.WriteVector2Packed(command.move_axis);
                    writer.WriteVector2Packed(command.look_axis);
                    writer.WriteBool(command.jump);
                    writer.WriteBool(command.crouch);
                    writer.WriteBool(command.use);
                    writer.WriteBool(command.reload);
                    writer.WriteBool(command.aim);
                    writer.WriteInt16Packed((short) command.selected_slot);
                }
                return writeStream;
            }
        }
    }


    //PackedSnapshot
    public static PackedSnapshot ConvertPackedSnapshot(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            PackedSnapshot snapshot = new PackedSnapshot()
            {
                snapshotId = reader.ReadInt32Packed(),
                networkTime = reader.ReadSinglePacked()
            };

            int playerLength = reader.ReadInt32Packed();
            int aiLength = reader.ReadInt32Packed();
            int worldObjectLength = reader.ReadInt32Packed();

            for (int i = 0; i < playerLength; i++)
            {
                snapshot.players.Add(new Snapshot_Player()
                {
                    networkId = reader.ReadUInt64Packed(),
                    location = reader.ReadVector3Packed(),
                    rotation = reader.ReadVector2Packed(),
                    holdId = reader.ReadInt16Packed()
                });
            }

            for (int i = 0; i < aiLength; i++)
            {
                snapshot.ai.Add(new Snapshot_AI()
                {
                    networkId = reader.ReadUInt64Packed(),
                    location = reader.ReadVector3Packed(),
                    rotation = reader.ReadVector2Packed(),
                    holdId = reader.ReadInt16Packed()
                });
            }

            for (int i = 0; i < worldObjectLength; i++)
            {
                snapshot.worldObjects.Add(new Snapshot_WorldObject()
                {
                    spawnId = reader.ReadInt32Packed(),
                    objectId = reader.ReadInt32Packed()
                });
            }
            return snapshot;
        }
    }
    public static Stream ConvertPackedSnapshot(PackedSnapshot snapshot)
    {
        using (PooledBitStream writeStream = PooledBitStream.Get())
        {
            using (PooledBitWriter writer = PooledBitWriter.Get(writeStream))
            {
                writer.WriteInt32Packed(snapshot.snapshotId);
                writer.WriteSinglePacked(snapshot.networkTime);

                int playerLength = snapshot.players.Count;
                int aiLength = snapshot.ai.Count;
                int worldObjectLength = snapshot.worldObjects.Count;

                writer.WriteInt32Packed(playerLength);
                writer.WriteInt32Packed(aiLength);
                writer.WriteInt32Packed(worldObjectLength);

                for (int i = 0; i < playerLength; i++)
                {
                    writer.WriteUInt64Packed(snapshot.players[i].networkId);
                    writer.WriteVector3Packed(snapshot.players[i].location);
                    writer.WriteVector2Packed(snapshot.players[i].rotation);
                    writer.WriteInt16Packed((short)snapshot.players[i].holdId);
                }

                for (int i = 0; i < aiLength; i++)
                {
                    writer.WriteUInt64Packed(snapshot.ai[i].networkId);
                    writer.WriteVector3Packed(snapshot.ai[i].location);
                    writer.WriteVector2Packed(snapshot.ai[i].rotation);
                    writer.WriteInt16Packed((short)snapshot.ai[i].holdId);
                }

                for (int i = 0; i < worldObjectLength; i++)
                {
                    writer.WriteInt32Packed(snapshot.worldObjects[i].spawnId);
                    writer.WriteInt32Packed(snapshot.worldObjects[i].objectId);
                }

                return writeStream;
            }
        }
    }
}
