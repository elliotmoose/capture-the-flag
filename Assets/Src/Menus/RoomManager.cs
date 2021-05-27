using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;

public class User {
    public ulong clientId;
    public string username;
    public Team team = Team.BLUE;

    public User(ulong clientId, Team team) {
        this.clientId = clientId;
        username = "loading...";
    }
}

public class RoomManager : NetworkBehaviour
{  
    public static RoomManager Instance;

    public int maxPlayersPerTeam = 3;
    public bool autoStartHost = false;
    
    List<User> roomUsers = new List<User>();

    void Start()
    {        
        CreateRoom();
    }



    // Update is called once per frame
    void Update()
    {

    }

    private void CreateRoom() 
    {    
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;    
        NetworkManager.Singleton.StartHost();

        if(IsHost) {
            Debug.Log($"Is Host with localClientId: {NetworkManager.LocalClientId}");
            User newUser = new User(NetworkManager.LocalClientId, Team.BLUE);
            roomUsers.Add(newUser);
        }
    }

    private void OnClientConnected(ulong clientId) {
        int blueTeamCount = roomUsers.FindAll((eachUser)=>eachUser.team == Team.BLUE).Count;
        int redTeamCount = (roomUsers.Count - blueTeamCount);
        bool joinRed = (blueTeamCount > redTeamCount);
        Team team = joinRed ? Team.RED : Team.BLUE;
        User newUser = new User(clientId, team);
        Debug.Log($"== RoomManager: Client connected: {clientId} and has joined {team} team");
    }
    
    private void OnClientDisconnected(ulong clientId) {
        User userToRemove = FindUserWithClientId(clientId);
        if(userToRemove != null) {
            roomUsers.Remove(userToRemove);
        }
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        int noOfPlayers = roomUsers.Count;
        bool hasReachMaxPlayers = (noOfPlayers < maxPlayersPerTeam*2);
        bool allowConnection = !hasReachMaxPlayers;
        bool createPlayerObject = true;
        ulong? prefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("NetworkPlayerController");
        callback(createPlayerObject, prefabHash, allowConnection, Vector3.zero, Quaternion.identity);
    }

    private User FindUserWithClientId(ulong clientId) {
        int roomUserIndex = roomUsers.FindIndex((User eachUser)=>eachUser.clientId == clientId);
        if(roomUserIndex != -1) {
            return roomUsers[roomUserIndex];
        }
        
        return null;
    }

    //CLIENT ACTIONS

    public void SetUsername(ulong clientId, string username) {
        User user = FindUserWithClientId(clientId);
        if(user != null) {
            //TODO sync this information across clients
            user.username = username;
        }
    }

    public void JoinTeam(ulong clientId, Team team) {
        int teamCount = roomUsers.FindAll((eachUser)=>eachUser.team == team).Count;
        if(teamCount < maxPlayersPerTeam) {
            User user = FindUserWithClientId(clientId);
            if(user != null) {
                //TODO sync this information across clients
                user.team = team;
            }   
        }
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
