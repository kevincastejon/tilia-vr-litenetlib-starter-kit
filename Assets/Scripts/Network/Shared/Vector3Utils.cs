using LiteNetLib.Utils;
using UnityEngine;

public class Vector3Utils
{
    public static void Serialize(NetDataWriter writer, Vector3 vector)
    {
        writer.Put(vector.x);
        writer.Put(vector.y);
        writer.Put(vector.z);
    }

    public static Vector3 Deserialize(NetDataReader reader)
    {
        return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }
    public static void SerializeHand(NetDataWriter writer, Vector3 hand, Vector3 head)
    {
        Vector3 dif = hand - head;
        writer.Put((short)(dif.x / 2 * short.MaxValue));
        writer.Put((short)(dif.y / 2 * short.MaxValue));
        writer.Put((short)(dif.z / 2 * short.MaxValue));
    }

    public static Vector3 DeserializeHand(NetDataReader reader, Vector3 head)
    {
        Vector3 dif = new Vector3(reader.GetShort() / short.MaxValue * 2f, reader.GetShort() / short.MaxValue * 2f, reader.GetShort() / short.MaxValue * 2f);
        return head + dif;
    }
}