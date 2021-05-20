using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using MLAPI.NetworkVariable;
using Cinemachine;
using UnityEngine.InputSystem;

public enum ControlType {
    ThirdPerson,
    TopDown
}

public class PlayerController : NetworkBehaviour 
{
    public InputControls controls;

    //third person
    NetworkVariableVector3 moveDir = new NetworkVariableVector3(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly,
        SendTickrate = 20,
    });
    
    NetworkVariableBool sprinting = new NetworkVariableBool(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly,
    });
    

    //third top down
    NetworkVariableVector3 navTarget = new NetworkVariableVector3(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly
    });
    
    public NetworkVariableULong playerObjNetId = new NetworkVariableULong(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.ServerOnly,
        SendTickrate = 0,        
    });


    public ControlType controlType = ControlType.ThirdPerson;

    float commandDurationThreshold = 0.1f;
    float timeSinceLastCommand = 0f;
    bool isStale = false;

    // Start is called before the first frame update
    public override void NetworkStart()
    {
        base.NetworkStart();        
        // if(IsLocalPlayer) {
        //     Camera.main.GetComponent<CameraFollower>().controlType = controlType;
        // }
    }

    void Awake() {
        controls = new InputControls();
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

        if(IsLocalPlayer) {
            Cursor.visible = false;

            controls.Enable();
            controls.Player.Catch.performed += ctx => {
                Debug.Log("Catch");
            };

            
        }        
    }

    // Update is called once per frame
    void Update()
    {   
        if(IsLocalPlayer) {
            Debug.Log(controlType);
            switch (controlType)
            {
                case ControlType.ThirdPerson:
                    Vector2 _moveDir = controls.Player.Move.ReadValue<Vector2>().normalized;
                    if(_moveDir.magnitude != 0) {
                        float targetAngle = Mathf.Atan2(_moveDir.x, _moveDir.y) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
                        moveDir.Value  = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
                    }
                    else {
                        moveDir.Value = Vector3.zero;
                    }
                    break;
                case ControlType.TopDown:
                    if(Input.GetMouseButton(1)) {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if(Physics.Raycast(ray, out hit)) {                    
                            // GameObject go = hit.transform.gameObject;
                            navTarget.Value = hit.point;
                        }
                    }
                    break;
            }
        }

        if(IsServer) {
            timeSinceLastCommand += Time.deltaTime;
            isStale = (timeSinceLastCommand > commandDurationThreshold); //if command hasn't come in, we mark as stale

            //server should move player
            // GameObject playerObj = GameObject.Find("player"+GetComponent<NetworkObject>().OwnerClientId.ToString());
            NetworkObject playerNetworkObj = NetworkSpawnManager.SpawnedObjects[playerObjNetId.Value];
            GameObject playerObj = playerNetworkObj.gameObject;

            // Debug.Log(navTarget.Value);
            if(playerObj) {

                switch (controlType)
                {
                    case ControlType.ThirdPerson:
                        Player player = playerObj.GetComponent<Player>();
                        player.moveDir = GetMoveDir();
                        break;
                    case ControlType.TopDown:
                        NavMeshAgent navMeshAgent = playerObj.GetComponent<NavMeshAgent>();
                        navMeshAgent.enabled = true;
                        navMeshAgent.SetDestination(navTarget.Value);
                        break;
                }
            }
        }
    }

    public Vector3 GetMoveDir() {
        return moveDir.Value;
        // return isStale ? new Vector3(0,0,0) : moveDir.Value;
    }

}
