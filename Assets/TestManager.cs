using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class TestManager : MonoBehaviour
{
    public bool autoStartHost = true;
    // Start is called before the first frame update
    void Start()
    {
        if(autoStartHost) {
            GetComponent<NetworkManager>().StartHost();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
