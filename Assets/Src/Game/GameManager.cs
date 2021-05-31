using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public GameObject playerControllerPrefab;

    public GameObject redTeamFlag;
    public GameObject blueTeamFlag;

    public Jail blueTeamJail;
    public Jail redTeamJail;
    
    public NetworkVariableInt blueTeamScore = new NetworkVariableInt(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    });
    public NetworkVariableInt redTeamScore = new NetworkVariableInt(new NetworkVariableSettings{
        SendTickrate = -1,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    public List<User> users = new List<User>();
    public int roomSize = 3; //no of players per team
    


    // Start is called before the first frame update
    // void Start()
    // {
    //     if(!IsServer) { return; }
    //     Debug.Log(users.Count);
    //     StartGame();
    // }

    public void StartGame() {
        if(!IsServer) { return; }
        
        Debug.Log("== GameManager: Game Started!");
        SpawnPlayerControllers();
        ResetFlags();        
    }

    public void ScorePoint(Team team) {
        if(team == Team.BLUE) {
            blueTeamScore.Value += 1;
        }
        else {
            redTeamScore.Value += 1;
        }

        ResetRound();
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void ResetRound() {
        if(!IsServer) {return;}
        ResetFlags();
        ResetPlayerPositions();
    }

    void ResetPlayerPositions() {
        foreach(Player player in PlayerSpawner.Instance.GetAllPlayers()) {
            player.ResetForRound();
        }
    }

    void SpawnPlayerControllers() {
        if(!IsServer) {return;}

        int redTeamIndex = 0;
        int blueTeamIndex = 0;

        //testing
        // for(int i=0; i<5; i++) {
        //     users.Add(new User(0, i%2 == 0 ? Team.BLUE: Team.RED, "test", Character.Warrior));
        // }
        
        foreach(User user in users) {
            //spawn player controller            
            GameObject playerControllerObject = GameObject.Instantiate(playerControllerPrefab, Vector3.zero, Quaternion.identity);
            PlayerController playerController = playerControllerObject.GetComponent<PlayerController>();
            playerController.user = user;
            playerControllerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(user.clientId);

            //spawn player
            int playerIndex = (user.team == Team.RED ? redTeamIndex : blueTeamIndex);
            GameObject spawnedPlayerGO = PlayerSpawner.Instance.SpawnPlayer(user.clientId, user.team, user.character, playerIndex, roomSize); 

            //link
            playerController.LinkPlayerReference(spawnedPlayerGO);            

            if(user.team == Team.BLUE) {
                blueTeamIndex += 1;
            }
            else {
                redTeamIndex += 1;
            }
        }
    }

    void ResetFlags() {
        if(!IsServer) { return; }
        redTeamFlag.GetComponent<Flag>().SetTeam(Team.RED);
        redTeamFlag.GetComponent<Flag>().ResetPosition();
        blueTeamFlag.GetComponent<Flag>().SetTeam(Team.BLUE);
        blueTeamFlag.GetComponent<Flag>().ResetPosition();
    }

    void Awake() {
        if(Instance != null) {
            throw new System.Exception("More than one GameManager exists");
        }
        Instance = this;
        DontDestroyOnLoad(transform.gameObject);
    }

    void OnDestroy() {
        Instance = null;
    }
}

    