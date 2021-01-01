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
    public string oculusId;
    [ReadOnly]
    public bool connected;
    [ReadOnly]
    public bool leftTrigger;
    [ReadOnly]
    public bool rightTrigger;
    [ReadOnly]
    public bool leftPointer;
    [ReadOnly]
    public bool rightPointer;
    [Header("!Server-Side Only!")]
    [ReadOnly]
    public Entity leftGrabbed;
    [ReadOnly]
    public Entity rightGrabbed;
    [ReadOnly]
    public int inputBufferLength;
    [ReadOnly]
    public int lagPikes;
    [HideInInspector]
    public LiteRingBuffer<PlayerInput> inputBuffer = new LiteRingBuffer<PlayerInput>(5);
    [HideInInspector]
    public GameObject nameOrientationTarget;
    [HideInInspector]
    public ShootEvent OnShoot = new ShootEvent();
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

    public bool LeftTrigger
    {
        get { return leftTrigger; }
        set {
            if (!leftTrigger && value)
            {
                if (leftGrabbed && leftGrabbed.type == EntityType.Gun)
                {
                    OnShoot.Invoke(leftGrabbed.GetComponent<Gun>());
                }
            }
            leftTrigger = value; }
    }

    public bool RightTrigger
    {
        get { return rightTrigger; }
        set
        {
            if (!rightTrigger && value)
            {
                if (rightGrabbed && rightGrabbed.type == EntityType.Gun)
                {
                    OnShoot.Invoke(rightGrabbed.GetComponent<Gun>());
                }
                rightPointerFacade.Select();
            }
            rightTrigger = value;  }
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
            lagPikes++;
            if (NetworkManager.showLagLogs)
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
