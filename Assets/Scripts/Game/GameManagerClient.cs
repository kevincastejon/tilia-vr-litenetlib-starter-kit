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
    [Header("Reference Settings")]
    public LocalAvatar localAvatar;
    public Client client;
    public ColoredCube coloredCube;
    public EntitiesSettings entitiesSettings;
    [Header("Monitoring")]
    [ReadOnly]
    public int stateBufferLength;
    [ReadOnly]
    public List<Entity> entities = new List<Entity>();
    [HideInInspector]
    public LiteRingBuffer<StateMessage> stateBuffer = new LiteRingBuffer<StateMessage>(5);
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private int lastSequence;
    private int sequence;
    private bool ready;
    private bool connecting;
    private float timerMax = 2 / 60f;
    private float timer = 0f;
    [HideInInspector]
    public static GameManagerClient instance;

    private void Awake()
    {
        instance = this;
        client.StartLANDiscovery();
    }

    public void OnLANDiscovery(string name, IPEndPoint ip)
    {
        if (connecting)
        {
            return;
        }
        connecting = true;
        client.Connect(ip);
    }

    public void AddEntity(Entity ent)
    {
        entities.Add(ent);
    }

    public void RemoveEntity(Entity ent)
    {
        entities.Remove(ent);
    }

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
                continue;
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
                continue;
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
                p.LeftPointer = p.leftGrabbed != null ? false : playersStateA.LeftPointer;
                p.RightPointer = p.rightGrabbed != null ? false : playersStateA.RightPointer;
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
        client.SendFastMessage(new PlayerInput()
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
    public void OnAddPlayer(PlayerAddMessage pam)
    {
        Player p;
        p = Instantiate(playerPrefab).GetComponent<Player>();
        p.id = pam.Id;
        p.nameOrientationTarget = localAvatar.headAlias;
        p.headAlias.transform.position = pam.HeadPosition;
        p.headAlias.transform.rotation = pam.HeadRotation;
        p.leftHandAlias.transform.position = pam.LeftHandPosition;
        p.leftHandAlias.transform.rotation = pam.LeftHandRotation;
        p.rightHandAlias.transform.position = pam.RightHandPosition;
        p.rightHandAlias.transform.rotation = pam.RightHandRotation;
        p.LeftPointer = pam.LeftPointer;
        p.RightPointer = pam.RightPointer;
        players[p.id] = p;
    }
    public void OnRemovePlayer(PlayerRemoveMessage prm)
    {
        Player p = players[prm.Id];
        players.Remove(prm.Id);
        Destroy(p.gameObject);
    }
    public void OnAddEntity(EntityAddMessage eam)
    {
        Entity ent;
        Entity linkableEntity = entities.Find(x => x.id == 0 && (int)x.type == eam.Type);
        if (linkableEntity != null)
        {
            ent = linkableEntity;
        }
        else
        {
            ent = Instantiate(entitiesSettings.settings[eam.Type].prefab).GetComponent<Entity>();
        }
        ent.id = eam.Id;
        ent.transformTarget.position = eam.Position;
        ent.transformTarget.rotation = eam.Rotation;
    }
    public void OnRemoveEntity(EntityRemoveMessage erm)
    {
        Entity entity = entities.Find(x => x.id == erm.Id);
        entities.Remove(entity);
        Destroy(entity.gameObject);
    }
    public void OnServerInit(InitMessage im)
    {
        localAvatar.id = im.OwnId;
        localAvatar.TransportTo(new Vector3(0f, 10f, -12.5f));
        for (int i = 0; i < im.Players.Length; i++)
        {
            PlayerAddMessage pam = im.Players[i];
            OnAddPlayer(pam);
        }
        for (int i = 0; i < im.Entities.Length; i++)
        {
            EntityAddMessage eam = im.Entities[i];
            OnAddEntity(eam);
        }
        coloredCube.SetColor(im.ColoredCube);
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
