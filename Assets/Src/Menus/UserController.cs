using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class UserController : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void NetworkStart() {
        //upon connecting, client tells server what their username is
        // if(IsLocalPlayer) {
        //     SetUsernameServerRpc(UserManager.Instance.username);
        // }
    }

    // [ServerRpc]
    // void JoinTeamServerRpc(Team team) {
    //     if(IsServer) {
    //         ulong clientId = this.OwnerClientId;
    //         RoomManager.Instance.JoinTeam(clientId, team);
    //     }
    // }
    
    // [ServerRpc]
    // void SetUsernameServerRpc(string username) {
    //     if(IsServer) {
    //         ulong clientId = this.OwnerClientId;
    //         RoomManager.Instance.SetUsername(clientId, username);
    //     }
    // }
}
