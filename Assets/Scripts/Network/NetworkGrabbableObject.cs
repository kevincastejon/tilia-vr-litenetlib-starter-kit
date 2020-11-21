﻿using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InteractableFacade))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkGrabbableObject : MonoBehaviour
{
    [ReadOnly]
    public int id;
    [ReadOnly]
    public bool grabbed;
    [ReadOnly]
    public bool leftHand;
    [ReadOnly]
    public int lastOwnerId;
    [HideInInspector]
    public Rigidbody rigidBody;
    private LTDescr moveTween;
    private LTDescr rotTween;
    private float lastPosUpdate;
    private float lastRotUpdate;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        if (DEVNetworkSwitcher.isServer)
        {
            id = GetInstanceID();
        }
        else
        {
            rigidBody.isKinematic = true;
        }
    }

    private void Update()
    {
        lastPosUpdate += Time.deltaTime;
        lastRotUpdate += Time.deltaTime;
    }

    public void SetPositionTarget(Vector3 posTarget)
    {
        if (moveTween != null)
        {
            LeanTween.cancel(moveTween.id);
        }
        moveTween = LeanTween.move(gameObject, posTarget, lastPosUpdate);
        moveTween.setOnComplete(() => moveTween = null);
        lastPosUpdate = 0;
    }

    public void SetRotationTarget(Quaternion rotTarget)
    {
        if (rotTween != null)
        {
            LeanTween.cancel(rotTween.id);
        }
        rotTween = LeanTween.rotate(gameObject, rotTarget.eulerAngles, lastRotUpdate);
        rotTween.setOnComplete(() => rotTween = null);
        lastRotUpdate = 0;
    }
}
