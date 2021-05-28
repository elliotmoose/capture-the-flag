using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.SceneManagement;

using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;
using MLAPI.Transports.UNET;
using MLAPI.Spawning;

public class RoomManager : NetworkBehaviour
{  
    public static RoomManager Instance;

    public bool autoStartHost = false;
    
// public List<User> roomUsers = new List<User>();
    public NetworkList<User> roomUsers = new NetworkList<User>(new NetworkVariableSettings {
        WritePermission=NetworkVariablePermission.ServerOnly,
        SendTickrate=3
    });    
    public NetworkVariableInt maxPlayersPerTeam = new NetworkVariableInt(new NetworkVariableSettings {
        WritePermission=NetworkVariablePermission.ServerOnly,
        SendTickrate=-1
    }, 3);

    public delegate void RoomManagerEvent();
    public RoomManagerEvent OnRoomUsersUpdate;
    public RoomManagerEvent OnClientJoinRoom;

    void Start()
    {        
        // CreateRoom();
        roomUsers.OnListChanged += (NetworkListEvent<User> changeEvent)=>{
            if(OnRoomUsersUpdate != null) {
                OnRoomUsersUpdate();
            }
        };
        
        maxPlayersPerTeam.OnValueChanged += (int oldVal, int newVal)=>{
            if(OnRoomUsersUpdate != null) {
                OnRoomUsersUpdate();
            }
        };

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }


    // Update is called once per frame
    void Update()
    {

    }


    #region Server Side Functions
    public void StartGame() {
        if(!IsServer) {return;}
        
        bool canStartGame = (roomUsers.Count == maxPlayersPerTeam.Value*2);

        // if(!canStartGame) {
        //     Debug.LogWarning("== RoomManager: Cannot start game as non enough players");
        //     return;
        // }

        SceneTransitionManager.Instance.RoomToGameScene(GetUsers());
    }

    public void CreateRoom() 
    {            
        NetworkManager.Singleton.StartHost();

        if(IsHost) {
            Debug.Log($"Is Host with localClientId: {NetworkManager.Singleton.LocalClientId}");
            User newUser = new User(NetworkManager.Singleton.LocalClientId, Team.RED, "Loading...");
            newUser.username = UserManager.Instance.username;
            roomUsers.Add(newUser);
        }
    }

    //runs on server
    private void OnClientConnected(ulong clientId) {        
        if(IsServer) {
            int blueTeamCount = FindUsersWithTeam(Team.BLUE).Count;
            int redTeamCount = (roomUsers.Count - blueTeamCount);
            bool joinRed = (blueTeamCount > redTeamCount);
            Team team = joinRed ? Team.RED : Team.BLUE;
            User newUser = new User(clientId, team, "Loading...");
            Debug.Log($"== RoomManager (Server): Client connected: {clientId} and has joined {team} team");
            roomUsers.Add(newUser);
        }
        else if (IsClient) {
            Debug.Log("== Room Manager (Client): Connected to server!");
            SetUsernameServerRpc(NetworkManager.Singleton.LocalClientId, UserManager.Instance.username);

            if(OnClientJoinRoom != null) {
                //let client know to progress (change menu)
                OnClientJoinRoom();
            }
        }
    }
    
    private void OnClientDisconnected(ulong clientId) {
        if(IsServer) {
            User? userToRemove = FindUserWithClientId(clientId);
            if(userToRemove != null) {
                roomUsers.Remove(userToRemove.Value);
            }
        }
        else if (IsClient) {
            
        }
    }

    //server
    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        if(!IsServer) {return;}
        int noOfPlayers = roomUsers.Count;
        bool hasReachMaxPlayers = (noOfPlayers < maxPlayersPerTeam.Value*2);
        bool allowConnection = !hasReachMaxPlayers;
        bool createPlayerObject = true;
        ulong? prefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("NetworkPlayerController");
        callback(createPlayerObject, prefabHash, allowConnection, Vector3.zero, Quaternion.identity);
    }

    public User? FindUserWithClientId(ulong clientId) {
        foreach(User user in roomUsers) {
            if(user.clientId == clientId) {
                return user;
            }
        }
        
        return null;
    }

    public List<User> FindUsersWithTeam(Team team) {
        List<User> users = new List<User>();

        foreach(User user in roomUsers) {
            if(user.team == team) {
                users.Add(user);
            }
        }

        return users;
    }

    private void UpdateUserValue(ulong clientId, Team team, string username, Character character) {
        for(int i=0; i<roomUsers.Count; i++) {            
            User user = roomUsers[i];
            if(user.clientId == clientId) {
                User updatedUser = new User(clientId, team, username, character);;
                roomUsers[i] = updatedUser;
                return;
            }
        }

        Debug.LogWarning($"Could not find user to update: {clientId}");
    }
    #endregion

    #region Client Side Functions
    public void JoinRoom(string ipAddressInput) 
    {    
        Debug.Log($"== RoomManager: Joining Room {ipAddressInput}...");
        string ipAddress = (ipAddressInput.Length <= 0) ? "127.0.0.1" : ipAddressInput;
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ipAddress;
        NetworkManager.Singleton.StartClient();
    }

    public void LeaveRoom() {
        if(IsHost) {
            NetworkManager.Singleton.StopHost();
        }
        else if(IsClient) {
            NetworkManager.Singleton.StopClient();
        }
        else if(IsServer){
            NetworkManager.Singleton.StopServer();
        }
    }

    [ServerRpc(RequireOwnership=false)]
    public void JoinTeamServerRpc(ulong clientId, Team team) {
        if(IsServer) {
            int teamCount = FindUsersWithTeam(team).Count;
            if(teamCount < maxPlayersPerTeam.Value) {
                User? user = FindUserWithClientId(clientId);
                if(user != null) {
                    UpdateUserValue(user.Value.clientId, team, user.Value.username, user.Value.character);
                }   
            }
        }
    }
    
    [ServerRpc(RequireOwnership=false)]
    void SetUsernameServerRpc(ulong clientId, string username) {
        if(IsServer) {
            User? user = FindUserWithClientId(clientId);
            if(user != null) {
                UpdateUserValue(user.Value.clientId, user.Value.team, username, user.Value.character);
            }
        }
    }
    
    [ServerRpc(RequireOwnership=false)]
    public void SelectCharacterServerRpc(ulong clientId, Character character) {
        if(IsServer) {
            User? user = FindUserWithClientId(clientId);
            if(user != null) {
                UpdateUserValue(user.Value.clientId, user.Value.team, user.Value.username, character);
            }
        }
    }

    #endregion

    //this will be called by GameManager when the game begins
    public List<User> GetUsers() {
        List<User> users = new List<User>();

        foreach(User user in roomUsers) {
            users.Add(user);
        }
        
        return users;
    }

    void Awake() {
        if(Instance != null) {
            throw new System.Exception("More than one RoomManager exists");
        }
        Instance = this;
    }

    void OnDestroy() {
        if(IsServer) {
            //TODO: check if events are unsubscribed when changing scene
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
        
        Instance = null;
    }
}
