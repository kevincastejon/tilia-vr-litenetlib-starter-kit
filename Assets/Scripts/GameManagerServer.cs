using System.Collections.Generic;
using Tilia.Indicators.ObjectPointers;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using Unity.Collections;
using UnityEngine;

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
    private NetworkManager networkObjectManager;
    private readonly List<Player> players = new List<Player>();
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
            StateMessage sm = GetWorldState();
            server.SendWorldState(sm);
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
    }

    public void OnClientConnected(int peerID)
    {
        Player newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
        newPlayer.SetNameOrientationTarget(headGO);
        newPlayer.GetComponent<Player>().id = peerID;
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
        int leftUngrabbedId = disconnectedPlayer.leftGrabId != 0 ? disconnectedPlayer.leftGrabId : 0;
        int rightUngrabbedId = disconnectedPlayer.rightGrabId != 0 ? disconnectedPlayer.rightGrabId : 0;
        networkObjectManager.ServerSideSyncClientUngrabbed(leftUngrabbedId, rightUngrabbedId);
        Destroy(disconnectedPlayer.gameObject);
    }

    public void OnClientInput(int peerID, PlayerInput pi)
    {
        Debug.Log("AvatarState : " + pi.LeftHandPosition);
        Player player = players.Find(x => x.GetComponent<Player>().id == peerID).GetComponent<Player>();
        Vector3 hp = pi.HeadPosition;
        Quaternion hr = pi.HeadRotation;
        Vector3 lp = pi.LeftHandPosition;
        Quaternion lr = pi.LeftHandRotation;
        Vector3 rp = pi.RightHandPosition;
        Quaternion rr = pi.RightHandRotation;
        player.AddStateToBuffer(new PlayerState()
        {
            HeadPosition = new Vector3(hp.x,hp.y,hp.z),
            HeadRotation = new Quaternion(hr.x,hr.y,hr.z,hr.w),
            LeftHandPosition = new Vector3(lp.x,lp.y,lp.z),
            LeftHandRotation = new Quaternion(lr.x, lr.y, lr.z, lr.w),
            RightHandPosition = new Vector3(rp.x,rp.y,rp.z),
            RightHandRotation = new Quaternion(rr.x, rr.y, rr.z, rr.w),
            LeftPointer = pi.LeftPointer,
            RightPointer = pi.RightPointer
        });
        player.SetLeftPointer(pi.LeftPointer);
        player.SetRightPointer(pi.RightPointer);
        int leftUngrabbedId = player.leftGrabId != 0 && pi.LeftGrabId == 0 ? player.leftGrabId : 0;
        int rightUngrabbedId = player.rightGrabId != 0 && pi.RightGrabId == 0 ? player.rightGrabId : 0;
        player.leftGrabId = pi.LeftGrabId;
        player.rightGrabId = pi.RightGrabId;
        networkObjectManager.ServerSideSyncClientUngrabbed(leftUngrabbedId, rightUngrabbedId);
        networkObjectManager.ServerSideSyncClientGrabbing(peerID, pi);
        if (!player.leftShooting && pi.LeftShooting && player.leftGrabId != 0)
        {
            NetworkObject obj = networkObjectManager.GetObject(player.leftGrabId);
            Transform spawnPoint = obj.GetComponent<Gun>().spawnPoint.transform;
            SpawnBullet(spawnPoint);
        }
        player.leftShooting = pi.LeftShooting;
        if (!player.rightShooting && pi.RightShooting && player.rightGrabId != 0)
        {
            NetworkObject obj = networkObjectManager.GetObject(player.rightGrabId);
            Transform spawnPoint = obj.GetComponent<Gun>().spawnPoint.transform;
            SpawnBullet(spawnPoint);
        }
        player.rightShooting = pi.RightShooting;
    }

    private StateMessage GetWorldState()
    {
        PlayerState[] playerStates = new PlayerState[players.Count + 1];
        for (int i = 0; i < players.Count; i++)
        {
            Player p = players[i];
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

        playerStates[players.Count] = new PlayerState()
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
        EntityState[] entityStates = networkObjectManager.GetEntityStates();
        StateMessage sm = new StateMessage()
        {
            Players = playerStates,
            Entities = entityStates,
        };
        return sm;
    }
}
