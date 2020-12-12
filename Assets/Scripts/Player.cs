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
    public Entity leftGrabbed;
    [ReadOnly]
    public Entity rightGrabbed;
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

    public void AddStateToBuffer(PlayerInput pi)
    {
        if (pi.Sequence <= lastSequence)
        {
            return;
        }
        lastSequence = pi.Sequence;
        if (inputBuffer.IsFull)
        {
            if (DEVNetworkSwitcher.showLagLogs)
            {
                Debug.Log("TOO MUCH STATE RECEIVED");
            }
            //Lag?
            inputBuffer.FastClear();
        }
        inputBuffer.Add(pi);
        inputBufferLength = inputBuffer.Count;
    }
}
