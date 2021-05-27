using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Player : NetworkBehaviour
{
    //movement controls
    public GameObject flagSlot;
    public Vector2 moveDir = Vector2.zero;
    public float faceAngle = 0;

    float moveSpeed = 18;
    float sprintMultiplier = 2.4f;
    public bool sprinting = false;
    public Team team = Team.BLUE;

    public NetworkVariableULong ownerClientId = new NetworkVariableULong(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    public NetworkVariableFloat stamina = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = 20,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 100);
    
    public NetworkVariableFloat maxStamina = new NetworkVariableFloat(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    }, 100);

    private float _animationTransitionTime = 0.15f;
    private float _curTransitionTime = 0f;

    void Update()
    {
        if(IsServer) {
            this.transform.rotation = Quaternion.Euler(0, faceAngle, 0);

            //test
            if(sprinting) {
                stamina.Value -= Time.deltaTime * 20;
            }
            
            bool isMoving = (moveDir.magnitude > 0.01f);
            if(isMoving) {                
                float moveDirAngle = Mathf.Atan2(moveDir.x, moveDir.y) * Mathf.Rad2Deg + faceAngle;
                Vector3 positionDelta = Quaternion.Euler(0, moveDirAngle, 0) * Vector3.forward;
                transform.position += positionDelta.normalized * Time.deltaTime * moveSpeed * (sprinting ? sprintMultiplier : 1);
            }

            Vector2 targetMoveDir = moveDir/(sprinting ? 1 : 2);
            //animation smoohting
            Animator animator = GetComponent<Animator>();
            Vector2 curMoveDir = new Vector2(animator.GetFloat("HorMovement"), animator.GetFloat("VertMovement")); //current movedir state

            if(curMoveDir != targetMoveDir) {
                // _lastMoveDir = curMoveDir;
                _curTransitionTime += Time.deltaTime;
                Vector2 newSmoothMoveDir = Vector2.Lerp(curMoveDir, targetMoveDir, _curTransitionTime/_animationTransitionTime);
                animator.SetFloat("HorMovement", newSmoothMoveDir.x);
                animator.SetFloat("VertMovement", newSmoothMoveDir.y);            
            }
            else {
                _curTransitionTime = 0;
            }
            
            animator.SetBool("IsMoving", isMoving);
        }
    }

    void LateUpdate() {
        //TODO: face based on facedir
        // if(NetworkManager.LocalClientId == ownerClientId.Value) {
        //     transform.rotation = Quaternion.Euler(transform.rotation.x, Camera.main.transform.eulerAngles.y, transform.rotation.z);
        // }
    }

    public void ResetForRound() {
        this.stamina = maxStamina;
        this.transform.position = Vector3.zero; //todo: make spawn point
    }
}
