using LiteNetLib.Utils;
using UnityEngine;

public enum EntityType
{
    Bullet = 0,
    Ball = 1,
    Pin = 2,
    Gun = 3,
    LinearLever = 4,
    Door = 5
}
public class EntityState : INetSerializable
{
    public int Id { get; set; }
    public byte Type { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    
    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(Type);
        Vector3Utils.Serialize(writer, Position);
        Vector3Utils.Serialize(writer, Rotation);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        Type = reader.GetByte();
        Position = Vector3Utils.Deserialize(reader);
        Rotation = Vector3Utils.Deserialize(reader);
    }
}
