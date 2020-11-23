using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class NetworkObjectEvent : UnityEvent<NetworkObject> { }

public class NetworkObjectManager : MonoBehaviour
{
    public List<GameObject> prefabs = new List<GameObject>();
    public NetworkObjectEvent onObjectAdd = new NetworkObjectEvent();
    public NetworkObjectEvent onObjectRemove = new NetworkObjectEvent();
    private static NetworkObjectManager _instance;
    private readonly List<NetworkObject> objects = new List<NetworkObject>();
    private GameManagerServer server;
    private GameManagerClient client;

    static public NetworkObjectManager GetInstance()
    {
        return _instance;
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        if (DEVNetworkSwitcher.isServer)
        {
            server = FindObjectOfType<GameManagerServer>();
        }
        else
        {
            client = FindObjectOfType<GameManagerClient>();
        }
    }

    public void Add(NetworkObject obj)
    {
        objects.Add(obj);
        InteractableFacade interactable = obj.GetComponent<InteractableFacade>();
        if (interactable != null)
        {
            interactable.Grabbed.AddListener((InteractorFacade interactor) => SetGrab(obj, interactor.name == "LeftInteractor"));
            interactable.Ungrabbed.AddListener((InteractorFacade interactor) => SetGrab(null, interactor.name == "LeftInteractor"));
        }
        onObjectAdd.Invoke(obj);
    }
    public void Remove(NetworkObject obj)
    {
        objects.Remove(obj);
        onObjectRemove.Invoke(obj);
    }

    public void ClientSideRemoveOldObjects(EntityState[] esArr)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            NetworkObject oldObject = objects[i];
            if (Array.Find(esArr, x => x.Id == oldObject.id) == null)
            {
                Destroy(oldObject.gameObject);
            }
        }
    }

    public void ClientSideSpawn(EntityState es)
    {
        NetworkObject newObject = Instantiate(prefabs[es.Type]).GetComponent<NetworkObject>();
        newObject.transform.position = es.Position;
        newObject.transform.eulerAngles = es.Rotation;
        newObject.id = es.Id;
        objects.Add(newObject);
    }

    public void ClientSideLinkOrSpawnLocalObject(EntityState es)
    {
        NetworkObject localGun = objects.Find((NetworkObject g) => (byte)g.type == es.Type && g.id == 0);
        if (localGun != null)
        {
            localGun.transform.position = es.Position;
            localGun.transform.eulerAngles = es.Rotation;
            localGun.id = es.Id;
        }
        else
        {
            ClientSideSpawn(es);
        }
    }

    public void ClientSideSync(EntityState[] esArr)
    {
        for (int i = 0; i < esArr.Length; i++)
        {
            NetworkObject obj = objects.Find(x => x.id == esArr[i].Id);
            if (obj != null)
            {
                if (!obj.grabbed)
                {
                    obj.SetPositionTarget(esArr[i].Position);
                    obj.SetRotationTarget(esArr[i].Rotation);
                }
            }
            else
            {
                ClientSideLinkOrSpawnLocalObject(esArr[i]);
            }
        }
        ClientSideRemoveOldObjects(esArr);
    }

    public void ServerSideSyncClientGrabbing(int peerID, PlayerInput pi)
    {
        if (pi.LeftGrabId != 0)
        {
            NetworkObject grabbed = objects.Find((NetworkObject g) => g.id == pi.LeftGrabId);
            if (grabbed.snapContainer != null)
            {
                grabbed.snapContainer.Unsnap();
                grabbed.snapContainer = null;
            }
            grabbed.grabbed = true;
            grabbed.leftHand = true;
            grabbed.lastOwnerId = peerID;
            grabbed.rigidBody.isKinematic = true;
            grabbed.bufferVelocity = pi.LeftGrabVelocity;
            grabbed.bufferAngularVelocity = pi.LeftGrabAngularVelocity;
            grabbed.SetPositionTarget(pi.LeftGrabPosition);
            grabbed.SetRotationTarget(pi.LeftGrabRotation);
        }
        if (pi.RightGrabId != 0)
        {
            NetworkObject grabbed = objects.Find((NetworkObject g) => g.id == pi.RightGrabId);
            if (grabbed.snapContainer != null)
            {
                grabbed.snapContainer.Unsnap();
                grabbed.snapContainer = null;
            }
            grabbed.grabbed = true;
            grabbed.leftHand = false;
            grabbed.lastOwnerId = peerID;
            grabbed.rigidBody.isKinematic = true;
            grabbed.bufferVelocity = pi.RightGrabVelocity;
            grabbed.bufferAngularVelocity = pi.RightGrabAngularVelocity;
            grabbed.SetPositionTarget(pi.RightGrabPosition);
            grabbed.SetRotationTarget(pi.RightGrabRotation);
        }
    }
    public void ServerSideSyncClientUngrabbed(int leftUngrabbedId, int rightUngrabbedId)
    {
        if (leftUngrabbedId != 0)
        {
            NetworkObject ungrabbed = objects.Find((NetworkObject g) => g.id == leftUngrabbedId);
            ungrabbed.grabbed = false;
            ungrabbed.leftHand = false;
            ungrabbed.rigidBody.isKinematic = ungrabbed.kinematicInitValue;
            ungrabbed.rigidBody.velocity = ungrabbed.bufferVelocity;
            ungrabbed.bufferVelocity = Vector3.zero;
            ungrabbed.rigidBody.angularVelocity = ungrabbed.bufferAngularVelocity;
            ungrabbed.bufferAngularVelocity = Vector3.zero;
        }
        if (rightUngrabbedId != 0)
        {
            NetworkObject ungrabbed = objects.Find((NetworkObject g) => g.id == rightUngrabbedId);
            ungrabbed.grabbed = false;
            ungrabbed.leftHand = false;
            ungrabbed.rigidBody.isKinematic = ungrabbed.kinematicInitValue;
            ungrabbed.rigidBody.velocity = ungrabbed.bufferVelocity;
            ungrabbed.bufferVelocity = Vector3.zero;
            ungrabbed.rigidBody.angularVelocity = ungrabbed.bufferAngularVelocity;
            ungrabbed.bufferAngularVelocity = Vector3.zero;
        }
    }

    public void SetGrab(NetworkObject obj, bool leftHand)
    {
        int ownId = DEVNetworkSwitcher.isServer ? server.serverId : client.avatarId;
        NetworkObject leftGrab = DEVNetworkSwitcher.isServer ? server.leftGrab : client.leftGrab;
        NetworkObject rightGrab = DEVNetworkSwitcher.isServer ? server.rightGrab : client.rightGrab;
        if (leftHand)
        {
            if (obj != null)
            {
                obj.grabbed = true;
                obj.leftHand = true;
                obj.lastOwnerId = ownId;
            }
            else
            {
                leftGrab.grabbed = false;
            }
            if (DEVNetworkSwitcher.isServer)
            {
                server.leftGrab = obj;
            }
            else
            {
                client.leftGrab = obj;
            }
        }
        else
        {
            if (obj != null)
            {
                obj.grabbed = true;
                obj.leftHand = false;
                obj.lastOwnerId = ownId;
            }
            else
            {
                rightGrab.grabbed = false;
            }
            if (DEVNetworkSwitcher.isServer)
            {
                server.rightGrab = obj;
            }
            else
            {
                client.rightGrab = obj;
            }
        }
    }

    public EntityState[] GetEntityStates()
    {
        EntityState[] entityStates = new EntityState[objects.Count];
        for (int i = 0; i < objects.Count; i++)
        {
            NetworkObject b = objects[i];
            entityStates[i] = new EntityState()
            {
                Id = b.id,
                Type = (byte)b.type,
                Position = b.transform.position,
                Rotation = b.transform.rotation.eulerAngles,
            };
        }
        return (entityStates);
    }
}
