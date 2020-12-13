using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Indicators.ObjectPointers;
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
    public PointerFacade leftPointerFacade;
    public PointerFacade rightPointerFacade;
    public OVRInputButtonAction leftTriggerAction;
    public OVRInputButtonAction rightTriggerAction;
    public OVRInputTouchAction leftPointerAction;
    public OVRInputTouchAction rightPointerAction;
    [Header("Monitoring")]
    [ReadOnly]
    public int id;
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
    [HideInInspector]
    public Vector3 leftGrabVelocity;
    [HideInInspector]
    public Vector3 leftGrabAngularVelocity;
    [HideInInspector]
    public Vector3 rightGrabVelocity;
    [HideInInspector]
    public Vector3 rightGrabAngularVelocity;

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

    private void FixedUpdate()
    {
        if (leftGrabbed)
        {
            leftGrabVelocity = leftInteractor.VelocityTracker.GetVelocity();
            leftGrabAngularVelocity = leftInteractor.VelocityTracker.GetAngularVelocity();
        }
        if (rightGrabbed)
        {
            rightGrabVelocity = rightInteractor.VelocityTracker.GetVelocity();
            rightGrabAngularVelocity = rightInteractor.VelocityTracker.GetAngularVelocity();
        }
    }

    public Entity GetLeftGrabbedEntity()
    {
        if (leftGrabbed)
        {
            Entity ent = leftGrabbed.GetComponent<Entity>();
            if (ent)
            {
                return ent;
            }
        }
        return null;
    }
    public Entity GetRightGrabbedEntity()
    {
        if (rightGrabbed)
        {
            Entity ent = rightGrabbed.GetComponent<Entity>();
            if (ent)
            {
                return ent;
            }
        }
        return null;
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
        leftPointerFacade.gameObject.SetActive(false);
        leftGrabbed = interactable;
        Entity ent = interactable.GetComponent<Entity>();
        if (ent)
        {
            ent.ownerId = id;
        }
    }

    private void OnRightGrab(InteractableFacade interactable)
    {
        rightPointerFacade.gameObject.SetActive(false);
        rightGrabbed = interactable;
        Entity ent = interactable.GetComponent<Entity>();
        if (ent)
        {
            //Debug.Log("GRABBED "+ ent.id);
            ent.ownerId = id;
        }
    }
    private void OnLeftUngrab(InteractableFacade interactable)
    {
        leftPointerFacade.gameObject.SetActive(true);
        leftGrabbed = null;
        Entity ent = interactable.GetComponent<Entity>();
        if (ent)
        {
            if (!DEVNetworkSwitcher.isServer && ent.body)
            {
                ent.body.isKinematic = true;
            }
            else if(DEVNetworkSwitcher.isServer && ent.body)
            {
                ent.body.isKinematic = ent.initialIsKinematic;
            }
            ent.ownerId = -1;
        }
    }

    private void OnRightUngrab(InteractableFacade interactable)
    {
        rightPointerFacade.gameObject.SetActive(true);
        rightGrabbed = null;
        Entity ent = interactable.GetComponent<Entity>();
        if (ent)
        {
            if (!DEVNetworkSwitcher.isServer && ent.body)
            {
                ent.body.isKinematic = true;
                Debug.Log("SET ISKINEMATIC BACK TO TRUE");
            }
            else if (DEVNetworkSwitcher.isServer && ent.body)
            {
                ent.body.isKinematic = ent.initialIsKinematic;
            }
            //Debug.Log("UNGRABBED " + ent.id);
            ent.ownerId = -1;
        }
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
