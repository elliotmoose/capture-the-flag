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

    NetworkVariableBool sprinting = new NetworkVariableBool(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly
    });

    public bool GetSprinting() {
        return sprinting.Value;
    }

    NetworkVariableVector2 moveDir = new NetworkVariableVector2(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly,
        SendTickrate = 20,
    });
    
    NetworkVariableFloat faceAngle = new NetworkVariableFloat(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly,
        SendTickrate = 20,
    });

    //skill cooldown displats
    public NetworkVariableFloat skill1CooldownDisplay = new NetworkVariableFloat(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly,
        SendTickrate = 1,
    });
    public NetworkVariableFloat skill2CooldownDisplay = new NetworkVariableFloat(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly,
        SendTickrate = 1,
    });
    public NetworkVariableFloat catchCooldownDisplay = new NetworkVariableFloat(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.OwnerOnly,
        SendTickrate = 1,
    });

    public NetworkVariableULong playerObjNetId = new NetworkVariableULong(new NetworkVariableSettings{
        WritePermission = NetworkVariablePermission.ServerOnly,
        SendTickrate = 0,        
    });

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
    // Start is called before the first frame update
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

            LocalPlayer player = GetPlayer();
            if(player) {
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");
                
                player.moveDir = new Vector2(horizontal, vertical);
                
                player.faceAngle += Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY;
                // player.moveDir = moveDir.Value;
                // player.faceAngle = faceAngle.Value;
                player.sprinting = sprinting.Value;

                // Debug.Log(player.faceAngle);
            }
        }

        if(IsServer) {
            //server should move player
            // GameObject playerObj = GameObject.Find("player"+GetComponent<NetworkObject>().OwnerClientId.ToString());
            LocalPlayer player = GetPlayer();
            if(player) {
                // player.moveDir = moveDir.Value;
                // player.faceAngle = faceAngle.Value;
                // player.sprinting = sprinting.Value;

                //display cooldowns
                skill1CooldownDisplay.Value = player.skill1CooldownTime;
                skill2CooldownDisplay.Value = player.skill2CooldownTime;
                catchCooldownDisplay.Value = player.catchCooldownTime;
            }
        }
    }

    void ClientControls() {
        //movement
        // float horizontal = Input.GetAxisRaw("Horizontal");
        // float vertical = Input.GetAxisRaw("Vertical");
        
        // moveDir.Value = new Vector2(horizontal, vertical);
        
        // faceAngle.Value += Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY;
        // faceAngle.Value = Camera.main.transform.eulerAngles.y;
        
        if(Input.GetMouseButtonDown(0)) {
            Debug.Log("Catch!");
            CatchServerRpc();
        }
        if(Input.GetKeyDown(SKILL1_KEY)) {
            Skill1ServerRpc();
        }
        if(Input.GetKeyDown(SKILL2_KEY)) {
            Skill2ServerRpc();
        }
        
        sprinting.Value = Input.GetKey(SPRINT_KEY);
    }

    [ServerRpc]
    void CatchServerRpc() {
        if(IsServer) {            
            LocalPlayer player = GetPlayer();
            if (player)
            {
                player.Catch();
            }
        }
    }

    [ServerRpc]
    void Skill1ServerRpc() {
        if(IsServer) {
            LocalPlayer player = GetPlayer();
            if(player) {
                player.CastSkillAtIndex(0);
            }
        }
    }

    [ServerRpc]
    void Skill2ServerRpc() {
        if(IsServer) {
            LocalPlayer player = GetPlayer();
            if(player) {
                player.CastSkillAtIndex(1);
            }
        }
    }

    public void ResetFaceAngle() {
        faceAngle.Value = (GetUser().team == Team.BLUE ? 180 : 0);
    }

    public void LinkPlayerReference(GameObject playerGameObject) {
        if(!IsServer) {return;}
        playerObjNetId.Value = playerGameObject.GetComponent<NetworkObject>().NetworkObjectId;
    }    

    public LocalPlayer GetPlayer()
    {
        return LocalPlayer.WithClientId(this.OwnerClientId);
        // if(!NetworkSpawnManager.SpawnedObjects.ContainsKey(playerObjNetId.Value)) {
        //     Debug.Log("Could not find player for this PlayerController");
        //     return null;
        // }
        // NetworkObject playerNetworkObj = NetworkSpawnManager.SpawnedObjects[playerObjNetId.Value];
        // GameObject playerObj = playerNetworkObj.gameObject;
        // if (playerObj)
        // {
        //     return playerObj.GetComponent<LocalPlayer>();
        // }
        // else
        // {
        //     return null;
        // }
    }
}
