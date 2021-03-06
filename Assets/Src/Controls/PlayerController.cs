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

    // public User user;
    private NetworkVariable<User> _user = new NetworkVariable<User>(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.ServerOnly,
        SendTickrate = 0,
    });

    private NetworkVariable<GameState> _playerGameState = new NetworkVariable<GameState>(new NetworkVariableSettings{
        WritePermission=NetworkVariablePermission.OwnerOnly,
        SendTickrate=10
    });

    public GameState playerGameState {
        get {
            return _playerGameState.Value;
        }

        set { 
            _playerGameState.Value = value;
        }
    }

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
        if(localPlayer && !InGameMenuManager.Instance.isMenuActive) {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if(!localPlayer.HasEffect("REACH_EFFECT")) {
                localPlayer.faceAngle += Input.GetAxis("Mouse X") * SettingsManager.Instance.mouseSensitivity;
            }

            localPlayer.moveDir = new Vector2(horizontal, vertical);            
            localPlayer.sprinting = Input.GetKey(SPRINT_KEY);
        
            if(Input.GetMouseButtonDown(0)) {
                localPlayer.Catch();
            }
            if(Input.GetMouseButtonDown(1)) {
                localPlayer.PassFlag();
            }
            if(Input.GetKeyDown(SKILL1_KEY)) {
                localPlayer.CastSkillAtIndex(0);
            }
            if(Input.GetKeyDown(SKILL2_KEY)) {
                localPlayer.CastSkillAtIndex(1);
            }
        }        
    }

    private LocalPlayer _player;
    public LocalPlayer GetPlayer()
    {
        if(_player != null) return _player;
        _player = LocalPlayer.WithClientId(this.OwnerClientId);        
        return _player;
    }

    public static List<PlayerController> AllControllers() {
        List<PlayerController> controllers = new List<PlayerController>();

        List<User> users = RoomManager.Instance.GetUsers();
        foreach(User user in users) {
            GameObject playerControllerGO = NetworkSpawnManager.GetPlayerNetworkObject(user.clientId).gameObject;
            PlayerController controller = playerControllerGO.GetComponent<PlayerController>();
            controllers.Add(controller);
        }

        return controllers;
    }
}
