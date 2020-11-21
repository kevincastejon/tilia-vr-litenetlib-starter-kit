using System.Collections.Generic;
using Tilia.Indicators.ObjectPointers;
using Tilia.Interactions.Interactables.Interactables;
using Tilia.Interactions.Interactables.Interactors;
using Unity.Collections;
using UnityEngine;

public class GameManagerServer : MonoBehaviour
{
    [ReadOnly]
    public bool leftShooting;
    [ReadOnly]
    public bool rightShooting;
    public GameServer server;
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    public GameObject headGO;
    public GameObject leftGO;
    public GameObject rightGO;
    public List<NetworkGrabbableObject> guns = new List<NetworkGrabbableObject>();
    [ReadOnly]
    public NetworkGrabbableObject leftGrab;
    [ReadOnly]
    public NetworkGrabbableObject rightGrab;
    [ReadOnly]
    public bool leftPointer;
    [ReadOnly]
    public bool rightPointer;
    public int maxBullets = 12;
    private readonly List<Player> players = new List<Player>();
    private readonly List<NetworkObject> bullets = new List<NetworkObject>();
    private float sendRate = 50 / 1000f;
    private float sendTimer = 0f;
    private int serverId = -1;

    private void Start()
    {
        guns.ForEach((NetworkGrabbableObject gun) =>
        {
            InteractableFacade interactable = gun.GetComponent<InteractableFacade>();
            interactable.Grabbed.AddListener((InteractorFacade interactor) => SetGrab(gun, interactor.name == "LeftInteractor"));
            interactable.Ungrabbed.AddListener((InteractorFacade interactor) => SetGrab(null, interactor.name == "LeftInteractor"));
        });
    }

    private void Update()
    {
        sendTimer += Time.deltaTime;
        if (sendTimer >= sendRate)
        {
            StateMessage sm = GetWorldState();
            server.SendWorldState(sm);
        }
    }

    public void SetGrab(NetworkGrabbableObject obj, bool leftHand)
    {
        if (leftHand)
        {
            if (obj != null)
            {
                obj.grabbed = true;
                obj.leftHand = true;
                obj.lastOwnerId = serverId;
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
                obj.lastOwnerId = serverId;
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
        if (bullets.Count == maxBullets)
        {
            NetworkObject oldBullet = bullets[0];
            bullets.RemoveAt(0);
            Destroy(oldBullet.gameObject);
        }
        NetworkObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<NetworkObject>();
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 1, ForceMode.Impulse);
        bullets.Add(bullet);
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
        Destroy(disconnectedPlayer);
    }

    public void OnClientInput(int peerID, PlayerInput pi)
    {
        //Debug.Log("AvatarState : " + peerID);
        Player player = players.Find(x => x.GetComponent<Player>().id == peerID).GetComponent<Player>();
        player.SetHeadPositionTarget(pi.HeadPosition);
        player.SetHeadRotationTarget(pi.HeadRotation);
        player.SetLeftHandPositionTarget(pi.LeftHandPosition);
        player.SetLeftHandRotationTarget(pi.LeftHandRotation);
        player.SetRightHandPositionTarget(pi.RightHandPosition);
        player.SetRightHandRotationTarget(pi.RightHandRotation);
        if (player.leftGrabId != 0 && pi.LeftGrabId == 0)
        {
            NetworkGrabbableObject ungrabbed = guns.Find((NetworkGrabbableObject g) => g.id == pi.LeftGrabId);
            ungrabbed.rigidBody.isKinematic = false;
        }
        if (player.rightGrabId != 0 && pi.RightGrabId == 0)
        {
            NetworkGrabbableObject ungrabbed = guns.Find((NetworkGrabbableObject g) => g.id == pi.RightGrabId);
            ungrabbed.rigidBody.isKinematic = false;
        }
        player.leftGrabId = pi.LeftGrabId;
        player.rightGrabId = pi.RightGrabId;
        if (pi.LeftGrabId != 0)
        {
            NetworkGrabbableObject grabbed = guns.Find((NetworkGrabbableObject g) => g.id == pi.LeftGrabId);
            grabbed.rigidBody.isKinematic = true;
            grabbed.SetPositionTarget(pi.LeftGrabPosition);
            grabbed.SetRotationTarget(pi.LeftGrabRotation);
        }
        if (pi.RightGrabId != 0)
        {
            NetworkGrabbableObject grabbed = guns.Find((NetworkGrabbableObject g) => g.id == pi.RightGrabId);
            grabbed.rigidBody.isKinematic = true;
            grabbed.SetPositionTarget(pi.RightGrabPosition);
            grabbed.SetRotationTarget(pi.RightGrabRotation);
        }
        player.SetLeftPointer(pi.LeftPointer);
        player.SetRightPointer(pi.RightPointer);
        if (!player.leftShooting && pi.LeftShooting && player.leftGrabId != 0)
        {
            NetworkGrabbableObject obj = guns.Find((NetworkGrabbableObject g) => g.id == player.leftGrabId);
            Transform spawnPoint = obj.GetComponent<Gun>().spawnPoint.transform;
            SpawnBullet(spawnPoint);
        }
            player.leftShooting = pi.LeftShooting;
        if (!player.rightShooting && pi.RightShooting && player.rightGrabId != 0)
        {
            NetworkGrabbableObject obj = guns.Find((NetworkGrabbableObject g) => g.id == player.rightGrabId);
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
        EntityState[] bulletStates = new EntityState[bullets.Count];
        for (int i = 0; i < bullets.Count; i++)
        {
            NetworkObject b = bullets[i];
            bulletStates[i] = new EntityState()
            {
                Id = b.id,
                Position = b.transform.position,
                Rotation = b.transform.rotation,
            };
        }
        EntityState[] gunStates = new EntityState[guns.Count];
        for (int i = 0; i < guns.Count; i++)
        {
            NetworkGrabbableObject g = guns[i];
            Vector3 position = g.transform.position;
            Quaternion rotation = g.transform.rotation;
            gunStates[i] = new EntityState()
            {
                Id = g.id,
                Position = position,
                Rotation = rotation,
            };
        }
        StateMessage sm = new StateMessage()
        {
            Players = playerStates,
            Bullets = bulletStates,
            Guns = gunStates
            //Players = new PlayerState[0],
            //Bullets = new EntityState[0],
            //Guns = new EntityState[0]
        };
        return sm;
    }
}
