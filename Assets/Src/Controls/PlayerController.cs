using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.NetworkVariable;

public class PlayerController : NetworkBehaviour 
{
    NetworkVariableVector3 moveDir = new NetworkVariableVector3(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly
    });
    
    NetworkVariableVector3 navTarget = new NetworkVariableVector3(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly
    });
    
    NetworkVariableULong playerObjNetId = new NetworkVariableULong(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.ServerOnly,
        SendTickrate = 0
    });

    float commandDurationThreshold = 0.1f;
    float timeSinceLastCommand = 0f;
    bool isStale = false;

    // Start is called before the first frame update
    void Start()
    {
        if(IsServer) {
            moveDir.OnValueChanged += (Vector3 prevMoveDir, Vector3 newMoveDir)=>{
                timeSinceLastCommand = 0;
            };
            
            //tell spawner to spawn
            GameObject spawnedPlayerGO = PlayerSpawner.Instance.SpawnPlayer(GetComponent<NetworkObject>().OwnerClientId);            
            playerObjNetId.Value = spawnedPlayerGO.GetComponent<NetworkObject>().NetworkObjectId;
        }        
    }
    public override void NetworkStart()
    {
        base.NetworkStart();
        Debug.Log(playerObjNetId.Value);
    }

    void Awake() {
        if(IsLocalPlayer) {
            playerObjNetId.OnValueChanged += (ulong oldId, ulong newId) => {                
                Debug.Log("value set!!");

                NetworkObject playerNetworkObj = NetworkSpawnManager.SpawnedObjects[newId];
                Camera.main.transform.SetParent(playerNetworkObj.transform);
            };
        }
    }

    // Update is called once per frame
    void Update()
    {   
        Debug.Log(playerObjNetId.Value);

        if(IsLocalPlayer) {
            if(Input.GetKey(KeyCode.LeftArrow)) {
                moveDir.Value = new Vector3(-1, 0, 0);
            }
            else if(Input.GetKey(KeyCode.RightArrow)) {
                moveDir.Value = new Vector3(1, 0, 0);
            }
            else {
                moveDir.Value = new Vector3(0, 0, 0);
            }

            if(Input.GetMouseButtonDown(1)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit)) {                    
                    // GameObject go = hit.transform.gameObject;
                    navTarget.Value = hit.point;
                }
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
                // Player player = playerObj.GetComponent<Player>();
                // player.moveDir = GetMoveDir();
                NavMeshAgent navMeshAgent = playerObj.GetComponent<NavMeshAgent>();
                navMeshAgent.SetDestination(navTarget.Value);
            }
        }
    }

    public Vector3 GetMoveDir() {
        return moveDir.Value;
        // return isStale ? new Vector3(0,0,0) : moveDir.Value;
    }

}
