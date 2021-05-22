using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.Spawning;
using Cinemachine;

public class CameraFollower : MonoBehaviour
{
    GameObject target;

    // Update is called once per frame
    void Update()
    {
        AttachToPlayerIfNeeded();
    }

    void AttachToPlayerIfNeeded() {
        if(!target) { 
            GameObject localPlayerObject = NetworkSpawnManager.GetLocalPlayerObject()?.gameObject;
            PlayerController playerController = localPlayerObject?.GetComponent<PlayerController>();

            if(playerController) {
                ulong playerGameObjectNetId = playerController.playerObjNetId.Value;
                if(playerGameObjectNetId != 0) {
                    CinemachineFreeLook camera = GameObject.FindGameObjectWithTag("CinemachineCamera").GetComponent<CinemachineFreeLook>();
                    target = NetworkSpawnManager.SpawnedObjects[playerGameObjectNetId].gameObject;
                    camera.LookAt = target.transform;
                    camera.Follow = target.transform;
                }
            }
        }
    }
}
