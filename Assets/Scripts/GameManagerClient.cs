using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    public void AddEntity(Entity ent)
    {
        entities.Add(ent);
        if (ent.interactable != null)
        {
            ent.interactable.Grabbed.AddListener((InteractorFacade ifc) => OnLocalGrab(ent));
            ent.interactable.Ungrabbed.AddListener((InteractorFacade ifc) => OnLocalUngrab(ent));
        }
    }

    public void RemoveEntity(Entity ent)
    {
        entities.Remove(ent);
        if (ent.interactable != null)
        {
            ent.interactable.Grabbed.RemoveAllListeners();
            ent.interactable.Ungrabbed.RemoveAllListeners();
        }
    }

    private void OnLocalGrab(Entity ent)
    {
        
    }

    private void OnLocalUngrab(Entity ent)
    {
        
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
            Debug.Log("NOT ENOUGTH DATA RECEIVED");
            return;
        }
        StateMessage stateA = stateBuffer[0];
        StateMessage stateB = stateBuffer[1];

        LerpPlayers(stateA.Players, stateB.Players, t);
        
        if (isLastFrame)
        {
            stateBuffer.RemoveFromStart(1);
            stateBufferLength = stateBuffer.Count;
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

    private void SendInput()
    {
        client.SendInput(new PlayerInput()
        {
            Sequence = sequence,
            HeadPosition = localAvatar.headAlias.transform.position,
            HeadRotation = localAvatar.headAlias.transform.rotation,
            LeftHandPosition = localAvatar.leftHandAlias.transform.position,
            LeftHandRotation = localAvatar.leftHandAlias.transform.rotation,
            RightHandPosition = localAvatar.rightHandAlias.transform.position,
            RightHandRotation = localAvatar.rightHandAlias.transform.rotation,
            LeftPointer = localAvatar.leftPointer,
            RightPointer = localAvatar.rightPointer,
        });
        sequence++;
    }
    public void OnServerInit(InitMessage im)
    {
        localAvatar.id = im.OwnId;
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
            Debug.Log("TOO MUCH STATE RECEIVED");
            //Lag?
            stateBuffer.FastClear();
        }
        stateBuffer.Add(sm.Clone());
        stateBufferLength = stateBuffer.Count;
    }
}
