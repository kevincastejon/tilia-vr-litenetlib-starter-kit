using LiteNetLib.Utils;
using UnityEngine;

public class PlayerInput : INetSerializable
{
    public int Id { get; set; }
    public Vector3 HeadPosition { get; set; }
    public Quaternion HeadRotation { get; set; }
    public Vector3 LeftHandPosition { get; set; }
    public Quaternion LeftHandRotation { get; set; }
    public Vector3 RightHandPosition { get; set; }
    public Quaternion RightHandRotation { get; set; }
    public Vector3 LeftGrabPosition { get; set; }
    public Quaternion LeftGrabRotation { get; set; }
    public Vector3 RightGrabPosition { get; set; }
    public Quaternion RightGrabRotation { get; set; }
    public bool Shooting { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        Vector3Utils.Serialize(writer, HeadPosition);
        QuatUtils.Serialize(writer, HeadRotation);
        Vector3Utils.Serialize(writer, LeftHandPosition);
        QuatUtils.Serialize(writer, LeftHandRotation);
        Vector3Utils.Serialize(writer, RightHandPosition);
        QuatUtils.Serialize(writer, RightHandRotation);
        Vector3Utils.Serialize(writer, LeftGrabPosition);
        QuatUtils.Serialize(writer, LeftGrabRotation);
        Vector3Utils.Serialize(writer, RightGrabPosition);
        QuatUtils.Serialize(writer, RightGrabRotation);
        writer.Put(Shooting);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetInt();
        HeadPosition = Vector3Utils.Deserialize(reader);
        HeadRotation = QuatUtils.Deserialize(reader);
        LeftHandPosition = Vector3Utils.Deserialize(reader);
        LeftHandRotation = QuatUtils.Deserialize(reader);
        RightHandPosition = Vector3Utils.Deserialize(reader);
        RightHandRotation = QuatUtils.Deserialize(reader);
        LeftGrabPosition = Vector3Utils.Deserialize(reader);
        LeftGrabRotation = QuatUtils.Deserialize(reader);
        RightGrabPosition = Vector3Utils.Deserialize(reader);
        RightGrabRotation = QuatUtils.Deserialize(reader);
        Shooting = reader.GetBool();
    }
}
