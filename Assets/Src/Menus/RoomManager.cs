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

/// <summary>
/// IMPLEMENTATION DETAILS:
/// 1. RoomManager requires a NetworkObject in order to be synced
/// 2. However, we want to put RoomManager on NetworkManager so that the script will be brought into Game scene
/// 
/// The two are mutually exclusive. as NetworkObjects cannot be brought into next scene.
/// 
/// To solve this, for each scene we have a RoomManager to keep track of the users. We let the SceneTransitionManager hand over the NetworkList states
/// </summary>
public class RoomManager : NetworkBehaviour
{  
    public static RoomManager Instance;
    
    public NetworkList<User> roomUsers = new NetworkList<User>(new NetworkVariableSettings {
        WritePermission=NetworkVariablePermission.ServerOnly,
        SendTickrate=3
    });    
    public NetworkVariableInt roomSize = new NetworkVariableInt(new NetworkVariableSettings {
        WritePermission=NetworkVariablePermission.ServerOnly,
        SendTickrate=3
    }, 3);

    public delegate void RoomManagerEvent();
    public RoomManagerEvent OnRoomUsersUpdate;
    public RoomManagerEvent OnClientJoinRoom;
    public RoomManagerEvent OnClientLeaveRoom;

    public GameObject userControllerPrefab;

    void Start()
    {        
        // CreateRoom();
        roomUsers.OnListChanged += (NetworkListEvent<User> changeEvent)=>{
            if(OnRoomUsersUpdate != null) OnRoomUsersUpdate();
        };
        
        roomSize.OnValueChanged += (int oldVal, int newVal)=>{
            if(OnRoomUsersUpdate != null) OnRoomUsersUpdate();
        };

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    void OnDestroy() {
        //TODO: check if events are unsubscribed when changing scene
        // NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        // NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        // NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;        
        // Instance = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    #region Server Side Functions
    

    //runs on server
    private void OnClientConnected(ulong clientId) {        
        if(NetworkManager.Singleton.IsServer) {
            int blueTeamCount = FindUsersWithTeam(Team.BLUE).Count;
            int redTeamCount = (roomUsers.Count - blueTeamCount);
            bool joinRed = (redTeamCount <= blueTeamCount);
            Team team = joinRed ? Team.RED : Team.BLUE;
            User newUser = new User(clientId, team, "Loading...");
            Debug.Log($"== RoomManager (Server): Client connected: {clientId} and has joined {team} team");
            roomUsers.Add(newUser);

            GameObject userController = GameObject.Instantiate(userControllerPrefab, Vector3.zero, Quaternion.identity);
            userController.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, null, true);            
        }
        else if (NetworkManager.Singleton.IsClient) {
            Debug.Log("== Room Manager (Client): Connected to server!");            
            //let client know to progress (change menu)
            if(OnClientJoinRoom != null) OnClientJoinRoom();
        }
    }


    
    private void OnClientDisconnected(ulong clientId) {
        if(NetworkManager.Singleton.IsServer) {
            User? userToRemove = FindUserWithClientId(clientId);
            if(userToRemove != null) {
                roomUsers.Remove(userToRemove.Value);
            }
        }
        else if (IsClient) {
            
        }
        else if (IsHost) {
            
        }
        else
        {
            if(OnClientLeaveRoom != null) {
                OnClientLeaveRoom();
            }
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    //server
    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        if(!NetworkManager.Singleton.IsServer) {
            Debug.Log("client approval check");
            return;
        }
        int noOfPlayers = roomUsers.Count;
        bool hasReachMaxPlayers = (noOfPlayers >= roomSize.Value*2);
        bool allowConnection = !hasReachMaxPlayers;
        bool createPlayerObject = true;
        ulong? prefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("NetworkPlayerController");
        Debug.Log($"== RoomManager: Approving client connection: {allowConnection}");
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

    #region User actions

    public void StartGame() {
        if(!NetworkManager.Singleton.IsServer) {return;}
        
        bool canStartGame = (roomUsers.Count == roomSize.Value*2);

        // if(!canStartGame) {
        //     Debug.LogWarning("== RoomManager: Cannot start game as non enough players");
        //     return;
        // }

        // foreach(User user in GetUsers()) {
        //     Debug.Log("Destroying user controller:" + user.username);
        //     NetworkObject no = NetworkSpawnManager.GetPlayerNetworkObject(user.clientId);
        //     no.Despawn(true);
        // }
        foreach (User user in GetUsers())
        {
            Debug.Log("Destroying user controller:" + user.username);
            NetworkObject no = NetworkSpawnManager.GetPlayerNetworkObject(user.clientId);

            if (no != null)
            {
                no.Despawn(true);
            }
        }

        SceneTransitionManager.Instance.RoomToGameScene(GetUsers(), roomSize.Value);
    }

    public void CreateRoom() 
    {            
        NetworkManager.Singleton.StartHost();

        if(NetworkManager.Singleton.IsHost) {
            Debug.Log($"Is Host with localClientId: {NetworkManager.Singleton.LocalClientId}");
            OnClientConnected(NetworkManager.Singleton.LocalClientId);
        }
    }
    
    public void JoinRoom(string ipAddressInput) 
    {    
        Debug.Log($"== RoomManager: Joining Room {ipAddressInput}...");
        string ipAddress = (ipAddressInput.Length <= 0) ? "127.0.0.1" : ipAddressInput;
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ipAddress;
        NetworkManager.Singleton.StartClient();
        Debug.Log($"== RoomManager: StartClient finished");
    }

    
    public void LeaveRoom() {
        if (NetworkManager.Singleton.IsHost) {
            NetworkManager.Singleton.StopHost();
        }
        else if(NetworkManager.Singleton.IsClient) {
            NetworkManager.Singleton.StopClient();
        }
        else if(NetworkManager.Singleton.IsServer){
            NetworkManager.Singleton.StopServer();
        }
        
    }

    public void UserRequestJoinTeam(ulong clientId, Team team) {
        Debug.Log($"{clientId} joint team {team}");
        if(IsServer) {
            int teamCount = FindUsersWithTeam(team).Count;
            if(teamCount < roomSize.Value) {
                User? user = FindUserWithClientId(clientId);
                if(user != null) {
                    UpdateUserValue(user.Value.clientId, team, user.Value.username, user.Value.character);
                }   
            }
            else {
                Debug.LogWarning("Need to implement request team swap mechanism");
            }
        }
    }
    
    public void UserRequestSetUsername(ulong clientId, string username) {
        // Debug.Log($"{clientId} set username {username}");
        if(NetworkManager.Singleton.IsServer) {
            User? user = FindUserWithClientId(clientId);
            if(user != null) {
                UpdateUserValue(user.Value.clientId, user.Value.team, username, user.Value.character);
            }
        }
    }
    
    public void UserRequestSelectCharacter(ulong clientId, Character character) {
        // Debug.Log($"{clientId} selected {character}");
        if(NetworkManager.Singleton.IsServer) {
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
        // if(Instance != null) {
        //     throw new System.Exception("More than one RoomManager exists");
        // }
        Instance = this;
    }
}
