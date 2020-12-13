using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;

public class GameManagerClient : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject playerPrefab;
    public List<GameObject> entityPrefabs = new List<GameObject>();
    [Header("Reference Settings")]
    public LocalAvatar localAvatar;
    public GameClient client;
    public ColoredCube coloredCube;
    [Header("Monitoring")]
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private List<Entity> entities = new List<Entity>();
    [HideInInspector]
    public LiteRingBuffer<StateMessage> stateBuffer = new LiteRingBuffer<StateMessage>(5);
    [ReadOnly]
    public int stateBufferLength;
    private int lastSequence;
    private int sequence;
    private bool ready;
    private float timerMax = 2 / 60f;
    private float timer = 0f;
    [HideInInspector]
    public static GameManagerClient instance;

    private void Awake()
    {
        instance = this;
        Debug.Log(client.awaked);
        client.StartLANDiscovery();
    }

    public void OnLANDiscovery(string name, IPEndPoint ip)
    {
        client.Connect(ip);
    }

    public void AddEntity(Entity ent)
    {
        entities.Add(ent);
        //if (ent.interactable != null)
        //{
        //    ent.interactable.Grabbed.AddListener((InteractorFacade ifc) => OnLocalGrab(ent));
        //    ent.interactable.Ungrabbed.AddListener((InteractorFacade ifc) => OnLocalUngrab(ent));
        //}
    }

    public void RemoveEntity(Entity ent)
    {
        entities.Remove(ent);
        //if (ent.interactable != null)
        //{
        //    ent.interactable.Grabbed.RemoveAllListeners();
        //    ent.interactable.Ungrabbed.RemoveAllListeners();
        //}
    }

    //private void OnLocalGrab(Entity ent)
    //{
    //    ent.grabbed = true;
    //    ent.ownerId = localAvatar.id;
    //}

    //private void OnLocalUngrab(Entity ent)
    //{
    //    ent.grabbed = false;
    //}

    private void FixedUpdate()
    {
        if (!ready)
        {
            return;
        }
        float newTimer = timer + Time.fixedDeltaTime;
        bool isLastFrame = newTimer > timerMax;
        LerpStates(timer / timerMax, isLastFrame);
        timer = newTimer;
        if (isLastFrame)
        {
            timer -= timerMax;
            SendInput();
        }
    }

    private void LerpStates(float t, bool isLastFrame)
    {
        if (stateBuffer.Count < 2)
        {
            if (DEVNetworkSwitcher.showLagLogs)
            {
                Debug.Log("NOT ENOUGTH DATA RECEIVED");
            }
            return;
        }
        StateMessage stateA = stateBuffer[0];
        StateMessage stateB = stateBuffer[1];

        LerpPlayers(stateA.Players, stateB.Players, t);
        LerpEntities(stateA.Entities, stateB.Entities, t);
        coloredCube.SetColor(stateA.ColoredCube);

        if (isLastFrame)
        {
            stateBuffer.RemoveFromStart(1);
            stateBufferLength = stateBuffer.Count;
            if (stateBuffer.Count > 1)
            {
                DestroyOldPlayers(stateBuffer[1].Players);
                DestroyOldEntities(stateBuffer[1].Entities);
            }
        }
    }

    private void LerpEntities(EntityState[] entitiesA, EntityState[] entitiesB, float t)
    {
        for (int i = 0; i < entitiesB.Length; i++)
        {
            EntityState entityStateB = entitiesB[i];
            Entity ent = entities.Find(x => x.id == entityStateB.Id);
            if (ent == null)
            {
                Entity linkableEntity = entities.Find(x => x.id == 0 && (int)x.type == entityStateB.Type);
                if (linkableEntity != null)
                {
                    ent = linkableEntity;
                }
                else
                {
                    ent = Instantiate(entityPrefabs[entityStateB.Type]).GetComponent<Entity>();
                    entities.Add(ent);
                }
                ent.id = entityStateB.Id;
                ent.transformTarget.position = entityStateB.Position;
                ent.transformTarget.rotation = entityStateB.Rotation;
            }
            else
            {
                Entity leftGrabbedEnt = localAvatar.GetLeftGrabbedEntity();
                Entity rightGrabbedEnt = localAvatar.GetRightGrabbedEntity();
                if (leftGrabbedEnt && leftGrabbedEnt.id == ent.id || rightGrabbedEnt && rightGrabbedEnt.id == ent.id)
                {
                    continue;
                }
            }
            EntityState entityStateA = null;
            for (int j = 0; j < entitiesA.Length; j++)
            {
                if (entitiesA[j].Id == entityStateB.Id)
                {
                    entityStateA = entitiesA[j];
                }
            }
            if (entityStateA != null)
            {
                ent.transformTarget.position = Vector3.Lerp(entityStateA.Position, entityStateB.Position, t);
                ent.transformTarget.rotation = Quaternion.Lerp(entityStateA.Rotation, entityStateB.Rotation, t);
                ent.ownerId = entityStateA.Owner;
                if (ent.interactable)
                {
                    if (ent.ownerId != -1 && ent.ownerId != localAvatar.id)
                    {
                        //Debug.Log("owner id : "+ent.ownerId+" - local id : "+localAvatar.id);
                        //Debug.Log("DISABLING GRAB ACTION");
                        ent.interactable.DisableGrab();
                    }
                    else
                    {
                        ent.interactable.EnableGrab();
                    }
                }
            }
        }
    }

    private void LerpPlayers(PlayerState[] playersA, PlayerState[] playersB, float t)
    {
        for (int i = 0; i < playersB.Length; i++)
        {
            PlayerState playersStateB = playersB[i];
            Player p = players.ContainsKey(playersStateB.Id) ? players[playersStateB.Id] : null;
            if (p == null)
            {
                p = Instantiate(playerPrefab).GetComponent<Player>();
                p.id = playersStateB.Id;
                p.headAlias.transform.position = playersStateB.HeadPosition;
                p.headAlias.transform.rotation = playersStateB.HeadRotation;
                p.leftHandAlias.transform.position = playersStateB.LeftHandPosition;
                p.leftHandAlias.transform.rotation = playersStateB.LeftHandRotation;
                p.rightHandAlias.transform.position = playersStateB.RightHandPosition;
                p.rightHandAlias.transform.rotation = playersStateB.RightHandRotation;
                p.LeftPointer = playersStateB.LeftPointer;
                p.RightPointer = playersStateB.RightPointer;
                players[p.id] = p;
            }
            PlayerState playersStateA = null;
            for (int j = 0; j < playersA.Length; j++)
            {
                if (playersA[j].Id == playersStateB.Id)
                {
                    playersStateA = playersA[j];
                }
            }
            if (playersStateA != null)
            {
                p.headAlias.transform.position = Vector3.Lerp(playersStateA.HeadPosition, playersStateB.HeadPosition, t);
                p.headAlias.transform.rotation = Quaternion.Lerp(playersStateA.HeadRotation, playersStateB.HeadRotation, t);
                p.leftHandAlias.transform.position = Vector3.Lerp(playersStateA.LeftHandPosition, playersStateB.LeftHandPosition, t);
                p.leftHandAlias.transform.rotation = Quaternion.Lerp(playersStateA.LeftHandRotation, playersStateB.LeftHandRotation, t);
                p.rightHandAlias.transform.position = Vector3.Lerp(playersStateA.RightHandPosition, playersStateB.RightHandPosition, t);
                p.rightHandAlias.transform.rotation = Quaternion.Lerp(playersStateA.RightHandRotation, playersStateB.RightHandRotation, t);
                p.LeftPointer = playersStateA.LeftPointer;
                p.RightPointer = playersStateA.RightPointer;
            }
        }
    }

    private void DestroyOldPlayers(PlayerState[] playerStates)
    {
        List<int> oldKeys = new List<int>();
        foreach (KeyValuePair<int, Player> entry in players)
        {
            int key = entry.Key;
            Player player = entry.Value;
            bool presence = false;
            for (int i = 0; i < playerStates.Length; i++)
            {
                if (playerStates[i].Id == player.id)
                {
                    presence = true;
                    break;
                }
            }
            if (!presence)
            {
                oldKeys.Add(key);
            }
        }
        for (int i = 0; i < oldKeys.Count; i++)
        {
            int key = oldKeys[i];
            Player p = players[key];
            players.Remove(key);
            Destroy(p.gameObject);
        }
    }
    private void DestroyOldEntities(EntityState[] entityStates)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            Entity entity = entities[i];
            bool presence = false;
            for (int j = 0; j < entityStates.Length; j++)
            {
                if (entityStates[j].Id == entity.id)
                {
                    presence = true;
                    break;
                }
            }
            if (!presence)
            {
                entities.Remove(entity);
                Destroy(entity.gameObject);
            }
        }
    }

    private void SendInput()
    {
        int leftId = 0;
        Entity leftEnt = null;
        if (localAvatar.leftGrabbed != null)
        {
            leftEnt = localAvatar.leftGrabbed.GetComponent<Entity>();
            if (leftEnt != null)
            {
                leftId = leftEnt.id;
            }
        }
        int rightId = 0;
        Entity rightEnt = null;
        if (localAvatar.rightGrabbed != null)
        {
            rightEnt = localAvatar.rightGrabbed.GetComponent<Entity>();
            if (rightEnt != null)
            {
                rightId = rightEnt.id;
            }
        }
        client.SendInput(new PlayerInput()
        {
            Sequence = sequence,
            HeadPosition = localAvatar.headAlias.transform.position,
            HeadRotation = localAvatar.headAlias.transform.rotation,
            LeftHandPosition = localAvatar.leftHandAlias.transform.position,
            LeftHandRotation = localAvatar.leftHandAlias.transform.rotation,
            RightHandPosition = localAvatar.rightHandAlias.transform.position,
            RightHandRotation = localAvatar.rightHandAlias.transform.rotation,
            LeftGrabId = leftId,
            LeftGrabPosition = leftId == 0 ? Vector3.zero : leftEnt.transformTarget.position,
            LeftGrabRotation = leftId == 0 ? Quaternion.identity : leftEnt.transformTarget.rotation,
            LeftGrabVelocity = leftId == 0 ? Vector3.zero : localAvatar.leftGrabVelocity,
            LeftGrabAngularVelocity = leftId == 0 ? Vector3.zero : localAvatar.leftGrabAngularVelocity,
            RightGrabId = rightId,
            RightGrabPosition = rightId == 0 ? Vector3.zero : rightEnt.transformTarget.position,
            RightGrabRotation = rightId == 0 ? Quaternion.identity : rightEnt.transformTarget.rotation,
            RightGrabVelocity = rightId == 0 ? Vector3.zero : localAvatar.rightGrabVelocity,
            RightGrabAngularVelocity = rightId == 0 ? Vector3.zero : localAvatar.rightGrabAngularVelocity,
            LeftPointer = localAvatar.leftPointer,
            RightPointer = localAvatar.rightPointer,
            LeftTrigger = localAvatar.leftTrigger,
            RightTrigger = localAvatar.rightTrigger,
        });
        sequence++;
    }
    public void OnServerInit(InitMessage im)
    {
        localAvatar.id = im.OwnId;
        localAvatar.TransportTo(new Vector3(0f, 10f, -12.5f));
        ready = true;
    }
    public void OnServerState(StateMessage sm)
    {
        if (sm.Sequence <= lastSequence)
        {
            return;
        }
        lastSequence = sm.Sequence;
        if (stateBuffer.IsFull)
        {
            if (DEVNetworkSwitcher.showLagLogs)
            {
                Debug.Log("TOO MUCH STATE RECEIVED");
            }
            //Lag?
            stateBuffer.FastClear();
        }
        stateBuffer.Add(sm.Clone());
        stateBufferLength = stateBuffer.Count;
    }
}
