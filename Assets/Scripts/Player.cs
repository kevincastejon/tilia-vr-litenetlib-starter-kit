using System.Collections;
using System.Collections.Generic;
using Tilia.Indicators.ObjectPointers;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using Tilia.SDK.OculusIntegration.Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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
    private GameObject nameOrientationTarget;
    private float _receivedTime;
    private float _timer;
    private const float BufferTime = 0.1f; //100 milliseconds

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

    public void SetNameOrientationTarget(GameObject target)
    {
        nameOrientationTarget = target;
    }


    public void UpdatePosition(float delta)
    {
        if (nameOrientationTarget)
        {
            playerName.transform.rotation = Quaternion.LookRotation(playerName.transform.position - nameOrientationTarget.transform.position);
        }

        if (_receivedTime < BufferTime || inputBuffer.Count < 2)
            return;
        var dataA = inputBuffer[0];
        var dataB = inputBuffer[1];

        float lerpTime = NetworkGeneral.SeqDiff(dataB.Sequence, dataA.Sequence) * LogicTimer.FixedDelta;
        float t = _timer / lerpTime;
        headAlias.transform.position = Vector3.Lerp(dataA.HeadPosition, dataB.HeadPosition, t);
        headAlias.transform.rotation = Quaternion.Lerp(dataA.HeadRotation, dataB.HeadRotation, t);
        leftHandAlias.transform.position = Vector3.Lerp(dataA.LeftHandPosition, dataB.LeftHandPosition, t);
        leftHandAlias.transform.rotation = Quaternion.Lerp(dataA.LeftHandRotation, dataB.LeftHandRotation, t);
        rightHandAlias.transform.position = Vector3.Lerp(dataA.RightHandPosition, dataB.RightHandPosition, t);
        rightHandAlias.transform.rotation = Quaternion.Lerp(dataA.RightHandRotation, dataB.RightHandRotation, t);
        _timer += delta;
        if (_timer > lerpTime)
        {
            _receivedTime -= lerpTime;
            inputBuffer.RemoveFromStart(1);
            inputBufferLength = inputBuffer.Count;
            _timer -= lerpTime;
        }
    }

    public void AddStateToBuffer(PlayerInput pi)
    {
        int diff = NetworkGeneral.SeqDiff(pi.Sequence, inputBuffer.Last.Sequence);
        if (diff <= 0)
            return;

        _receivedTime += diff * LogicTimer.FixedDelta;
        if (inputBuffer.IsFull)
        {
            Debug.LogWarning("[C] Remote: Something happened");
            //Lag?
            _receivedTime = 0f;
            inputBuffer.FastClear();
        }
        inputBuffer.Add(pi);
        inputBufferLength = inputBuffer.Count;
    }
}
