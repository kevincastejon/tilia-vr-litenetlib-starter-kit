using LiteNetLib.Utils;
using UnityEngine;

public class PlayerInitInfo : INetSerializable
{
    public string OculusID { get; set; }
    public Vector3 HeadPosition { get; set; }
    public Quaternion HeadRotation { get; set; }
    public Vector3 LeftHandPosition { get; set; }
    public Quaternion LeftHandRotation { get; set; }
    public Vector3 RightHandPosition { get; set; }
    public Quaternion RightHandRotation { get; set; }
    public bool LeftTrigger { get; set; }
    public bool RightTrigger { get; set; }
    public bool LeftPointer { get; set; }
    public bool RightPointer { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(OculusID);
        Vector3Utils.Serialize(writer, HeadPosition);
        QuatUtils.Serialize(writer, HeadRotation);
        Vector3Utils.SerializeHand(writer, LeftHandPosition, HeadPosition);
        QuatUtils.Serialize(writer, LeftHandRotation);
        Vector3Utils.SerializeHand(writer, RightHandPosition, HeadPosition);
        QuatUtils.Serialize(writer, RightHandRotation);
        writer.Put(LeftTrigger);
        writer.Put(RightTrigger);
        writer.Put(LeftPointer);
        writer.Put(RightPointer);
    }

    public void Deserialize(NetDataReader reader)
    {
        OculusID = reader.GetString();
        HeadPosition = Vector3Utils.Deserialize(reader);
        HeadRotation = QuatUtils.Deserialize(reader);
        LeftHandPosition = Vector3Utils.DeserializeHand(reader, HeadPosition);
        LeftHandRotation = QuatUtils.Deserialize(reader);
        RightHandPosition = Vector3Utils.DeserializeHand(reader, HeadPosition);
        RightHandRotation = QuatUtils.Deserialize(reader);
        LeftTrigger = reader.GetBool();
        RightTrigger = reader.GetBool();
        LeftPointer = reader.GetBool();
        RightPointer = reader.GetBool();
    }

    public PlayerInitInfo Clone()
    {
        return new PlayerInitInfo()
        {
            OculusID = OculusID,
            HeadPosition = new Vector3(HeadPosition.x, HeadPosition.y, HeadPosition.z),
            HeadRotation = new Quaternion(HeadRotation.x, HeadRotation.y, HeadRotation.z, HeadRotation.w),
            LeftHandPosition = new Vector3(LeftHandPosition.x, LeftHandPosition.y, LeftHandPosition.z),
            LeftHandRotation = new Quaternion(LeftHandRotation.x, LeftHandRotation.y, LeftHandRotation.z, LeftHandRotation.w),
            RightHandPosition = new Vector3(RightHandPosition.x, RightHandPosition.y, RightHandPosition.z),
            RightHandRotation = new Quaternion(RightHandRotation.x, RightHandRotation.y, RightHandRotation.z, RightHandRotation.w),
            LeftPointer = LeftPointer,
            RightPointer = RightPointer,
            LeftTrigger = LeftTrigger,
            RightTrigger = RightTrigger,
        };
    }
}
