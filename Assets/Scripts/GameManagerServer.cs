using System.Collections.Generic;
using Tilia.Indicators.ObjectPointers;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using Unity.Collections;
using UnityEngine;

public class PlayersInputManager
{
    public readonly List<PlayerInput> playerInputsBuffer = new List<PlayerInput>();
    public PlayerInput inputA;
    public PlayerInput inputB;
}

public class GameManagerServer : MonoBehaviour
{
    [ReadOnly]
    public int serverId = -1;
    public GameServer server;
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    [ReadOnly]
    public NetworkObject leftGrab;
    [ReadOnly]
    public NetworkObject rightGrab;
    [ReadOnly]
    public bool leftPointer;
    [ReadOnly]
    public bool rightPointer;
    [ReadOnly]
    public bool leftShooting;
    [ReadOnly]
    public bool rightShooting;
    public int maxBullets = 12;
    [ReadOnly]
    public int sequence = 0;
    private NetworkManager networkObjectManager;
    public List<NetworkObject> networkObjects = new List<NetworkObject>();
    private readonly List<Player> players = new List<Player>();
    Dictionary<int, PlayersInputManager> playersInputManager = new Dictionary<int, PlayersInputManager>();
    private float lerpMax = 1 / 60f;
    private float lerpTimer = 0f;
    private float sendRate = 50 / 1000f;
    private float sendTimer = 0f;

    private void Start()
    {
        networkObjectManager = NetworkManager.GetInstance();
    }

    private void FixedUpdate()
    {
        sendTimer += Time.fixedDeltaTime;
        if (true)
        //if (sendTimer >= sendRate)
        {
            sendTimer = 0f;
            sequence++;
            for (int i = 0; i < players.Count; i++)
            {
                if (!players[i].ready)
                {
                    continue;
                }
                StateMessage sm = GetWorldState(players[i].id);
                server.SendWorldState(sm, players[i].id);
            }
        }

        foreach (KeyValuePair<int, PlayersInputManager> entry in playersInputManager)
        {
            Player player = players.Find(x => x.id == entry.Key);
            List<PlayerInput> inputBuffer = entry.Value.playerInputsBuffer;
            bool isLerping = entry.Value.inputA != null && entry.Value.inputB != null;
            if (inputBuffer.Count >= 2 && !isLerping)
            {
                if (entry.Value.inputA == null)
                {
                    entry.Value.inputA = inputBuffer[0];
                    inputBuffer.RemoveAt(0);
                }
                entry.Value.inputB = inputBuffer[0];
                inputBuffer.RemoveAt(0);
                isLerping = true;
                player.SetLeftPointer(entry.Value.inputA.LeftPointer);
                player.SetRightPointer(entry.Value.inputA.RightPointer);
                int leftUngrabbedId = player.leftGrabId != 0 && entry.Value.inputA.LeftGrabId == 0 ? player.leftGrabId : 0;
                int rightUngrabbedId = player.rightGrabId != 0 && entry.Value.inputA.RightGrabId == 0 ? player.rightGrabId : 0;
                player.leftGrabId = entry.Value.inputA.LeftGrabId;
                player.rightGrabId = entry.Value.inputA.RightGrabId;
                if (leftUngrabbedId != 0)
                {
                    NetworkObject ungrabbed = networkObjects.Find((NetworkObject g) => g.id == leftUngrabbedId);
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
                    NetworkObject ungrabbed = networkObjects.Find((NetworkObject g) => g.id == rightUngrabbedId);
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
                if (entry.Value.inputA.LeftGrabId != 0)
                {
                    NetworkObject grabbed = networkObjects.Find((NetworkObject g) => g.id == entry.Value.inputA.LeftGrabId);
                    if (grabbed.snapContainer != null)
                    {
                        grabbed.snapContainer.Unsnap();
                        grabbed.snapContainer = null;
                    }
                    grabbed.grabbed = true;
                    grabbed.leftHand = true;
                    grabbed.lastOwnerId = player.id;
                    if (grabbed.body != null)
                    {
                        grabbed.body.isKinematic = true;
                        grabbed.bufferVelocity = entry.Value.inputA.LeftGrabVelocity;
                        grabbed.bufferAngularVelocity = entry.Value.inputA.LeftGrabAngularVelocity;
                    }
                    //grabbed.AddStateToBuffer(new EntityState() { Position = inputA.LeftGrabPosition, Rotation = inputA.LeftGrabRotation });
                }
                if (entry.Value.inputA.RightGrabId != 0)
                {
                    NetworkObject grabbed = networkObjects.Find((NetworkObject g) => g.id == entry.Value.inputA.RightGrabId);
                    if (grabbed.snapContainer != null)
                    {
                        grabbed.snapContainer.Unsnap();
                        grabbed.snapContainer = null;
                    }
                    grabbed.grabbed = true;
                    grabbed.leftHand = false;
                    grabbed.lastOwnerId = player.id;
                    if (grabbed.body != null)
                    {
                        grabbed.body.isKinematic = true;
                        grabbed.bufferVelocity = entry.Value.inputA.RightGrabVelocity;
                        grabbed.bufferAngularVelocity = entry.Value.inputA.RightGrabAngularVelocity;
                    }
                    //grabbed.AddStateToBuffer(new EntityState() { Position = inputA.RightGrabPosition, Rotation = inputA.RightGrabRotation });
                }
                //networkObjectManager.ServerSideSyncClientUngrabbed(leftUngrabbedId, rightUngrabbedId);
                //networkObjectManager.ServerSideSyncClientGrabbing(peerID, inputA);
                if (!player.leftShooting && entry.Value.inputA.LeftShooting && player.leftGrabId != 0)
                {
                    NetworkObject obj = networkObjects.Find((NetworkObject g) => g.id == player.leftGrabId);
                    Transform spawnPoint = obj.GetComponent<Gun>().spawnPoint.transform;
                    SpawnBullet(spawnPoint);
                }
                player.leftShooting = entry.Value.inputA.LeftShooting;
                if (!player.rightShooting && entry.Value.inputA.RightShooting && player.rightGrabId != 0)
                {
                    NetworkObject obj = networkObjects.Find((NetworkObject g) => g.id == player.rightGrabId);
                    Transform spawnPoint = obj.GetComponent<Gun>().spawnPoint.transform;
                    SpawnBullet(spawnPoint);
                }
                player.rightShooting = entry.Value.inputA.RightShooting;
            }
            if (isLerping)
            {
                player.headGO.transform.position = Vector3.Lerp(entry.Value.inputA.HeadPosition, entry.Value.inputB.HeadPosition, lerpTimer / lerpMax);
                player.headGO.transform.rotation = Quaternion.Lerp(entry.Value.inputA.HeadRotation, entry.Value.inputB.HeadRotation, lerpTimer / lerpMax);
                player.leftGO.transform.position = Vector3.Lerp(entry.Value.inputA.LeftHandPosition, entry.Value.inputB.LeftHandPosition, lerpTimer / lerpMax);
                player.leftGO.transform.rotation = Quaternion.Lerp(entry.Value.inputA.LeftHandRotation, entry.Value.inputB.LeftHandRotation, lerpTimer / lerpMax);
                player.rightGO.transform.position = Vector3.Lerp(entry.Value.inputA.RightHandPosition, entry.Value.inputB.RightHandPosition, lerpTimer / lerpMax);
                player.rightGO.transform.rotation = Quaternion.Lerp(entry.Value.inputA.RightHandRotation, entry.Value.inputB.RightHandRotation, lerpTimer / lerpMax);
                NetworkObject leftGrabbed = networkObjects.Find((NetworkObject g) => g.id == entry.Value.inputB.LeftGrabId);
                NetworkObject rightGrabbed = networkObjects.Find((NetworkObject g) => g.id == entry.Value.inputB.RightGrabId);
                if (leftGrabbed != null)
                {
                    leftGrabbed.transform.position = Vector3.Lerp(entry.Value.inputA.LeftGrabPosition, entry.Value.inputB.LeftGrabPosition, lerpTimer / lerpMax);
                    leftGrabbed.transform.rotation = Quaternion.Lerp(entry.Value.inputA.LeftGrabRotation, entry.Value.inputB.LeftGrabRotation, lerpTimer / lerpMax);
                }
                if (rightGrabbed != null)
                {
                    rightGrabbed.transform.position = Vector3.Lerp(entry.Value.inputA.RightGrabPosition, entry.Value.inputB.RightGrabPosition, lerpTimer / lerpMax);
                    rightGrabbed.transform.rotation = Quaternion.Lerp(entry.Value.inputA.RightGrabRotation, entry.Value.inputB.RightGrabRotation, lerpTimer / lerpMax);
                }

            }
            lerpTimer += Time.fixedDeltaTime;
            if (true)
            //if (lerpTimer >= lerpMax)
            {
                lerpTimer = 0f;
                entry.Value.inputA = entry.Value.inputB;
                entry.Value.inputB = null;
            }
        }
    }

    public void SetGrab(NetworkObject obj, bool leftHand)
    {
        int ownId = serverId;
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
            leftGrab = obj;
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
            rightGrab = obj;
        }
    }

    public void SetShootingLeft(bool shooting)
    {
        leftShooting = shooting;
        if (shooting)
        {
            Transform spawnPoint = leftGrab.GetComponent<Gun>().spawnPoint.transform;
            SpawnBullet(spawnPoint);
        }
    }

    public void SetShootingRight(bool shooting)
    {
        rightShooting = shooting;
        if (shooting)
        {
            Transform spawnPoint = rightGrab.GetComponent<Gun>().spawnPoint.transform;
            SpawnBullet(spawnPoint);
        }
    }
    public void SetPointerLeft(bool activated)
    {
        leftPointer = activated;
    }

    public void SetPointerRight(bool activated)
    {
        rightPointer = activated;
    }

    public void SpawnBullet(Transform spawnPoint)
    {
        GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 1, ForceMode.Impulse);
        networkObjects.Add(bullet.GetComponent<NetworkObject>());
    }

    public void OnClientConnected(int peerID)
    {
        Player newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
        newPlayer.SetNameOrientationTarget(headGO);
        newPlayer.id = peerID;
        playersInputManager.Add(peerID, new PlayersInputManager());
        players.Add(newPlayer);
        InitMessage im = new InitMessage()
        {
            OwnId = peerID
        };
        server.SendInitMessage(im, peerID);
    }

    public void OnClientDisconnected(int peerID)
    {
        Player disconnectedPlayer = players.Find(x => x.id == peerID);
        players.Remove(disconnectedPlayer);
        playersInputManager.Remove(peerID);
        int leftUngrabbedId = disconnectedPlayer.leftGrabId != 0 ? disconnectedPlayer.leftGrabId : 0;
        int rightUngrabbedId = disconnectedPlayer.rightGrabId != 0 ? disconnectedPlayer.rightGrabId : 0;
        if (leftUngrabbedId != 0)
        {
            NetworkObject ungrabbed = networkObjects.Find((NetworkObject g) => g.id == leftUngrabbedId);
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
            NetworkObject ungrabbed = networkObjects.Find((NetworkObject g) => g.id == rightUngrabbedId);
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
        Destroy(disconnectedPlayer.gameObject);
    }

    public void OnClientInput(int peerID, PlayerInput pi)
    {
        Player p = players.Find(x => x.id == peerID);
        p.ready = true;
        playersInputManager[peerID].playerInputsBuffer.Add(pi.Clone());
        //playersInputBufferLength = playersInputManager[peerID].playerInputsBuffer.Count;
        //Player player = players.Find(x => x.GetComponent<Player>().id == peerID).GetComponent<Player>();
        //Vector3 hp = pi.HeadPosition;
        //Quaternion hr = pi.HeadRotation;
        //Vector3 lp = pi.LeftHandPosition;
        //Quaternion lr = pi.LeftHandRotation;
        //Vector3 rp = pi.RightHandPosition;
        //Quaternion rr = pi.RightHandRotation;
        //player.AddStateToBuffer(new PlayerState()
        //{
        //    HeadPosition = new Vector3(hp.x,hp.y,hp.z),
        //    HeadRotation = new Quaternion(hr.x,hr.y,hr.z,hr.w),
        //    LeftHandPosition = new Vector3(lp.x,lp.y,lp.z),
        //    LeftHandRotation = new Quaternion(lr.x, lr.y, lr.z, lr.w),
        //    RightHandPosition = new Vector3(rp.x,rp.y,rp.z),
        //    RightHandRotation = new Quaternion(rr.x, rr.y, rr.z, rr.w),
        //    LeftPointer = pi.LeftPointer,
        //    RightPointer = pi.RightPointer
        //});
        //player.SetLeftPointer(pi.LeftPointer);
        //player.SetRightPointer(pi.RightPointer);
        //int leftUngrabbedId = player.leftGrabId != 0 && pi.LeftGrabId == 0 ? player.leftGrabId : 0;
        //int rightUngrabbedId = player.rightGrabId != 0 && pi.RightGrabId == 0 ? player.rightGrabId : 0;
        //player.leftGrabId = pi.LeftGrabId;
        //player.rightGrabId = pi.RightGrabId;
        //networkObjectManager.ServerSideSyncClientUngrabbed(leftUngrabbedId, rightUngrabbedId);
        //networkObjectManager.ServerSideSyncClientGrabbing(peerID, pi);
        //if (!player.leftShooting && pi.LeftShooting && player.leftGrabId != 0)
        //{
        //    NetworkObject obj = networkObjectManager.GetObject(player.leftGrabId);
        //    Transform spawnPoint = obj.GetComponent<Gun>().spawnPoint.transform;
        //    SpawnBullet(spawnPoint);
        //}
        //player.leftShooting = pi.LeftShooting;
        //if (!player.rightShooting && pi.RightShooting && player.rightGrabId != 0)
        //{
        //    NetworkObject obj = networkObjectManager.GetObject(player.rightGrabId);
        //    Transform spawnPoint = obj.GetComponent<Gun>().spawnPoint.transform;
        //    SpawnBullet(spawnPoint);
        //}
        //player.rightShooting = pi.RightShooting;
    }

    private StateMessage GetWorldState(int playerId)
    {
        PlayerState[] playerStates = new PlayerState[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            Player p = players[i];
            if (p.id == playerId)
            {
                continue;
            }
            playerStates[i] = new PlayerState()
            {
                Id = p.id,
                HeadPosition = p.headGO.transform.position,
                HeadRotation = p.headGO.transform.rotation,
                LeftHandPosition = p.leftGO.transform.position,
                LeftHandRotation = p.leftGO.transform.rotation,
                RightHandPosition = p.rightGO.transform.position,
                RightHandRotation = p.rightGO.transform.rotation,
                LeftPointer = p.leftPointerActivated,
                RightPointer = p.rightPointerActivated
            };
        }

        playerStates[players.Count-1] = new PlayerState()
        {
            Id = serverId,
            HeadPosition = headGO.transform.position,
            HeadRotation = headGO.transform.rotation,
            LeftHandPosition = leftGO.transform.position,
            LeftHandRotation = leftGO.transform.rotation,
            RightHandPosition = rightGO.transform.position,
            RightHandRotation = rightGO.transform.rotation,
            LeftPointer = leftPointer,
            RightPointer = rightPointer,
        };
        //for (int i = 0; i < playerStates.Length; i++)
        //{
        //    Debug.Log(playerStates[i].Id);
        //}
        EntityState[] entityStates = new EntityState[networkObjects.Count];
        for (int i = 0; i < networkObjects.Count; i++)
        {
            NetworkObject b = networkObjects[i];
            entityStates[i] = new EntityState()
            {
                Id = b.id,
                Type = (byte)b.type,
                Position = b.transform.position,
                Rotation = b.transform.rotation,
            };
        }
        StateMessage sm = new StateMessage()
        {
            Sequence = sequence,
            Players = playerStates,
            Entities = entityStates,
        };
        return sm;
    }
}
