using System;
using System.Collections;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EntityTypeSettings
{
    public GameObject prefab;
    public int max;
    public bool removeFirstEntity;
}
[Serializable]
public class NetworkObjectEvent : UnityEvent<NetworkObject> { }

public class NetworkManager : MonoBehaviour
{
    public List<EntityTypeSettings> entityTypesSettings = new List<EntityTypeSettings>();
    public NetworkObjectEvent onObjectAdd = new NetworkObjectEvent();
    public NetworkObjectEvent onObjectRemove = new NetworkObjectEvent();
    private static NetworkManager _instance;
    private readonly List<NetworkObject> objects = new List<NetworkObject>();
    private GameManagerServer server;
    private GameManagerClient client;

    static public NetworkManager GetInstance()
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
        EntityTypeSettings settings = entityTypesSettings[(int)obj.type];
        if (settings.max > -1)
        {
            List<NetworkObject> objs = objects.FindAll((NetworkObject x) =>
            {
                return x.type == obj.type;
            });
            if (objs.Count == settings.max)
            {

                if (settings.removeFirstEntity)
                {
                    NetworkObject oldObj = objs[0];
                    objects.Remove(oldObj);
                    Destroy(oldObj.gameObject);
                }
                else
                {
                    Destroy(obj.gameObject);
                }
            }
        }
        if (settings.max == -1 || settings.removeFirstEntity)
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
    }
    public void Remove(NetworkObject obj)
    {
        if (objects.Contains(obj))
        {
            objects.Remove(obj);
            onObjectRemove.Invoke(obj);
        }
    }

    public NetworkObject GetObject(int id)
    {
        return (objects.Find((NetworkObject no) => no.id == id));
    }

    public void ClientSideRemoveOldObjects(EntityState[] esArr)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            NetworkObject oldObject = objects[i];
            bool isOld = Array.Find(esArr, x => x.Id == oldObject.id) == null;
            if (isOld)
            {
                Destroy(oldObject.gameObject);
            }
        }
    }

    public void ClientSideSpawn(EntityState es)
    {
        NetworkObject newObject = Instantiate(entityTypesSettings[es.Type].prefab).GetComponent<NetworkObject>();
        newObject.transform.position = es.Position;
        newObject.transform.rotation = es.Rotation;
        newObject.id = es.Id;
        print("spawned object " + newObject.gameObject);
    }

    public void ClientSideLinkOrSpawnLocalObject(EntityState es)
    {
        NetworkObject localObject = objects.Find((NetworkObject g) => (byte)g.type == es.Type && g.id == 0);
        if (localObject != null)
        {
            localObject.transform.position = es.Position;
            localObject.transform.rotation = es.Rotation;
            localObject.id = es.Id;
            print("linked object " + localObject.gameObject);
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
                    obj.AddStateToBuffer(esArr[i].Clone());
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
            if (grabbed.body != null)
            {
                grabbed.body.isKinematic = true;
                grabbed.bufferVelocity = pi.LeftGrabVelocity;
                grabbed.bufferAngularVelocity = pi.LeftGrabAngularVelocity;
            }
            grabbed.AddStateToBuffer(new EntityState() { Position = pi.LeftGrabPosition, Rotation = pi.LeftGrabRotation });
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
            if (grabbed.body != null)
            {
                grabbed.body.isKinematic = true;
                grabbed.bufferVelocity = pi.RightGrabVelocity;
                grabbed.bufferAngularVelocity = pi.RightGrabAngularVelocity;
            }
            grabbed.AddStateToBuffer(new EntityState() { Position = pi.RightGrabPosition, Rotation = pi.RightGrabRotation });
        }
    }
    public void ServerSideSyncClientUngrabbed(int leftUngrabbedId, int rightUngrabbedId)
    {
        if (leftUngrabbedId != 0)
        {
            NetworkObject ungrabbed = objects.Find((NetworkObject g) => g.id == leftUngrabbedId);
            ungrabbed.grabbed = false;
            ungrabbed.leftHand = false;
            if (ungrabbed.body != null)
            {
                ungrabbed.body.isKinematic = ungrabbed.kinematicInitValue;
                ungrabbed.body.velocity = ungrabbed.bufferVelocity;
                ungrabbed.bufferVelocity = Vector3.zero;
                ungrabbed.body.angularVelocity = ungrabbed.bufferAngularVelocity;
                ungrabbed.bufferAngularVelocity = Vector3.zero;
            }
        }
        if (rightUngrabbedId != 0)
        {
            NetworkObject ungrabbed = objects.Find((NetworkObject g) => g.id == rightUngrabbedId);
            ungrabbed.grabbed = false;
            ungrabbed.leftHand = false;
            if (ungrabbed.body != null)
            {
                ungrabbed.body.isKinematic = ungrabbed.kinematicInitValue;
                ungrabbed.body.velocity = ungrabbed.bufferVelocity;
                ungrabbed.bufferVelocity = Vector3.zero;
                ungrabbed.body.angularVelocity = ungrabbed.bufferAngularVelocity;
                ungrabbed.bufferAngularVelocity = Vector3.zero;
            }
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
                obj.ClearBuffer();
            }
            else
            {
                leftGrab.grabbed = false;
                if (!DEVNetworkSwitcher.isServer)
                {
                    leftGrab.body.isKinematic = true;
                }
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
                obj.ClearBuffer();
            }
            else
            {
                rightGrab.grabbed = false;
                if (!DEVNetworkSwitcher.isServer)
                {
                    rightGrab.body.isKinematic = true;
                }
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
                Rotation = b.transform.rotation,
            };
        }
        return (entityStates);
    }
}
