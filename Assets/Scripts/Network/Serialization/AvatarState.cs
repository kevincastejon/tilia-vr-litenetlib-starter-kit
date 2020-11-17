using LiteNetLib.Utils;
using UnityEngine;

public class AvatarState
{
    public Vector3 HeadPosition { get; set; }
    public Vector3 HeadRotation { get; set; }
    public Vector3 LeftHandPosition { get; set; }
    public Vector3 LeftHandRotation { get; set; }
    public Vector3 RightHandPosition { get; set; }
    public Vector3 RightHandRotation { get; set; }
    public bool Shooting { get; set; }
}
