using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using MLAPI.Spawning;
using MLAPI;

public class MinimapCameraFollower : MonoBehaviour
{
    public static MinimapCameraFollower Instance;
    GameObject target;

    void Awake() {
        Instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        AttachToPlayerIfNeeded();
    }

    void AttachToPlayerIfNeeded() {
        if(!NetworkManager.Singleton.IsClient) return;
        if(target) return;

        NetworkObject localPlayerNetObj = NetworkSpawnManager.GetLocalPlayerObject();
        if(!localPlayerNetObj) return;
        
        GameObject localPlayerObject = localPlayerNetObj.gameObject; //error here refers to usercontroller that is not yet despawning
        PlayerController playerController = localPlayerObject.GetComponent<PlayerController>();
        if(!playerController) return;

        LocalPlayer localPlayer = playerController.GetPlayer();
        if(!localPlayer) return;
        target = localPlayer.gameObject;        
        GameObject camera = GameObject.FindGameObjectWithTag("MinimapCam");
        camera.transform.parent = target.transform;
        camera.transform.localPosition = new Vector3(0, 50, 0);
        camera.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
    }
}


