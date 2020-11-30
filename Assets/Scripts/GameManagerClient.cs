using System;
using System.Collections.Generic;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using UnityEngine;

public class GameManagerClient : MonoBehaviour
{
    [ReadOnly]
    public int avatarId;
    public GameClient gameClient;
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
    public InteractorFacade leftInteractor;
    public InteractorFacade rightInteractor;
    public List<NetworkObject> networkObjects = new List<NetworkObject>();
    private NetworkManager networkObjectManager;
    private readonly List<Player> players = new List<Player>();
    private readonly List<StateMessage> stateBuffer = new List<StateMessage>();
    private StateMessage stateA;
    private StateMessage stateB;
    private float lerpMax = 1 / 60f;
    private float lerpTimer = 0f;
    private float sendRate = 50 / 1000f;
    private float sendTimer = 0f;
    private int stateBufferLength;

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
            if (gameClient.Connected)
            {
                PlayerInput pi = GetPlayerInput();
                gameClient.SendInput(pi);
            }
        }
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
            for (int i = 0; i < stateB.Players.Length; i++)
            {
                Player player = players.Find(x => x.id == stateB.Players[i].Id);
                if (player != null)
                {
                    //player.AddStateToBuffer(stateB.Players[i].Clone());
                }
                else if (stateB.Players[i].Id != avatarId)
                {
                    SpawnPlayer(stateB.Players[i]);
                }
            }
            DespawnOldPlayers(stateB.Players);
            for (int i = 0; i < stateB.Entities.Length; i++)
            {
                NetworkObject obj = networkObjects.Find(x => x.id == stateB.Entities[i].Id);
                if (obj != null)
                {
                    if (!obj.grabbed)
                    {
                        //obj.AddStateToBuffer(stateB.Entities[i].Clone());
                    }
                }
                else
                {
                    LinkOrSpawnLocalObject(stateB.Entities[i]);
                }
            }
            DespawnOldObjects(stateB.Entities);
        }
        if (isLerping)
        {
            for (int i = 0; i < stateB.Players.Length; i++)
            {
                if (stateB.Players[i].Id == avatarId || stateA.Players.Length-1 < i)
                {
                    continue;
                }
                Player player = players.Find(x => x.id == stateB.Players[i].Id);
                player.headGO.transform.position = Vector3.Lerp(stateA.Players[i].HeadPosition, stateB.Players[i].HeadPosition, lerpTimer / lerpMax);
                player.headGO.transform.rotation = Quaternion.Lerp(stateA.Players[i].HeadRotation, stateB.Players[i].HeadRotation, lerpTimer / lerpMax);
                player.leftGO.transform.position = Vector3.Lerp(stateA.Players[i].LeftHandPosition, stateB.Players[i].LeftHandPosition, lerpTimer / lerpMax);
                player.leftGO.transform.rotation = Quaternion.Lerp(stateA.Players[i].LeftHandRotation, stateB.Players[i].LeftHandRotation, lerpTimer / lerpMax);
                player.rightGO.transform.position = Vector3.Lerp(stateA.Players[i].RightHandPosition, stateB.Players[i].RightHandPosition, lerpTimer / lerpMax);
                player.rightGO.transform.rotation = Quaternion.Lerp(stateA.Players[i].RightHandRotation, stateB.Players[i].RightHandRotation, lerpTimer / lerpMax);
            }
            for (int i = 0; i < stateB.Entities.Length; i++)
            {
                NetworkObject obj = networkObjects.Find(x => x.id == stateB.Entities[i].Id);
                if (!obj.grabbed)
                {
                    obj.transform.position = Vector3.Lerp(stateA.Entities[i].Position, stateB.Entities[i].Position, lerpTimer / lerpMax);
                    obj.transform.rotation = Quaternion.Lerp(stateA.Entities[i].Rotation, stateB.Entities[i].Rotation, lerpTimer / lerpMax);
                }
            }
        }
        lerpTimer += Time.fixedDeltaTime;
        if (true)
        //if (lerpTimer >= lerpMax)
        {
            lerpTimer = 0f;
            stateA = stateB;
            stateB = null;
        }
    }

    public void LinkOrSpawnLocalObject(EntityState es)
    {
        NetworkObject localObject = networkObjects.Find((NetworkObject g) => (byte)g.type == es.Type && g.id == 0);
        if (localObject != null)
        {
            localObject.transform.position = es.Position;
            localObject.transform.rotation = es.Rotation;
            localObject.id = es.Id;
            print("linked object " + localObject.gameObject);
        }
        else
        {
            SpawnObject(es);
        }
    }

    public void SpawnObject(EntityState es)
    {
        NetworkObject newObject = Instantiate(networkObjectManager.entityTypesSettings[es.Type].prefab).GetComponent<NetworkObject>();
        newObject.transform.position = es.Position;
        newObject.transform.rotation = es.Rotation;
        newObject.id = es.Id;
        networkObjects.Add(newObject);
        print("spawned object " + newObject.gameObject);
    }

    public void DespawnOldObjects(EntityState[] esArr)
    {
        for (int i = 0; i < networkObjects.Count; i++)
        {
            NetworkObject oldObject = networkObjects[i];
            bool isOld = Array.Find(esArr, x => x.Id == oldObject.id) == null;
            if (isOld)
            {
                networkObjects.Remove(oldObject);
                Destroy(oldObject.gameObject);
            }
        }
    }

    public void SetGrab(NetworkObject obj, bool leftHand)
    {
        int ownId = avatarId;
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
                if (!DEVNetworkSwitcher.isServer)
                {
                    leftGrab.body.isKinematic = true;
                }
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
                rightGrab.body.isKinematic = true;
            }
            rightGrab = obj;
        }
    }

    public void SetShootingLeft(bool shooting)
    {
        leftShooting = shooting;
    }

    public void SetShootingRight(bool shooting)
    {
        rightShooting = shooting;
    }

    public void SetPointerLeft(bool activated)
    {
        leftPointer = activated;
    }

    public void SetPointerRight(bool activated)
    {
        rightPointer = activated;
    }

    /// <summary>
    /// Called when connected to the server
    /// </summary>
    public void OnConnected()
    {

    }
    /// <summary>
    /// Called when disconnected from the server
    /// </summary>
    public void OnDisconnected()
    {

    }
    /// <summary>
    /// Called when received initial data from the server
    /// </summary>
    public void OnInit(InitMessage im)
    {
        avatarId = im.OwnId;
    }
    /// <summary>
    /// Called when received fresh state data from the server
    /// </summary>
    public void OnState(StateMessage sm)
    {
        stateBuffer.Add(sm.Clone());
        //for (int i = 0; i < sm.Players.Length; i++)
        //{
        //    Player player = players.Find(x => x.id == sm.Players[i].Id);
        //    if (player != null)
        //    {
        //        player.AddStateToBuffer(sm.Players[i].Clone());
        //    }
        //    else if (sm.Players[i].Id != avatarId)
        //    {
        //        SpawnPlayer(sm.Players[i]);
        //    }
        //}
        //DespawnOldPlayers(sm.Players);
        //networkObjectManager.ClientSideSync(sm.Entities);
    }

    private void SpawnPlayer(PlayerState ps)
    {
        Player newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
        newPlayer.SetNameOrientationTarget(headGO);
        newPlayer.id = ps.Id;
        newPlayer.headGO.transform.position = ps.HeadPosition;
        newPlayer.headGO.transform.rotation = ps.HeadRotation;
        newPlayer.leftGO.transform.position = ps.LeftHandPosition;
        newPlayer.leftGO.transform.rotation = ps.LeftHandRotation;
        newPlayer.rightGO.transform.position = ps.RightHandPosition;
        newPlayer.rightGO.transform.rotation = ps.RightHandRotation;
        newPlayer.SetLeftPointer(ps.LeftPointer);
        newPlayer.SetRightPointer(ps.RightPointer);
        players.Add(newPlayer);
        Debug.Log("new player connected");
    }

    private void DespawnOldPlayers(PlayerState[] ps)
    {
        for (int i = 0; i < players.Count; i++)
        {
            Player oldPlayer = players[i];
            if (Array.Find(ps, x => x.Id == oldPlayer.id) == null)
            {
                players.Remove(oldPlayer);
                Destroy(oldPlayer.gameObject);
            }
        }
    }

    private PlayerInput GetPlayerInput()
    {
        return new PlayerInput()
        {
            HeadPosition = headGO.transform.position,
            HeadRotation = headGO.transform.rotation,
            LeftHandPosition = leftGO.transform.position,
            LeftHandRotation = leftGO.transform.rotation,
            RightHandPosition = rightGO.transform.position,
            RightHandRotation = rightGO.transform.rotation,
            LeftGrabId = leftGrab == null ? 0 : leftGrab.id,
            LeftGrabPosition = leftGrab == null ? Vector3.zero : leftGrab.transform.position,
            LeftGrabVelocity = leftGrab == null ? Vector3.zero : leftInteractor.VelocityTracker.GetVelocity(),
            LeftGrabRotation = leftGrab == null ? Quaternion.identity : leftGrab.transform.rotation,
            LeftGrabAngularVelocity = leftGrab == null ? Vector3.zero : leftInteractor.VelocityTracker.GetAngularVelocity(),
            RightGrabId = rightGrab == null ? 0 : rightGrab.id,
            RightGrabPosition = rightGrab == null ? Vector3.zero : rightGrab.transform.position,
            RightGrabVelocity = rightGrab == null ? Vector3.zero : rightInteractor.VelocityTracker.GetVelocity(),
            RightGrabRotation = rightGrab == null ? Quaternion.identity : rightGrab.transform.rotation,
            RightGrabAngularVelocity = rightGrab == null ? Vector3.zero : rightInteractor.VelocityTracker.GetAngularVelocity(),
            LeftShooting = leftShooting,
            RightShooting = rightShooting,
            LeftPointer = leftPointer,
            RightPointer = rightPointer,
        };
    }
}
