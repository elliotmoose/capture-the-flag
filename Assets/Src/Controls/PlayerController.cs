using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using MLAPI.NetworkVariable;
using Cinemachine;
public enum ControlType {
    ThirdPerson,
    TopDown
}

public class PlayerController : NetworkBehaviour 
{   
    NetworkVariableVector3 moveDir = new NetworkVariableVector3(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly,
        SendTickrate = 20,
    });
    
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
            switch (controlType)
            {
                case ControlType.ThirdPerson:
                    float horizontal = Input.GetAxisRaw("Horizontal");
                    float vertical = Input.GetAxisRaw("Vertical");
                    
                    Vector3 _moveDir = new Vector3(horizontal, 0, vertical).normalized;
                    if(_moveDir.magnitude != 0) {
                        float targetAngle = Mathf.Atan2(_moveDir.x, _moveDir.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
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
