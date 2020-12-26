using LiteNetLib.Utils;
using UnityEngine;
public class EntityRemoveMessage : INetSerializable
{
    public int Id { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
    }

    public EntityRemoveMessage Clone()
    {
        EntityRemoveMessage clone = new EntityRemoveMessage()
        {
            Id = Id,
        };
        return clone;
    }
}
