using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using Tilia.Interactions.SnapZone;
using UnityEngine;
using UnityEngine.Events;

//[RequireComponent(typeof(InteractableFacade))]
//[RequireComponent(typeof(Rigidbody))]
public class NetworkObject : MonoBehaviour
{
    public EntityType type;
    public Rigidbody body;
    [ReadOnly]
    public int id;
    [ReadOnly]
    public bool grabbed;
    [ReadOnly]
    public bool leftHand;
    [ReadOnly]
    public int lastOwnerId;
    [ReadOnly]
    public SnapZoneFacade snapContainer;
    [HideInInspector]
    public Vector3 bufferVelocity;
    [HideInInspector]
    public Vector3 bufferAngularVelocity;
    [HideInInspector]
    public bool kinematicInitValue;
    private LTDescr moveTween;
    private LTDescr rotTween;
    private float lastPosUpdate;
    private float lastRotUpdate;

    private void Start()
    {
        if (DEVNetworkSwitcher.isServer)
        {
            id = GetInstanceID();
            if (body)
            {
                kinematicInitValue = body.isKinematic;
            }
        }
        else
        {
            if (body)
            {
                body.isKinematic = true;
            }
        }
        NetworkManager.GetInstance().Add(this);
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

    private void OnDestroy()
    {
        NetworkManager.GetInstance().Remove(this);
    }
}
