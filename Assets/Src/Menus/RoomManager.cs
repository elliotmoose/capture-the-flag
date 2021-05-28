using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Serialization;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using MLAPI.Messaging;
using MLAPI.Transports.UNET;
using MLAPI.Spawning;

public struct User : INetworkSerializable {
    public ulong clientId;
    public string username;
    public Team team;
    public string character;

    public User(ulong clientId, Team team, string username) {
        this.clientId = clientId;
        this.team = team;
        this.username = username;
        this.character = "";
    }

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref clientId);
        serializer.Serialize(ref username);
        serializer.Serialize(ref team);
    }
}

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

        DontDestroyOnLoad(this.gameObject);
    }


    // Update is called once per frame
    void Update()
    {

    }

    public override void NetworkStart()
    {
        //on connected
        if(IsClient && !IsHost) {
            //clients should tell the server what their username is when they connect
            SetUsernameServerRpc(NetworkManager.LocalClientId, UserManager.Instance.username);          
            if(OnClientJoinRoom != null) {
                OnClientJoinRoom();
            }
        }
    }

    #region Server Side Functions
    public void StartGame() {
        if(!IsServer) {return;}
        
        bool canStartGame = (roomUsers.Count == maxPlayersPerTeam.Value*2);

        // if(!canStartGame) {
        //     Debug.LogWarning("== RoomManager: Cannot start game as non enough players");
        //     return;
        // }

        Debug.Log($"Pre scene change user count: ${roomUsers.Count}");
        NetworkSceneManager.SwitchScene("Game");
        Debug.Log($"Post scene change user count: ${roomUsers.Count}");
    }

    public void CreateRoom() 
    {    
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;    
        NetworkManager.Singleton.StartHost();

        if(IsHost) {
            Debug.Log($"Is Host with localClientId: {NetworkManager.LocalClientId}");
            User newUser = new User(NetworkManager.LocalClientId, Team.RED, "Loading...");
            newUser.username = UserManager.Instance.username;
            roomUsers.Add(newUser);
        }
    }

    //runs on server
    private void OnClientConnected(ulong clientId) {        
        int blueTeamCount = FindUsersWithTeam(Team.BLUE).Count;
        int redTeamCount = (roomUsers.Count - blueTeamCount);
        bool joinRed = (blueTeamCount > redTeamCount);
        Team team = joinRed ? Team.RED : Team.BLUE;
        User newUser = new User(clientId, team, "Loading...");
        Debug.Log($"== RoomManager: Client connected: {clientId} and has joined {team} team");
        roomUsers.Add(newUser);
    }
    
    private void OnClientDisconnected(ulong clientId) {
        User? userToRemove = FindUserWithClientId(clientId);
        if(userToRemove != null) {
            roomUsers.Remove(userToRemove.Value);
        }
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
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

    private void UpdateUserValue(ulong clientId, Team team, string username) {
        for(int i=0; i<roomUsers.Count; i++) {            
            User user = roomUsers[i];
            if(user.clientId == clientId) {
                User updatedUser = new User(clientId, team, username);;
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
    void JoinTeamServerRpc(ulong clientId, Team team) {
        if(IsServer) {
            int teamCount = FindUsersWithTeam(team).Count;
            if(teamCount < maxPlayersPerTeam.Value) {
                User? user = FindUserWithClientId(clientId);
                if(user != null) {
                    UpdateUserValue(user.Value.clientId, team, user.Value.username);
                }   
            }
        }
    }
    
    [ServerRpc(RequireOwnership=false)]
    void SetUsernameServerRpc(ulong clientId, string username) {
        if(IsServer) {
            User? user = FindUserWithClientId(clientId);
            if(user != null) {
                UpdateUserValue(user.Value.clientId, user.Value.team, username);
            }
        }
    }

    #endregion

    //this will be called by GameManager when the game begins
    public List<User> HandoverUsersAndDestory() {
        List<User> users = new List<User>();

        foreach(User user in roomUsers) {
            users.Add(user);
        }

        Debug.Log($"roomUsers: {roomUsers.Count} vs users:{users.Count}");
        //tell everyone to destroy room manager
        // GameObject.Destroy(this.gameObject);
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
