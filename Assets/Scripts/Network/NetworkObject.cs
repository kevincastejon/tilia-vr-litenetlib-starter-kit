using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkObject : MonoBehaviour
{
    [ReadOnly]
    public int id;
    private LTDescr moveTween;
    private LTDescr rotTween;
    private float lastPosUpdate;
    private float lastRotUpdate;

    private void Start()
    {
        if (DEVNetworkSwitcher.isServer)
        {
            id = GetInstanceID();
        }
        else
        {
            GetComponent<Rigidbody>().isKinematic = true;
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

    public void SetRotationTarget(Vector3 rotTarget)
    {
        if (rotTween != null)
        {
            LeanTween.cancel(rotTween.id);
        }
        rotTween = LeanTween.rotate(gameObject, rotTarget, lastRotUpdate);
        rotTween.setOnComplete(() => rotTween = null);
        lastRotUpdate = 0;
    }
}
