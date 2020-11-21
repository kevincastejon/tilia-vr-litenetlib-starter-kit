using LiteNetLib.Utils;
using UnityEngine;

public class PlayerState : INetSerializable
{
    public int Id { get; set; }
    public Vector3 HeadPosition { get; set; }
    public Vector3 HeadRotation { get; set; }
    public Vector3 LeftHandPosition { get; set; }
    public Vector3 LeftHandRotation { get; set; }
    public Vector3 RightHandPosition { get; set; }
    public Vector3 RightHandRotation { get; set; }
    public bool LeftPointer { get; set; }
    public bool RightPointer { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        Vector3Utils.Serialize(writer, HeadPosition);
        Vector3Utils.Serialize(writer, HeadRotation);
        Vector3Utils.Serialize(writer, LeftHandPosition);
        Vector3Utils.Serialize(writer, LeftHandRotation);
        Vector3Utils.Serialize(writer, RightHandPosition);
        Vector3Utils.Serialize(writer, RightHandRotation);
        writer.Put(LeftPointer);
        writer.Put(RightPointer);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        HeadPosition = Vector3Utils.Deserialize(reader);
        HeadRotation = Vector3Utils.Deserialize(reader);
        LeftHandPosition = Vector3Utils.Deserialize(reader);
        LeftHandRotation = Vector3Utils.Deserialize(reader);
        RightHandPosition = Vector3Utils.Deserialize(reader);
        RightHandRotation = Vector3Utils.Deserialize(reader);
        LeftPointer = reader.GetBool();
        RightPointer = reader.GetBool();
    }
}
