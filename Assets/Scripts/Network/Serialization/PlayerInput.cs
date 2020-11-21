using LiteNetLib.Utils;
using UnityEngine;

public class PlayerInput : INetSerializable
{
    public Vector3 HeadPosition { get; set; }
    public Vector3 HeadRotation { get; set; }
    public Vector3 LeftHandPosition { get; set; }
    public Vector3 LeftHandRotation { get; set; }
    public Vector3 RightHandPosition { get; set; }
    public Vector3 RightHandRotation { get; set; }
    public int LeftGrabId { get; set; }
    public Vector3 LeftGrabPosition { get; set; }
    public Vector3 LeftGrabRotation { get; set; }
    public Vector3 RightGrabPosition { get; set; }
    public Vector3 RightGrabRotation { get; set; }
    public int RightGrabId { get; set; }
    public bool LeftShooting { get; set; }
    public bool RightShooting { get; set; }
    public bool LeftPointer { get; set; }
    public bool RightPointer { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        Vector3Utils.Serialize(writer, HeadPosition);
        Vector3Utils.Serialize(writer, HeadRotation);
        Vector3Utils.Serialize(writer, LeftHandPosition);
        Vector3Utils.Serialize(writer, LeftHandRotation);
        Vector3Utils.Serialize(writer, RightHandPosition);
        Vector3Utils.Serialize(writer, RightHandRotation);
        writer.Put(LeftGrabId);
        Vector3Utils.Serialize(writer, LeftGrabPosition);
        Vector3Utils.Serialize(writer, LeftGrabRotation);
        Vector3Utils.Serialize(writer, RightGrabPosition);
        Vector3Utils.Serialize(writer, RightGrabRotation);
        writer.Put(RightGrabId);
        writer.Put(LeftShooting);
        writer.Put(RightShooting);
    }

    public void Deserialize(NetDataReader reader)
    {
        HeadPosition = Vector3Utils.Deserialize(reader);
        HeadRotation = Vector3Utils.Deserialize(reader);
        LeftHandPosition = Vector3Utils.Deserialize(reader);
        LeftHandRotation = Vector3Utils.Deserialize(reader);
        RightHandPosition = Vector3Utils.Deserialize(reader);
        RightHandRotation = Vector3Utils.Deserialize(reader);
        LeftGrabId = reader.GetInt();
        LeftGrabPosition = Vector3Utils.Deserialize(reader);
        LeftGrabRotation = Vector3Utils.Deserialize(reader);
        RightGrabPosition = Vector3Utils.Deserialize(reader);
        RightGrabRotation = Vector3Utils.Deserialize(reader);
        RightGrabId = reader.GetInt();
        LeftShooting = reader.GetBool();
        RightShooting = reader.GetBool();
    }
}
