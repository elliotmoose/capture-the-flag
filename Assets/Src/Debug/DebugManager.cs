using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class DebugManager : NetworkBehaviour
{
    public bool isHost;
    // Start is called before the first frame update
    void Start()
    {
        if(isHost) {
            NetworkManager.Singleton.StartHost();
        }
        else {
            NetworkManager.Singleton.StartClient();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
