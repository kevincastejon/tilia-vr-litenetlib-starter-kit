using LiteNetLib.Utils;
using System;
using UnityEngine;

public class QuatUtils
{
    private const float FLOAT_PRECISION_MULT = 10000f;
    public static void Serialize(NetDataWriter writer, Quaternion quat)
    {
        byte largest;
        short integer_a;
        short integer_b;
        short integer_c;
        CompressRotation(quat, out largest, out integer_a, out integer_b, out integer_c);
        writer.Put(largest);
        writer.Put(integer_a);
        writer.Put(integer_b);
        writer.Put(integer_c);
    }

    public static Quaternion Deserialize(NetDataReader reader)
    {
        byte largest = reader.GetByte();
        short integer_a = reader.GetShort();
        short integer_b = reader.GetShort();
        short integer_c = reader.GetShort();
        return (UncompressRotation(largest, integer_a, integer_b, integer_c));
    }
    public static void CompressRotation(Quaternion rotation, out byte largest, out short _a, out short _b, out short _c)
    {
        var maxIndex = (byte)0;
        var maxValue = float.MinValue;
        var sign = 1f;

        // Determine the index of the largest (absolute value) element in the Quaternion.
        // We will transmit only the three smallest elements, and reconstruct the largest
        // element during decoding. 
        for (int i = 0; i < 4; i++)
        {
            var element = rotation[i];
            var abs = Mathf.Abs(rotation[i]);
            if (abs > maxValue)
            {
                // We don't need to explicitly transmit the sign bit of the omitted element because you 
                // can make the omitted element always positive by negating the entire quaternion if 
                // the omitted element is negative (in quaternion space (x,y,z,w) and (-x,-y,-z,-w) 
                // represent the same rotation.), but we need to keep track of the sign for use below.
                sign = (element < 0) ? -1 : 1;

                // Keep track of the index of the largest element
                maxIndex = (byte)i;
                maxValue = abs;
            }
        }

        // If the maximum value is approximately 1f (such as Quaternion.identity [0,0,0,1]), then we can 
        // reduce storage even further due to the fact that all other fields must be 0f by definition, so 
        // we only need to send the index of the largest field.
        if (Mathf.Approximately(maxValue, 1f))
        {
            // Again, don't need to transmit the sign since in quaternion space (x,y,z,w) and (-x,-y,-z,-w) 
            // represent the same rotation. We only need to send the index of the single element whose value
            // is 1f in order to recreate an equivalent rotation on the receiver.
            largest = (byte)(maxIndex + 4);
            _a = 0;
            _b = 0;
            _c = 0;
            return;
        }

        var a = (short)0;
        var b = (short)0;
        var c = (short)0;

        // We multiply the value of each element by QUAT_PRECISION_MULT before converting to 16-bit integer 
        // in order to maintain precision. This is necessary since by definition each of the three smallest 
        // elements are less than 1.0, and the conversion to 16-bit integer would otherwise truncate everything 
        // to the right of the decimal place. This allows us to keep five decimal places.

        if (maxIndex == 0)
        {
            a = (short)(rotation.y * sign * FLOAT_PRECISION_MULT);
            b = (short)(rotation.z * sign * FLOAT_PRECISION_MULT);
            c = (short)(rotation.w * sign * FLOAT_PRECISION_MULT);
        }
        else if (maxIndex == 1)
        {
            a = (short)(rotation.x * sign * FLOAT_PRECISION_MULT);
            b = (short)(rotation.z * sign * FLOAT_PRECISION_MULT);
            c = (short)(rotation.w * sign * FLOAT_PRECISION_MULT);
        }
        else if (maxIndex == 2)
        {
            a = (short)(rotation.x * sign * FLOAT_PRECISION_MULT);
            b = (short)(rotation.y * sign * FLOAT_PRECISION_MULT);
            c = (short)(rotation.w * sign * FLOAT_PRECISION_MULT);
        }
        else
        {
            a = (short)(rotation.x * sign * FLOAT_PRECISION_MULT);
            b = (short)(rotation.y * sign * FLOAT_PRECISION_MULT);
            c = (short)(rotation.z * sign * FLOAT_PRECISION_MULT);
        }

        largest = maxIndex;
        _a = a;
        _b = b;
        _c = c;
    }

    /// <summary>
    /// Reads a compressed rotation value from the network stream. This value must have been previously written
    /// with WriteCompressedRotation() in order to be properly decompressed.
    /// </summary>
    /// <param name="reader">The network stream to read the compressed rotation value from.</param>
    /// <returns>Returns the uncompressed rotation value as a Quaternion.</returns>
    public static Quaternion UncompressRotation(byte largest, short _a, short _b, short _c)
    {
        // Read the index of the omitted field from the stream.
        var maxIndex = largest;

        // Values between 4 and 7 indicate that only the index of the single field whose value is 1f was
        // sent, and (maxIndex - 4) is the correct index for that field.
        if (maxIndex >= 4 && maxIndex <= 7)
        {
            var x = (maxIndex == 4) ? 1f : 0f;
            var y = (maxIndex == 5) ? 1f : 0f;
            var z = (maxIndex == 6) ? 1f : 0f;
            var w = (maxIndex == 7) ? 1f : 0f;

            return new Quaternion(x, y, z, w);
        }

        // Read the other three fields and derive the value of the omitted field
        var a = (float)_a / FLOAT_PRECISION_MULT;
        var b = (float)_b / FLOAT_PRECISION_MULT;
        var c = (float)_c / FLOAT_PRECISION_MULT;
        var d = Mathf.Sqrt(1f - (a * a + b * b + c * c));

        if (maxIndex == 0)
            return new Quaternion(d, a, b, c);
        else if (maxIndex == 1)
            return new Quaternion(a, d, b, c);
        else if (maxIndex == 2)
            return new Quaternion(a, b, d, c);

        return new Quaternion(a, b, c, d);
    }
}