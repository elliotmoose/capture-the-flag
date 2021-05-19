using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.Spawning;

public class CameraFollower : MonoBehaviour
{
    GameObject target;
    public ControlType controlType = ControlType.ThirdPerson;

    // Update is called once per frame
    void Update()
    {
        AttachToPlayerIfNeeded();
    }

    void LateUpdate() {
        if(target) {
            switch (controlType)
            {
                case ControlType.ThirdPerson:
                    //calculate camera's position based on orientation, that was set by player controller
                    // float yRotationAngle = this.transform.rotation.eulerAngles.y;
                    // float radius = 7;
                    // float newX = target.transform.position.x + 
                    break;
                case ControlType.TopDown:
                    this.transform.position = target.transform.position + new Vector3(0, 12, -4);
                    this.transform.rotation = Quaternion.LookRotation(target.transform.position - this.transform.position);
                    break;
            }
        }
    }

    void AttachToPlayerIfNeeded() {
        if(!target) { 
            GameObject localPlayerObject = NetworkSpawnManager.GetLocalPlayerObject()?.gameObject;
            PlayerController playerController = localPlayerObject?.GetComponent<PlayerController>();

            if(playerController) {
                ulong playerGameObjectNetId = playerController.playerObjNetId.Value;
                Debug.Log(playerGameObjectNetId);
                if(playerGameObjectNetId != 0) {
                    target = NetworkSpawnManager.SpawnedObjects[playerGameObjectNetId].gameObject;
                }
            }
        }
    }
}
