using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using Tilia.SDK.OculusIntegration.Input;
using UnityEngine;
using UnityEngine.Events;

public class ShootEvent : UnityEvent<Gun> { }

public class LocalAvatar : MonoBehaviour
{
    [Header("Reference Settings")]
    public GameObject headAlias;
    public GameObject leftHandAlias;
    public GameObject rightHandAlias;
    public InteractorFacade leftInteractor;
    public InteractorFacade rightInteractor;
    public OVRInputButtonAction leftTriggerAction;
    public OVRInputButtonAction rightTriggerAction;
    public OVRInputTouchAction leftPointerAction;
    public OVRInputTouchAction rightPointerAction;
    [Header("Monitoring")]
    [ReadOnly]
    public InteractableFacade leftGrabbed;
    [ReadOnly]
    public InteractableFacade rightGrabbed;
    [ReadOnly]
    public bool leftPointer;
    [ReadOnly]
    public bool rightPointer;
    [ReadOnly]
    public bool leftTrigger;
    [ReadOnly]
    public bool rightTrigger;
    [HideInInspector]
    public ShootEvent OnShoot = new ShootEvent();

    // Start is called before the first frame update
    private void Start()
    {
        leftInteractor.Grabbed.AddListener(OnLeftGrab);
        rightInteractor.Grabbed.AddListener(OnRightGrab);
        leftInteractor.Ungrabbed.AddListener(OnLeftUngrab);  
        rightInteractor.Ungrabbed.AddListener(OnRightUngrab);
        leftTriggerAction.ValueChanged.AddListener(OnLeftTrigger);
        rightTriggerAction.ValueChanged.AddListener(OnRightTrigger);
        leftPointerAction.ValueChanged.AddListener(OnLeftPointer);
        rightPointerAction.ValueChanged.AddListener(OnRightPointer);
    }

    private void OnLeftPointer(bool value)
    {
        leftPointer = value;
    }
    
    private void OnRightPointer(bool value)
    {
        rightPointer = value;
    }

    private void OnLeftGrab(InteractableFacade interactable)
    {
        leftGrabbed = interactable;
    }

    private void OnRightGrab(InteractableFacade interactable)
    {
        rightGrabbed = interactable;
    }
    private void OnLeftUngrab(InteractableFacade interactable)
    {
        leftGrabbed = null;
    }

    private void OnRightUngrab(InteractableFacade interactable)
    {
        rightGrabbed = null;
    }

    private void OnLeftTrigger(bool value)
    {
        leftTrigger = value;
        if (value && leftGrabbed)
        {
            Gun gun = leftGrabbed.GetComponent<Gun>();
            if (gun != null)
            {
                OnShoot.Invoke(gun);
            }
        }
    }
    private void OnRightTrigger(bool value)
    {
        rightTrigger = value;
        if (value && rightGrabbed)
        {
            Gun gun = rightGrabbed.GetComponent<Gun>();
            if (gun != null)
            {
                OnShoot.Invoke(gun);
            }
        }
    }
}
