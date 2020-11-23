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
    public List<NetworkGrabbableObject> linearLevers = new List<NetworkGrabbableObject>();
    public List<NetworkGrabbableObject> pins = new List<NetworkGrabbableObject>();
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
    private readonly List<NetworkGrabbableObject> bullets = new List<NetworkGrabbableObject>();
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
        linearLevers.ForEach((NetworkGrabbableObject lever) =>
        {
            InteractableFacade interactable = lever.GetComponent<InteractableFacade>();
            interactable.Grabbed.AddListener((InteractorFacade interactor) => SetGrab(lever, interactor.name == "LeftInteractor"));
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
            NetworkGrabbableObject oldBullet = bullets[0];
            bullets.RemoveAt(0);
            Destroy(oldBullet.gameObject);
        }
        NetworkGrabbableObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<NetworkGrabbableObject>();
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
        if (disconnectedPlayer.leftGrabId != 0)
        {
            NetworkGrabbableObject ungrabbed = guns.Find((NetworkGrabbableObject g) => g.id == disconnectedPlayer.leftGrabId);
            ungrabbed.grabbed = false;
            ungrabbed.leftHand = false;
            ungrabbed.rigidBody.isKinematic = false;
            ungrabbed.rigidBody.velocity = ungrabbed.bufferVelocity;
            ungrabbed.bufferVelocity = Vector3.zero;
            ungrabbed.rigidBody.angularVelocity = ungrabbed.bufferAngularVelocity;
            ungrabbed.bufferAngularVelocity = Vector3.zero;
        }
        if (disconnectedPlayer.rightGrabId != 0)
        {
            NetworkGrabbableObject ungrabbed = guns.Find((NetworkGrabbableObject g) => g.id == disconnectedPlayer.rightGrabId);
            ungrabbed.grabbed = false;
            ungrabbed.leftHand = false;
            ungrabbed.rigidBody.isKinematic = false;
            ungrabbed.rigidBody.velocity = ungrabbed.bufferVelocity;
            ungrabbed.bufferVelocity = Vector3.zero;
            ungrabbed.rigidBody.angularVelocity = ungrabbed.bufferAngularVelocity;
            ungrabbed.bufferAngularVelocity = Vector3.zero;
        }
        Destroy(disconnectedPlayer.gameObject);
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
        player.SetLeftPointer(pi.LeftPointer);
        player.SetRightPointer(pi.RightPointer);
        if (player.leftGrabId != 0 && pi.LeftGrabId == 0)
        {
            NetworkGrabbableObject ungrabbed = guns.Find((NetworkGrabbableObject g) => g.id == player.leftGrabId);
            if (ungrabbed==null)
            {
                ungrabbed = linearLevers.Find((NetworkGrabbableObject g) => g.id == player.leftGrabId);
            }
            ungrabbed.grabbed = false;
            ungrabbed.leftHand = false;
            ungrabbed.rigidBody.isKinematic = ungrabbed.kinematicInitValue;
            ungrabbed.rigidBody.velocity = ungrabbed.bufferVelocity;
            ungrabbed.bufferVelocity = Vector3.zero;
            ungrabbed.rigidBody.angularVelocity = ungrabbed.bufferAngularVelocity;
            ungrabbed.bufferAngularVelocity = Vector3.zero;
        }
        if (player.rightGrabId != 0 && pi.RightGrabId == 0)
        {
            NetworkGrabbableObject ungrabbed = guns.Find((NetworkGrabbableObject g) => g.id == player.rightGrabId);
            if (ungrabbed == null)
            {
                ungrabbed = linearLevers.Find((NetworkGrabbableObject g) => g.id == player.rightGrabId);
            }
            ungrabbed.grabbed = false;
            ungrabbed.leftHand = false;
            ungrabbed.rigidBody.isKinematic = ungrabbed.kinematicInitValue;
            ungrabbed.rigidBody.velocity = ungrabbed.bufferVelocity;
            ungrabbed.bufferVelocity = Vector3.zero;
            ungrabbed.rigidBody.angularVelocity = ungrabbed.bufferAngularVelocity;
            ungrabbed.bufferAngularVelocity = Vector3.zero;
        }
        player.leftGrabId = pi.LeftGrabId;
        player.rightGrabId = pi.RightGrabId;
        if (pi.LeftGrabId != 0)
        {
            NetworkGrabbableObject grabbed = guns.Find((NetworkGrabbableObject g) => g.id == pi.LeftGrabId);
            if (grabbed == null)
            {
                grabbed = linearLevers.Find((NetworkGrabbableObject g) => g.id == pi.LeftGrabId);
            }
            if (grabbed.snapContainer != null)
            {
                grabbed.snapContainer.Unsnap();
                grabbed.snapContainer = null;
            }
            grabbed.grabbed = true;
            grabbed.leftHand = true;
            grabbed.lastOwnerId = peerID;
            grabbed.rigidBody.isKinematic = true;
            grabbed.bufferVelocity = pi.LeftGrabVelocity;
            grabbed.bufferAngularVelocity = pi.LeftGrabAngularVelocity;
            grabbed.SetPositionTarget(pi.LeftGrabPosition);
            grabbed.SetRotationTarget(pi.LeftGrabRotation);
        }
        if (pi.RightGrabId != 0)
        {
            NetworkGrabbableObject grabbed = guns.Find((NetworkGrabbableObject g) => g.id == pi.RightGrabId);
            if (grabbed == null)
            {
                grabbed = linearLevers.Find((NetworkGrabbableObject g) => g.id == pi.RightGrabId);
            }
            if (grabbed.snapContainer != null)
            {
                grabbed.snapContainer.Unsnap();
                grabbed.snapContainer = null;
            }
            grabbed.grabbed = true;
            grabbed.leftHand = false;
            grabbed.lastOwnerId = peerID;
            grabbed.rigidBody.isKinematic = true;
            grabbed.bufferVelocity = pi.RightGrabVelocity;
            grabbed.bufferAngularVelocity = pi.RightGrabAngularVelocity;
            grabbed.SetPositionTarget(pi.RightGrabPosition);
            grabbed.SetRotationTarget(pi.RightGrabRotation);
        }
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
                HeadRotation = p.headGO.transform.rotation.eulerAngles,
                LeftHandPosition = p.leftGO.transform.position,
                LeftHandRotation = p.leftGO.transform.rotation.eulerAngles,
                RightHandPosition = p.rightGO.transform.position,
                RightHandRotation = p.rightGO.transform.rotation.eulerAngles,
                LeftPointer = p.leftPointerActivated,
                RightPointer = p.rightPointerActivated
            };
        }

        playerStates[players.Count] = new PlayerState()
        {
            Id = serverId,
            HeadPosition = headGO.transform.position,
            HeadRotation = headGO.transform.rotation.eulerAngles,
            LeftHandPosition = leftGO.transform.position,
            LeftHandRotation = leftGO.transform.rotation.eulerAngles,
            RightHandPosition = rightGO.transform.position,
            RightHandRotation = rightGO.transform.rotation.eulerAngles,
            LeftPointer = leftPointer,
            RightPointer = rightPointer,
        };
        EntityState[] bulletStates = new EntityState[bullets.Count];
        for (int i = 0; i < bullets.Count; i++)
        {
            NetworkGrabbableObject b = bullets[i];
            bulletStates[i] = new EntityState()
            {
                Id = b.id,
                Type = (byte)EntityType.Bullet,
                Position = b.transform.position,
                Rotation = b.transform.rotation.eulerAngles,
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
                Type = (byte)EntityType.Gun,
                Position = position,
                Rotation = rotation.eulerAngles,
            };
        }
        EntityState[] pinStates = new EntityState[pins.Count];
        for (int i = 0; i < pins.Count; i++)
        {
            NetworkGrabbableObject g = pins[i];
            Vector3 position = g.transform.position;
            Quaternion rotation = g.transform.rotation;
            pinStates[i] = new EntityState()
            {
                Id = g.id,
                Type = (byte)EntityType.Pin,
                Position = position,
                Rotation = rotation.eulerAngles,
            };
        }
        EntityState[] linearLeverStates = new EntityState[linearLevers.Count];
        for (int i = 0; i < linearLevers.Count; i++)
        {
            NetworkGrabbableObject g = linearLevers[i];
            Vector3 position = g.transform.position;
            Quaternion rotation = g.transform.rotation;
            linearLeverStates[i] = new EntityState()
            {
                Id = g.id,
                Type = (byte)EntityType.LinearLever,
                Position = position,
                Rotation = rotation.eulerAngles,
            };
        }
        StateMessage sm = new StateMessage()
        {
            Players = playerStates,
            Bullets = bulletStates,
            Guns = gunStates,
            Pins = pinStates,
            LinearLevers = linearLeverStates,
            //Players = new PlayerState[0],
            //Bullets = new EntityState[0],
            //Guns = new EntityState[0]
        };
        return sm;
    }
}
