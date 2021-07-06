using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Spawning;
using MLAPI.NetworkVariable;
using Cinemachine;
public class PlayerController : NetworkBehaviour 
{   
    public static PlayerController LocalInstance;
    float MOUSE_SENSITIVITY = 15;

    // public User user;
    private NetworkVariable<User> _user = new NetworkVariable<User>(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.ServerOnly,
        SendTickrate = 0,
    });

    public User GetUser() {
        return this._user.Value;
    }

    public void SetUser(User user) {
        this._user.Value = user;
    }

    public KeyCode SKILL1_KEY = KeyCode.Q;
    public KeyCode SKILL2_KEY = KeyCode.E;
    public KeyCode SPRINT_KEY = KeyCode.Space;

    public override void NetworkStart()
    {
        base.NetworkStart();        
        if(IsLocalPlayer) {
            LocalInstance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {   
        if(IsLocalPlayer) {
            ClientControls();
        }
    }

    void ClientControls() {
        //movement
        LocalPlayer localPlayer = GetPlayer();
        if(localPlayer) {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");            
            localPlayer.moveDir = new Vector2(horizontal, vertical);            
            localPlayer.faceAngle += Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY;
            localPlayer.sprinting = Input.GetKey(SPRINT_KEY);
        
            if(Input.GetMouseButtonDown(0)) {
                localPlayer.Catch();
            }
            if(Input.GetKeyDown(SKILL1_KEY)) {
                localPlayer.CastSkillAtIndex(0);
            }
            if(Input.GetKeyDown(SKILL2_KEY)) {
                localPlayer.CastSkillAtIndex(1);
            }
        }
        
    }

    public void ResetFaceAngle() {
        LocalPlayer localPlayer = GetPlayer();
        if(localPlayer) { 
            localPlayer.faceAngle = (localPlayer.team == Team.BLUE ? 180 : 0);
        }
    }

    private LocalPlayer _player;
    public LocalPlayer GetPlayer()
    {
        if(_player != null) return _player;
        _player = LocalPlayer.WithClientId(this.OwnerClientId);        
        return _player;
    }
}
