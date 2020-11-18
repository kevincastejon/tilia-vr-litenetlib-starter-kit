﻿using LiteNetLib.Utils;
using UnityEngine;

public class QuatUtils
{
    public static void Serialize(NetDataWriter writer, Quaternion quat)
    {
        writer.Put(quat.x);
        writer.Put(quat.y);
        writer.Put(quat.z);
        writer.Put(quat.w);
    }

    public static Quaternion Deserialize(NetDataReader reader)
    {
        return new Quaternion(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
    }
}