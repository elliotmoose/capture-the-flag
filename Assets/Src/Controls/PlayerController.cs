using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using MLAPI.NetworkVariable;
using Cinemachine;
public class PlayerController : NetworkBehaviour 
{   
    NetworkVariableBool sprinting = new NetworkVariableBool(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly
    });

    NetworkVariableVector3 moveDir = new NetworkVariableVector3(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly,
        SendTickrate = 20,
    });

    NetworkVariableQuaternion faceDir = new NetworkVariableQuaternion(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly,
        SendTickrate = 20,
    });
    
    public NetworkVariableULong playerObjNetId = new NetworkVariableULong(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.ServerOnly,
        SendTickrate = 0,        
    });

    float commandDurationThreshold = 0.1f;
    float timeSinceLastCommand = 0f;
    bool isStale = false;

    public KeyCode SKILL1_KEY = KeyCode.Q;
    public KeyCode SKILL2_KEY = KeyCode.E;
    public KeyCode SPRINT_KEY = KeyCode.Space;
    // Start is called before the first frame update
    public override void NetworkStart()
    {
        base.NetworkStart();        
        // if(IsLocalPlayer) {
        //     Camera.main.GetComponent<CameraFollower>().controlType = controlType;
        // }
    }
    
    void Start() {
        if(IsServer) {
            moveDir.OnValueChanged += (Vector3 prevMoveDir, Vector3 newMoveDir)=>{
                timeSinceLastCommand = 0;
            };

            //tell spawner to spawn
            GameObject spawnedPlayerGO = PlayerSpawner.Instance.SpawnPlayer(GetComponent<NetworkObject>().OwnerClientId);            
            playerObjNetId.Value = spawnedPlayerGO.GetComponent<NetworkObject>().NetworkObjectId;
            Debug.Log("server spawn id:" + playerObjNetId.Value.ToString());
        }        

        Cursor.visible = false;
       
    }

    // Update is called once per frame
    void Update()
    {   
        if(IsLocalPlayer) {
            Debug.Log("Checking for controls");
            ClientControls();
        }

        if(IsServer) {
            timeSinceLastCommand += Time.deltaTime;
            isStale = (timeSinceLastCommand > commandDurationThreshold); //if command hasn't come in, we mark as stale

            //server should move player
            // GameObject playerObj = GameObject.Find("player"+GetComponent<NetworkObject>().OwnerClientId.ToString());
            NetworkObject playerNetworkObj = NetworkSpawnManager.SpawnedObjects[playerObjNetId.Value];
            GameObject playerObj = playerNetworkObj.gameObject;
            if(playerObj) {
                Player player = playerObj.GetComponent<Player>();
                player.moveDir = moveDir.Value;
                player.sprinting = sprinting.Value;
                Debug.Log(sprinting.Value);
            }
        }
    }

    void ClientControls() {
        //movement
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 _moveDir = new Vector3(horizontal, 0, vertical).normalized;
        if(_moveDir.magnitude != 0) {
            float targetAngle = Mathf.Atan2(_moveDir.x, _moveDir.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            faceDir.Value = Quaternion.Euler(0, targetAngle, 0); 
            moveDir.Value = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
        }
        else {
            moveDir.Value = Vector3.zero;
        }

        if(Input.GetMouseButtonDown(0)) {
            CatchServerRpc();
        }
        if(Input.GetKeyDown(SKILL1_KEY)) {
            Skill1ServerRpc();
        }
        if(Input.GetKeyDown(SKILL2_KEY)) {
            Skill2ServerRpc();
        }
        
        sprinting.Value = Input.GetKey(SPRINT_KEY);
    }

    [ServerRpc]
    void CatchServerRpc() {
        if(IsServer) {
            Debug.Log("Catch!");
        }
    }

    [ServerRpc]
    void Skill1ServerRpc() {
        if(IsServer) {
            Debug.Log("Skill 1!");
        }
    }

    [ServerRpc]
    void Skill2ServerRpc() {
        if(IsServer) {
            Debug.Log("Skill 2!");
        }
    }
}
