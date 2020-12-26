using LiteNetLib.Utils;
using UnityEngine;
public class PlayerRemoveMessage : INetSerializable
{
    public int Id { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetSByte();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((sbyte)Id);
    }

    public PlayerRemoveMessage Clone()
    {
        PlayerRemoveMessage clone = new PlayerRemoveMessage()
        {
            Id = Id,
        };
        return clone;
    }
}
