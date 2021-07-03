using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Rigidbody>().velocity = Input.GetKey(KeyCode.Space) ? Vector3.forward : Vector3.zero;
        if(Input.GetKey(KeyCode.Space))    {
            // this.GetComponent<CharacterController>().SimpleMove(Vector3.forward*Time.deltaTime*20);
            // this.GetComponent<CharacterController>().Move(Vector3.forward*Time.deltaTime*20);
            // this.transform.Translate(Vector3.forward*Time.deltaTime*20);
        }
    }
}
