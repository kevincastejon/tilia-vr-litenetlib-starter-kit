using System;
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
    public List<Player> players = new List<Player>();
    [ReadOnly]
    public LiteRingBuffer<StateMessage> stateBuffer = new LiteRingBuffer<StateMessage>(5);
    //private LogicTimer logicTimer;
    private int lastReceivedSequence;
    private int localSequence;
    private float maxTimer = 32 / 1000f;
    private float timer = 0f;

    //private void Start()
    //{
    //    //logicTimer = new LogicTimer(OnLogicFrame);
    //    //logicTimer.Start();
    //}

    //private void Update()
    //{
    //    //logicTimer.Update();
    //}

    //private void OnLogicFrame()
    //{
    //    SendInput();
    //}

    private void FixedUpdate()
    {
        if (timer >= maxTimer)
        {
            SendInput();
        }
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
        //if (sm.Sequence <= lastReceivedSequence)
        //{
        //    return;
        //}
        //lastReceivedSequence = sm.Sequence;
        //serverStateBuffer.Add(sm.Clone());
    }
}
