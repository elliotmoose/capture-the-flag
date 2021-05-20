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
    float moveSpeed = 10;
    float sprintMultiplier = 2.4f;
    public bool sprinting = false;

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

    // Update is called once per frame
    void Update()
    {
        if(IsServer) {
            // Debug.Log(NetworkManager.Singleton.LocalClientId);
            transform.position += moveDir * Time.deltaTime * moveSpeed * (sprinting ? sprintMultiplier : 1);
        }

        if(moveDir.magnitude != 0) {
            GetComponent<Animator>().SetBool("IsForward", true);
        }
        else {
            GetComponent<Animator>().SetBool("IsForward", false);
        }
    }

    void LateUpdate() {
        if(NetworkManager.LocalClientId == ownerClientId.Value) {
            transform.rotation = Quaternion.Euler(transform.rotation.x, Camera.main.transform.eulerAngles.y, transform.rotation.z);
        }
    }
}
