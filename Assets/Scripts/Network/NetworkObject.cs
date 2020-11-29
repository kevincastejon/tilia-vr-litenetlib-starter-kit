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
    public int stateBufferLength;
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
    private readonly List<EntityState> stateBuffer = new List<EntityState>();
    private EntityState stateA;
    private EntityState stateB;
    private float lerpMax = 1 / 60f;
    private float lerpTimer = 0f;

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
        bool isLerping = stateA != null && stateB != null;
        if (stateBuffer.Count >= 3 && !isLerping)
        {
            if (stateA == null)
            {
                stateA = stateBuffer[0];
                stateBuffer.RemoveAt(0);
            }
            stateB = stateBuffer[0];
            stateBuffer.RemoveAt(0);
            //Debug.Log("removed state");
            stateBufferLength = stateBuffer.Count;
            isLerping = true;
        }
        if (isLerping)
        {
            EntityState esA = stateA;
            EntityState esB = stateB;
            transform.position = Vector3.Lerp(esA.Position, esB.Position, lerpTimer / lerpMax);
            transform.rotation = Quaternion.Lerp(esA.Rotation, esB.Rotation, lerpTimer / lerpMax);
        }
        lerpTimer += Time.deltaTime;
        if (true)
        //if (lerpTimer >= lerpMax)
        {
            lerpTimer = 0f;
            stateA = stateB;
            stateB = null;
        }
    }

    public void AddStateToBuffer(EntityState es)
    {
        //if (DEVNetworkSwitcher.isServer)
        //{
            //Debug.Log("added state");
        //}
        stateBuffer.Add(es);
        stateBufferLength = stateBuffer.Count;
    }

    public void ClearBuffer()
    {
        stateBuffer.Clear();
        stateA = null;
        stateB = null;
        lerpTimer = 0f;
    }

    private void OnDestroy()
    {
        NetworkManager.GetInstance().Remove(this);
    }
}
