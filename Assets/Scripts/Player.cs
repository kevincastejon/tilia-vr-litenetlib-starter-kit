using System.Collections;
using System.Collections.Generic;
using Tilia.Indicators.ObjectPointers;
using Tilia.Interactions.Interactables.Interactables;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Reference Settings")]
    public GameObject headAlias;
    public GameObject leftHandAlias;
    public GameObject rightHandAlias;
    public PointerFacade leftPointerFacade;
    public PointerFacade rightPointerFacade;
    public TextMeshProUGUI playerName;
    [Header("Monitoring")]
    [ReadOnly]
    public int id;
    [ReadOnly]
    public bool leftShooting;
    [ReadOnly]
    public bool rightShooting;
    [ReadOnly]
    private bool leftPointer;
    [ReadOnly]
    private bool rightPointer;
    [Header("!Server-Side Only!")]
    [ReadOnly]
    public InteractableFacade leftGrabbed;
    [ReadOnly]
    public InteractableFacade rightGrabbed;
    [ReadOnly]
    public int inputBufferLength;
    [HideInInspector]
    public LiteRingBuffer<PlayerInput> inputBuffer = new LiteRingBuffer<PlayerInput>(5);
    [HideInInspector]
    public GameObject nameOrientationTarget;
    private int lastSequence = -1;

    public bool LeftPointer
    {
        get { return leftPointer; }
        set { leftPointer = value; if (value) { leftPointerFacade.Activate(); } else { leftPointerFacade.Deactivate(); } }
    }

    public bool RightPointer
    {
        get { return rightPointer; }
        set { rightPointer = value; if (value) { rightPointerFacade.Activate(); } else { rightPointerFacade.Deactivate(); } }
    }

    //public void UpdatePosition(float t,bool isLastFrame)
    //{
    //    if (nameOrientationTarget)
    //    {
    //        playerName.transform.rotation = Quaternion.LookRotation(playerName.transform.position - nameOrientationTarget.transform.position);
    //    }

    //    if (inputBuffer.Count < 2)
    //    {
    //        Debug.Log("NOT ENOUGTH DATA RECEIVED FROM PLAYER "+id);
    //        return;
    //    }
    //    var dataA = inputBuffer[0];
    //    var dataB = inputBuffer[1];

    //    headAlias.transform.position = Vector3.Lerp(dataA.HeadPosition, dataB.HeadPosition, t);
    //    headAlias.transform.rotation = Quaternion.Lerp(dataA.HeadRotation, dataB.HeadRotation, t);
    //    leftHandAlias.transform.position = Vector3.Lerp(dataA.LeftHandPosition, dataB.LeftHandPosition, t);
    //    leftHandAlias.transform.rotation = Quaternion.Lerp(dataA.LeftHandRotation, dataB.LeftHandRotation, t);
    //    rightHandAlias.transform.position = Vector3.Lerp(dataA.RightHandPosition, dataB.RightHandPosition, t);
    //    rightHandAlias.transform.rotation = Quaternion.Lerp(dataA.RightHandRotation, dataB.RightHandRotation, t);
    //    if (isLastFrame)
    //    {
    //        inputBuffer.RemoveFromStart(1);
    //        inputBufferLength = inputBuffer.Count;
    //    }
    //}

    public void AddStateToBuffer(PlayerInput pi)
    {
        if (pi.Sequence<=lastSequence)
        {
            return;
        }
        lastSequence = pi.Sequence;
        if (inputBuffer.IsFull)
        {
            Debug.Log("TOO MUCH STATE RECEIVED");
            //Lag?
            inputBuffer.FastClear();
        }
        inputBuffer.Add(pi);
        inputBufferLength = inputBuffer.Count;
    }
}
