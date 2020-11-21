using LiteNetLib.Utils;
using UnityEngine;

public class QuatUtils
{
    public static void Serialize(NetDataWriter writer, Quaternion quat)
    {
        writer.Put((ushort)quat.x);
        writer.Put((ushort)quat.y);
        writer.Put((ushort)quat.z);
        writer.Put((ushort)quat.w);
    }

    public static Quaternion Deserialize(NetDataReader reader)
    {
        return new Quaternion(reader.GetUShort()/ushort.MaxValue, reader.GetUShort() / ushort.MaxValue, reader.GetUShort()/ushort.MaxValue, reader.GetUShort()/ushort.MaxValue);
    }
}