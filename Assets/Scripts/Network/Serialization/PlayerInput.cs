using LiteNetLib.Utils;
using UnityEngine;

public class PlayerInput : INetSerializable
{
    public Vector3 HeadPosition { get; set; }
    public Quaternion HeadRotation { get; set; }
    public Vector3 LeftHandPosition { get; set; }
    public Quaternion LeftHandRotation { get; set; }
    public Vector3 RightHandPosition { get; set; }
    public Quaternion RightHandRotation { get; set; }
    public int LeftGrabId { get; set; }
    public Vector3 LeftGrabPosition { get; set; }
    public Vector3 LeftGrabVelocity { get; set; }
    public Quaternion LeftGrabRotation { get; set; }
    public Vector3 LeftGrabAngularVelocity { get; set; }
    public Vector3 RightGrabPosition { get; set; }
    public Vector3 RightGrabVelocity { get; set; }
    public Quaternion RightGrabRotation { get; set; }
    public Vector3 RightGrabAngularVelocity { get; set; }
    public int RightGrabId { get; set; }
    public bool LeftShooting { get; set; }
    public bool RightShooting { get; set; }
    public bool LeftPointer { get; set; }
    public bool RightPointer { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        Vector3Utils.Serialize(writer, HeadPosition);
        QuatUtils.Serialize(writer, HeadRotation);
        Vector3Utils.Serialize(writer, LeftHandPosition);
        QuatUtils.Serialize(writer, LeftHandRotation);
        Vector3Utils.Serialize(writer, RightHandPosition);
        QuatUtils.Serialize(writer, RightHandRotation);
        writer.Put(LeftGrabId);
        Vector3Utils.Serialize(writer, LeftGrabPosition);
        Vector3Utils.Serialize(writer, LeftGrabVelocity);
        QuatUtils.Serialize(writer, LeftGrabRotation);
        Vector3Utils.Serialize(writer, LeftGrabAngularVelocity);
        Vector3Utils.Serialize(writer, RightGrabPosition);
        Vector3Utils.Serialize(writer, RightGrabVelocity);
        QuatUtils.Serialize(writer, RightGrabRotation);
        Vector3Utils.Serialize(writer, RightGrabAngularVelocity);
        writer.Put(RightGrabId);
        writer.Put(LeftShooting);
        writer.Put(RightShooting);
        writer.Put(LeftPointer);
        writer.Put(RightPointer);
    }

    public void Deserialize(NetDataReader reader)
    {
        HeadPosition = Vector3Utils.Deserialize(reader);
        HeadRotation = QuatUtils.Deserialize(reader);
        LeftHandPosition = Vector3Utils.Deserialize(reader);
        LeftHandRotation = QuatUtils.Deserialize(reader);
        RightHandPosition = Vector3Utils.Deserialize(reader);
        RightHandRotation = QuatUtils.Deserialize(reader);
        LeftGrabId = reader.GetInt();
        LeftGrabPosition = Vector3Utils.Deserialize(reader);
        LeftGrabVelocity = Vector3Utils.Deserialize(reader);
        LeftGrabRotation = QuatUtils.Deserialize(reader);
        LeftGrabAngularVelocity = Vector3Utils.Deserialize(reader);
        RightGrabPosition = Vector3Utils.Deserialize(reader);
        RightGrabVelocity = Vector3Utils.Deserialize(reader);
        RightGrabRotation = QuatUtils.Deserialize(reader);
        RightGrabAngularVelocity = Vector3Utils.Deserialize(reader);
        RightGrabId = reader.GetInt();
        LeftShooting = reader.GetBool();
        RightShooting = reader.GetBool();
        LeftPointer = reader.GetBool();
        RightPointer = reader.GetBool();
    }
}
