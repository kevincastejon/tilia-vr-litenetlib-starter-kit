using LiteNetLib.Utils;
using UnityEngine;

public class PlayerInput : INetSerializable
{
    public int Sequence { get; set; }
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
    public bool LeftTrigger { get; set; }
    public bool RightTrigger { get; set; }
    public bool LeftPointer { get; set; }
    public bool RightPointer { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Sequence);
        Vector3Utils.Serialize(writer, HeadPosition);
        QuatUtils.Serialize(writer, HeadRotation);
        Vector3Utils.SerializeHand(writer, LeftHandPosition, HeadPosition);
        QuatUtils.Serialize(writer, LeftHandRotation);
        Vector3Utils.SerializeHand(writer, RightHandPosition, HeadPosition);
        QuatUtils.Serialize(writer, RightHandRotation);
        writer.Put(LeftGrabId);
        Vector3Utils.SerializeHand(writer, LeftGrabPosition, HeadPosition);
        Vector3Utils.Serialize(writer, LeftGrabVelocity);
        QuatUtils.Serialize(writer, LeftGrabRotation);
        Vector3Utils.Serialize(writer, LeftGrabAngularVelocity);
        Vector3Utils.SerializeHand(writer, RightGrabPosition, HeadPosition);
        Vector3Utils.Serialize(writer, RightGrabVelocity);
        QuatUtils.Serialize(writer, RightGrabRotation);
        Vector3Utils.Serialize(writer, RightGrabAngularVelocity);
        writer.Put(RightGrabId);
        writer.Put(LeftTrigger);
        writer.Put(RightTrigger);
        writer.Put(LeftPointer);
        writer.Put(RightPointer);
    }

    public void Deserialize(NetDataReader reader)
    {
        Sequence = reader.GetInt();
        HeadPosition = Vector3Utils.Deserialize(reader);
        HeadRotation = QuatUtils.Deserialize(reader);
        LeftHandPosition = Vector3Utils.DeserializeHand(reader, HeadPosition);
        LeftHandRotation = QuatUtils.Deserialize(reader);
        RightHandPosition = Vector3Utils.DeserializeHand(reader, HeadPosition);
        RightHandRotation = QuatUtils.Deserialize(reader);
        LeftGrabId = reader.GetInt();
        LeftGrabPosition = Vector3Utils.DeserializeHand(reader, HeadPosition);
        LeftGrabVelocity = Vector3Utils.Deserialize(reader);
        LeftGrabRotation = QuatUtils.Deserialize(reader);
        LeftGrabAngularVelocity = Vector3Utils.Deserialize(reader);
        RightGrabPosition = Vector3Utils.DeserializeHand(reader, HeadPosition);
        RightGrabVelocity = Vector3Utils.Deserialize(reader);
        RightGrabRotation = QuatUtils.Deserialize(reader);
        RightGrabAngularVelocity = Vector3Utils.Deserialize(reader);
        RightGrabId = reader.GetInt();
        LeftTrigger = reader.GetBool();
        RightTrigger = reader.GetBool();
        LeftPointer = reader.GetBool();
        RightPointer = reader.GetBool();
    }

    public PlayerInput Clone()
    {
        return new PlayerInput()
        {
            Sequence = Sequence,
            HeadPosition = new Vector3(HeadPosition.x, HeadPosition.y, HeadPosition.z),
            HeadRotation = new Quaternion(HeadRotation.x, HeadRotation.y, HeadRotation.z, HeadRotation.w),
            LeftHandPosition = new Vector3(LeftHandPosition.x, LeftHandPosition.y, LeftHandPosition.z),
            LeftHandRotation = new Quaternion(LeftHandRotation.x, LeftHandRotation.y, LeftHandRotation.z, LeftHandRotation.w),
            RightHandPosition = new Vector3(RightHandPosition.x, RightHandPosition.y, RightHandPosition.z),
            RightHandRotation = new Quaternion(RightHandRotation.x, RightHandRotation.y, RightHandRotation.z, RightHandRotation.w),
            LeftGrabId = LeftGrabId,
            LeftGrabPosition = LeftGrabPosition,
            LeftGrabRotation = LeftGrabRotation,
            LeftGrabVelocity = LeftGrabVelocity,
            LeftGrabAngularVelocity = LeftGrabAngularVelocity,
            RightGrabId = RightGrabId,
            RightGrabPosition = RightGrabPosition,
            RightGrabRotation = RightGrabRotation,
            RightGrabVelocity = RightGrabVelocity,
            RightGrabAngularVelocity = RightGrabAngularVelocity,
            LeftPointer = LeftPointer,
            RightPointer = RightPointer,
            LeftTrigger = LeftTrigger,
            RightTrigger = RightTrigger,
        };
    }
}
