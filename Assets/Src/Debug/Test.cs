using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class Test : NetworkBehaviour
{
    // Update is called once per frame
    void FixedUpdate()
    {
        if(!IsLocalPlayer) {
            return;
        }

        // this.GetComponent<Rigidbody>().velocity = Input.GetKey(KeyCode.Space) ? Vector3.forward*10 : Vector3.zero;
        if(Input.GetKey(KeyCode.LeftArrow))    {
            // this.GetComponent<CharacterController>().SimpleMove(Vector3.forward*Time.deltaTime*20);
            this.GetComponent<CharacterController>().Move(-Vector3.forward*Time.deltaTime*20);
            // this.transform.Translate(-Vector3.forward*Time.fixedDeltaTime*20);
        }
        else if(Input.GetKey(KeyCode.RightArrow))    {
            // this.GetComponent<CharacterController>().SimpleMove(Vector3.forward*Time.deltaTime*20);
            this.GetComponent<CharacterController>().Move(Vector3.forward*Time.deltaTime*20);
            // this.transform.Translate(Vector3.forward*Time.fixedDeltaTime*20);
        }
    }

    void Start() {
        for (int i = 0; i < 20; i++)
        {
            TestClientRpc(i);
        }
    }

    [ClientRpc]
    void TestClientRpc(int i) {
        Debug.Log(i);
    }
}
