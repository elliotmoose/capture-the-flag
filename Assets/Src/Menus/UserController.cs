using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class UserController : NetworkBehaviour
{
    public static UserController LocalInstance;

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
        if(IsLocalPlayer) {
            LocalInstance = this;            
            SetUsernameServerRpc(NetworkManager.Singleton.LocalClientId, UserManager.Instance.username);
        }
    }

    [ServerRpc(RequireOwnership=false)]
    public void JoinTeamServerRpc(ulong clientId, Team team) {
        RoomManager.Instance.UserRequestJoinTeam(clientId, team);
    }
    
    [ServerRpc(RequireOwnership=false)]
    void SetUsernameServerRpc(ulong clientId, string username) {
        RoomManager.Instance.UserRequestSetUsername(clientId, username);
    }
    
    [ServerRpc(RequireOwnership=false)]
    public void SelectCharacterServerRpc(ulong clientId, Character character) {
        RoomManager.Instance.UserRequestSelectCharacter(clientId, character);
    }
}
