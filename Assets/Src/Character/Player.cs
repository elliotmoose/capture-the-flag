using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Player : NetworkBehaviour
{
    // Start is called before the first frame update    
    public Vector3 moveDir = Vector3.zero;
    float moveSpeed = 5;

    public NetworkVariableULong ownerClientId = new NetworkVariableULong(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    // Player() : base() {
    //     Debug.Log("player constructed");
    //     ownerClientId.OnValueChanged += (ulong oldId, ulong newId) => {
    //         this.name = "player"+newId.ToString();
    //         Debug.Log("onvaluechanged");
            
    //         //set camera
    //         if(newId == NetworkManager.Singleton.LocalClientId) {
    //             Camera.main.transform.SetParent(this.transform);
    //         }
    //     };

    //     Debug.Log("event subscribed");
    // }

    public override void NetworkStart() {
        Debug.Log(this.NetworkObjectId);
        if(!IsServer) {
            GetComponent<NavMeshAgent>().enabled = false;            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(IsServer) {
            // Debug.Log(NetworkManager.Singleton.LocalClientId);
            transform.position += moveDir * Time.deltaTime * moveSpeed;
        }
    }
}
