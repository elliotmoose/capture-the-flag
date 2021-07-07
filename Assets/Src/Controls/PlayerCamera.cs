using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.Spawning;
using MLAPI;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Instance;
    public GameObject sprintVfx;
    GameObject target;

    void Awake() {
        Instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        AttachToPlayerIfNeeded();
        UpdateVFX();
    }

    void AttachToPlayerIfNeeded() {
        if(!NetworkManager.Singleton.IsClient) return;
        if(target) return;

        NetworkObject localPlayerNetObj = NetworkSpawnManager.GetLocalPlayerObject();
        if(!localPlayerNetObj) return;
        
        GameObject localPlayerObject = localPlayerNetObj.gameObject; //error here refers to usercontroller that is not yet despawning
        PlayerController playerController = localPlayerObject.GetComponent<PlayerController>();
        if(!playerController) return;

        CinemachineVirtualCamera camera = GameObject.FindGameObjectWithTag("CinemachineCamera").GetComponent<CinemachineVirtualCamera>(); 
        // target = NetworkSpawnManager.SpawnedObjects[playerGameObjectNetId].gameObject;
        LocalPlayer localPlayer = playerController.GetPlayer();
        if(!localPlayer) return;
        target = localPlayer.gameObject;
        camera.LookAt = target.transform;
        camera.Follow = target.transform;
    }

    void UpdateVFX() {
        // PlayerController playerController = PlayerController.LocalInstance;
        // if(!playerController) return;
        // sprintVfx.SetActive(playerController.GetSprinting());        

        if(!target) return;
        Animator animator = target.GetComponent<Animator>();
        if(!animator) return;
        float hor = animator.GetFloat("HorMovement");
        float vert = animator.GetFloat("VertMovement");
        bool isSprinting = (hor == 1 || vert == 1);
        sprintVfx.SetActive(isSprinting);
    }
}
