using LiteNetLib.Utils;
using UnityEngine;

public class EntityState : INetSerializable
{
    public int Id { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    
    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        Vector3Utils.Serialize(writer, Position);
        QuatUtils.Serialize(writer, Rotation);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        Position = Vector3Utils.Deserialize(reader);
        Rotation = QuatUtils.Deserialize(reader);
    }
}
