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
    [ReadOnly]
    public bool connecting;
    [ReadOnly]
    public bool connected;
    [HideInInspector]
    public LiteRingBuffer<StateMessage> stateBuffer = new LiteRingBuffer<StateMessage>(5);
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private int lastSequence;
    private int sequence;
    private int lerpingSequence;
    private float timerMax = 2 / 60f;
    private float timer = 0f;
    [HideInInspector]
    public static GameManagerClient instance;
    private bool newFrameTem = true;

    private void Awake()
    {
        instance = this;
        client.StartLANDiscovery();
        Debug.Log("GAMEMANAGER CLIENT AWAKED");
    }

    public void OnLANDiscovery(string name, IPEndPoint ip)
    {
        Debug.Log("DISCOVERED LAN GAME ON IP:" + ip);
        if (connecting)
        {
            return;
        }
        connecting = true;
        client.Connect(ip);
    }

    public void OnConnected()
    {
        connecting = false;
        client.SendImportantMessage(new PlayerInitInfo()
        {
            OculusID = OculusAuthentifier.OculusId,
            HeadPosition = localAvatar.headAlias.transform.position,
            HeadRotation = localAvatar.headAlias.transform.rotation,
            LeftHandPosition = localAvatar.leftHandAlias.transform.position,
            LeftHandRotation = localAvatar.leftHandAlias.transform.rotation,
            RightHandPosition = localAvatar.rightHandAlias.transform.position,
            RightHandRotation = localAvatar.rightHandAlias.transform.rotation,
            LeftPointer = localAvatar.leftPointer,
            RightPointer = localAvatar.rightPointer,
            LeftTrigger = localAvatar.leftTrigger,
            RightTrigger = localAvatar.rightTrigger,
        });
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
        if (!connected)
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
        //Debug.Log("LERPING -> ISLASTFRAME : "+isLastFrame);
        if (stateBuffer.Count < 4)
        {
            if (NetworkManager.showLagLogs)
            {
                Debug.Log("NOT ENOUGTH DATA RECEIVED");
            }
            return;
        }
        StateMessage stateA = stateBuffer[0];
        StateMessage stateB = stateBuffer[1];
        lerpingSequence = stateA.Sequence;
        if (newFrameTem)
        {
            for (int i = 0; i < stateA.Entities.Length; i++)
            {
                Entity ent = entities.Find(x => x.id == stateA.Entities[i].Id);
                if (ent == null || ent.stateA != null)
                {
                    continue;
                }
                ent.stateA = stateA.Entities[i];
                ent.sequenceA = stateA.Sequence;
            }
            for (int i = 0; i < stateB.Entities.Length; i++)
            {
                Entity ent = entities.Find(x => x.id == stateB.Entities[i].Id);
                if (ent == null)
                {
                    continue;
                }
                if (ent.stateB != null)
                {
                    ent.stateA = ent.stateB;
                    ent.sequenceA = ent.sequenceB;
                }
                ent.stateB = stateB.Entities[i];
                ent.sequenceB = stateB.Sequence;
            }
        }
        LerpPlayers(stateA.Players, stateB.Players, t);
        //LerpPlayers(stateA.Players, stateB.Players, t);
        //Entity leftGrabbedEnt = localAvatar.GetLeftGrabbedEntity();
        //Entity rightGrabbedEnt = localAvatar.GetRightGrabbedEntity();
        //for (int i = 0; i < entities.Count; i++)
        //{
        //    Entity ent = entities[i];
        //    if (leftGrabbedEnt && leftGrabbedEnt.id == ent.id || rightGrabbedEnt && rightGrabbedEnt.id == ent.id)
        //    {
        //        continue;
        //    }
        //    ent.Lerp(t);
        //}
        LerpEntities(t, stateB.Sequence);
        coloredCube.SetColor(stateA.ColoredCube);
        if (newFrameTem)
        {
            newFrameTem = false;
        }
        if (isLastFrame)
        {
            //for (int i = 0; i < stateA.Entities.Length; i++)
            //{
            //    EntityState es = stateA.Entities[i];
            //    Entity ent = entities.Find(x => x.id == es.Id);
            //    if (ent == null)
            //    {
            //        continue;
            //    }
            //    ent.PushState(es, stateA.Sequence);
            //}
            stateBuffer.RemoveFromStart(1);
            stateBufferLength = stateBuffer.Count;
            newFrameTem = true;
        }
    }

    private void LerpEntities(float t, int currentSequence)
    {
        for (int i = 0; i < entities.Count; i++)
        {
            Entity ent = entities[i];
            Entity leftGrabbedEnt = localAvatar.GetLeftGrabbedEntity();
            Entity rightGrabbedEnt = localAvatar.GetRightGrabbedEntity();
            if ((leftGrabbedEnt && leftGrabbedEnt.id == ent.id) || (rightGrabbedEnt && rightGrabbedEnt.id == ent.id) || ent.stateA == null || ent.stateB == null)
            {
                continue;
            }
            //EntityState entityStateA = null;
            //for (int j = 0; j < entitiesA.Length; j++)
            //{
            //    if (entitiesA[j].Id == entityStateB.Id)
            //    {
            //        entityStateA = entitiesA[j];
            //    }
            //}
            int len = ent.sequenceB - ent.sequenceA;
            int realCurrentSeq = currentSequence - ent.sequenceB;
            float realT = (((realCurrentSeq / t) + 1) * t) / len;
            //if (i == 0)
            //{
            //    Debug.Log("LERPING T:" + t + " currentSequence:" + currentSequence);
            //    Debug.Log("STATEA SEQ:" + ent.sequenceA);
            //    Debug.Log("STATEB SEQ:" + ent.sequenceB);
            //    Debug.Log("LEN:" + len);
            //    Debug.Log("REAL SEQ:" + realCurrentSeq);
            //    Debug.Log("REAL T:" + realT);
            //}
            ent.transformTarget.position = Vector3.Lerp(ent.stateA.Position, ent.stateB.Position, realT);
            ent.transformTarget.rotation = Quaternion.Lerp(ent.stateA.Rotation, ent.stateB.Rotation, realT);
            ent.ownerId = ent.stateA.Owner;
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

    private void LerpPlayers(PlayerState[] playersA, PlayerState[] playersB, float t)
    {
        for (int i = 0; i < playersB.Length; i++)
        {
            PlayerState playersStateB = playersB[i];
            Player p = players.ContainsKey(playersStateB.Id) ? players[playersStateB.Id] : null;
            p.playerName.transform.rotation = Quaternion.LookRotation(p.playerName.transform.position - p.nameOrientationTarget.transform.position);
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
        p.connected = true;
        p.oculusId = pam.OculusId;
        p.playerName.text = pam.OculusId;
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
        ent.stateA = new EntityState() {
            Id = eam.Id,
            Owner = eam.Owner,
            Position= eam.Position,
            Rotation = eam.Rotation,
            Type = eam.Type,
        };
        ent.sequenceA = lerpingSequence;
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
        connected = true;
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
            if (NetworkManager.showLagLogs)
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
