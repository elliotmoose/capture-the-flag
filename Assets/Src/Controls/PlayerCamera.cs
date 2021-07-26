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

    public int cameraPreset = 0;

    void Awake() {
        Instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        AttachToPlayerIfNeeded();
        UpdateVFX();

        SetCameraPreset();
    }

    void SetCameraPreset() {
        CinemachineVirtualCamera virtCam = GetComponent<CinemachineVirtualCamera>();
        Cinemachine3rdPersonFollow bodySetting = virtCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        CinemachineComposer composer = virtCam.GetCinemachineComponent<CinemachineComposer>();
        
        switch (cameraPreset)
        {
            case 0:
                bodySetting.ShoulderOffset = new Vector3(0, -5, 0);
                bodySetting.CameraDistance = 18.3f;
                bodySetting.VerticalArmLength = 13f;
                composer.m_TrackedObjectOffset = new Vector3(0, 4.39f, 0);
                break;
            case 1:
                bodySetting.ShoulderOffset = new Vector3(0, 18.54f, 0);
                bodySetting.CameraDistance = 25.16f;
                bodySetting.VerticalArmLength = 0.69f;
                composer.m_TrackedObjectOffset = new Vector3(0, 4, 0);
                break;
            case 2:
                bodySetting.ShoulderOffset = new Vector3(0, 13, -5);
                bodySetting.CameraDistance = 17.75f;
                bodySetting.VerticalArmLength = 5.5f;
                composer.m_TrackedObjectOffset = new Vector3(0, 4.39f, 0);
                break;
            default:
                bodySetting.ShoulderOffset = new Vector3(0, -5, 0);
                bodySetting.CameraDistance = 18.3f;
                bodySetting.VerticalArmLength = 13f;
                break;
        }
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
        bool isSprinting = (Mathf.Abs(hor) >= 0.6f || Mathf.Abs(vert) >= 0.6f );
        sprintVfx.SetActive(isSprinting);
    }
}
