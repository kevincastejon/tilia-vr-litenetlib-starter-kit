﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerClient : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    [Header("Reference Settings")]
    public LocalAvatar localAvatar;
    public GameClient client;
    [Header("Monitoring")]
    [ReadOnly]
    public Dictionary<int, Player> players = new Dictionary<int, Player>();
    [ReadOnly]
    public LiteRingBuffer<StateMessage> stateBuffer = new LiteRingBuffer<StateMessage>(5);
    private LogicTimer logicTimer;
    private int lastReceivedSequence;
    private int localSequence;
    private float _receivedTime;
    private float _timer;
    private const float BufferTime = 0.1f; //100 milliseconds
    private int stateBufferLength;

    private void Start()
    {
        logicTimer = new LogicTimer(OnLogicFrame);
        logicTimer.Start();
    }

    private void Update()
    {
        logicTimer.Update();
        LerpStates(Time.deltaTime);
    }

    private void OnLogicFrame()
    {
        SendInput();
    }

    private void LerpStates(float delta)
    {
        if (_receivedTime < BufferTime || stateBuffer.Count < 2)
        {
            return;
        }
        StateMessage stateA = stateBuffer[0];
        StateMessage stateB = stateBuffer[1];
        float lerpTime = NetworkGeneral.SeqDiff(stateA.Sequence, stateB.Sequence) * LogicTimer.FixedDelta;
        float t = _timer / lerpTime;
        for (int i = 0; i < stateB.Players.Length; i++)
        {
            PlayerState playersStateB = stateB.Players[i];
            Player p = players[playersStateB.Id];
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
                p.leftPointer = playersStateB.LeftPointer;
                p.rightPointer = playersStateB.RightPointer;
                players[p.id] = p;
            }
            PlayerState dataA = null;
            PlayerState dataB = stateB.Players[i];
            for (int j = 0; j < stateA.Players.Length; j++)
            {
                if (stateA.Players[j].Id == dataB.Id)
                {
                    dataA = stateA.Players[j];
                }
            }
            if (dataA != null)
            {
            p.headAlias.transform.position = Vector3.Lerp(dataA.HeadPosition, dataB.HeadPosition, t);
            p.headAlias.transform.rotation = Quaternion.Lerp(dataA.HeadRotation, dataB.HeadRotation, t);
            p.leftHandAlias.transform.position = Vector3.Lerp(dataA.LeftHandPosition, dataB.LeftHandPosition, t);
            p.leftHandAlias.transform.rotation = Quaternion.Lerp(dataA.LeftHandRotation, dataB.LeftHandRotation, t);
            p.rightHandAlias.transform.position = Vector3.Lerp(dataA.RightHandPosition, dataB.RightHandPosition, t);
            p.rightHandAlias.transform.rotation = Quaternion.Lerp(dataA.RightHandRotation, dataB.RightHandRotation, t);
            }
        }
        _timer += delta;
        if (_timer > lerpTime)
        {
            _receivedTime -= lerpTime;
            stateBuffer.RemoveFromStart(1);
            stateBufferLength = stateBuffer.Count;
            _timer -= lerpTime;
        }

        //for (int i = 0; i < players.Count; i++)
        //{
        //    Player p = players[i];
        //    if (_receivedTime < BufferTime || stateBuffer.Count < 2)
        //        return;
        //    PlayerState dataA = stateA.Players[i];
        //    PlayerState dataB = stateB.Players[i];

        //    float lerpTime = NetworkGeneral.SeqDiff(stateA.Sequence, stateB.Sequence) * LogicTimer.FixedDelta;
        //    float t = _timer / lerpTime;
        //    p.headAlias.transform.position = Vector3.Lerp(dataA.HeadPosition, dataB.HeadPosition, t);
        //    p.headAlias.transform.rotation = Quaternion.Lerp(dataA.HeadRotation, dataB.HeadRotation, t);
        //    p.leftHandAlias.transform.position = Vector3.Lerp(dataA.LeftHandPosition, dataB.LeftHandPosition, t);
        //    p.leftHandAlias.transform.rotation = Quaternion.Lerp(dataA.LeftHandRotation, dataB.LeftHandRotation, t);
        //    p.rightHandAlias.transform.position = Vector3.Lerp(dataA.RightHandPosition, dataB.RightHandPosition, t);
        //    p.rightHandAlias.transform.rotation = Quaternion.Lerp(dataA.RightHandRotation, dataB.RightHandRotation, t);
        //    _timer += delta;
        //    if (_timer > lerpTime)
        //    {
        //        _receivedTime -= lerpTime;
        //        stateBuffer.RemoveFromStart(1);
        //        stateBufferLength = stateBuffer.Count;
        //        _timer -= lerpTime;
        //    }
        //}
    }

    private void SendInput()
    {
        client.SendInput(new PlayerInput()
        {
            Sequence = localSequence,
            HeadPosition = localAvatar.headAlias.transform.position,
            HeadRotation = localAvatar.headAlias.transform.rotation,
            LeftHandPosition = localAvatar.leftHandAlias.transform.position,
            LeftHandRotation = localAvatar.leftHandAlias.transform.rotation,
            RightHandPosition = localAvatar.rightHandAlias.transform.position,
            RightHandRotation = localAvatar.rightHandAlias.transform.rotation,
            LeftPointer = localAvatar.leftPointer,
            RightPointer = localAvatar.rightPointer,
        });
        localSequence++;
    }
    public void OnServerInit(InitMessage im)
    {
        localAvatar.id = im.OwnId;
    }
    public void OnServerState(StateMessage sm)
    {
        int diff = NetworkGeneral.SeqDiff(sm.Sequence, stateBuffer.Last.Sequence);
        if (diff <= 0)
            return;

        _receivedTime += diff * LogicTimer.FixedDelta;
        if (stateBuffer.IsFull)
        {
            //Debug.LogWarning("[C] Remote: Something happened");
            //Lag?
            _receivedTime = 0f;
            stateBuffer.FastClear();
        }
        stateBuffer.Add(sm.Clone());
        stateBufferLength = stateBuffer.Count;
        //if (sm.Sequence <= lastReceivedSequence)
        //{
        //    return;
        //}
        //lastReceivedSequence = sm.Sequence;
        //serverStateBuffer.Add(sm.Clone());
    }
}
