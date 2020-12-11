using LiteNetLib.Utils;
using UnityEngine;
public class EntityState : INetSerializable
{
    public int Id { get; set; }
    public byte Type { get; set; }
    public Vector3 Position { get; set; }
    public Quaternion Rotation { get; set; }
    public int Owner { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(Type);
        Vector3Utils.Serialize(writer, Position);
        QuatUtils.Serialize(writer, Rotation);
        writer.Put((sbyte)Id);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        Type = reader.GetByte();
        Position = Vector3Utils.Deserialize(reader);
        Rotation = QuatUtils.Deserialize(reader);
        Owner = reader.GetSByte();
    }

    public EntityState Clone()
    {
        EntityState clone = new EntityState()
        {
            Id = Id,
            Type = Type,
            Position = new Vector3(Position.x, Position.y, Position.z),
            Rotation = new Quaternion(Rotation.x, Rotation.y, Rotation.z, Rotation.w),
            Owner = Owner,
        };
        return clone;
    }
}
