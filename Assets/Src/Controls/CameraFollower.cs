using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.Spawning;
using Cinemachine;

public class CameraFollower : MonoBehaviour
{
    public static CameraFollower Instance;
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

                    Team playerTeam = playerController.user.team;
                    camera.m_XAxis.Value = (playerTeam == Team.BLUE ? 180 : 0);
                }
            }
        }
    }

    public void ResetFaceDirection() {
        GameObject localPlayerObject = NetworkSpawnManager.GetLocalPlayerObject()?.gameObject;
        PlayerController playerController = localPlayerObject?.GetComponent<PlayerController>();
        if (playerController)
        {
            CinemachineFreeLook camera = GameObject.FindGameObjectWithTag("CinemachineCamera").GetComponent<CinemachineFreeLook>();
            Team playerTeam = playerController.user.team;
            camera.m_XAxis.Value = (playerTeam == Team.BLUE ? 180 : 0);
        }
    }
}
