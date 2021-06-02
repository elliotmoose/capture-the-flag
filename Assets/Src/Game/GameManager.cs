using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

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

    public bool roundInProgress = false;

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
        ResetRound();
        BeginCountdownForRound();
    }

    private void BeginCountdownForRound() {
        roundInProgress = false;
        StartCoroutine(RoundCountdown());
    }

    private IEnumerator RoundCountdown() {
        CountdownClientRpc(3);
        yield return new WaitForSeconds(1);
        CountdownClientRpc(2);
        yield return new WaitForSeconds(1);
        CountdownClientRpc(1);
        yield return new WaitForSeconds(1);
        CountdownClientRpc(0);
        roundInProgress = true;
    }

    [ClientRpc]
    private void CountdownClientRpc(int count) {
        UIManager.Instance.DisplayCountdown(count);        
    } 

    [ClientRpc]
    private void ResetPlayerCamerasClientRpc() {
        CameraFollower.Instance.ResetFaceDirection();
    }

    public void Imprison(Player player, Player imprisonedBy) {
        if(player.GetTeam() == imprisonedBy.GetTeam()) {
            Debug.LogError("Cannot be imprisoned by player of same team");
            return;
        }

        if(player.team.Value == Team.BLUE) {
            redTeamJail.Imprison(player);
        }
        else {
            blueTeamJail.Imprison(player);
        }
    }

    public void Release(Player player, Player releasedBy) {
        Jail targetJail = player.GetTeam() == Team.BLUE ? blueTeamJail : redTeamJail;        
        // only release if player is not jailed themselves
        if (!targetJail.GetJailedPlayers().Contains(player))
        {
            targetJail.Release(player);
        }
    }

    public void ResetJail() {
        blueTeamJail.ReleaseAll();
        redTeamJail.ReleaseAll();
    }
    
    public void ScorePoint(Team team) {
        if(team == Team.BLUE) {
            blueTeamScore.Value += 1;
        }
        else {
            redTeamScore.Value += 1;
        }

        ResetRound();
        BeginCountdownForRound();
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

        ResetPlayerCamerasClientRpc();
    }

    void SpawnPlayerControllers() {
        if(!IsServer) {return;}

        List<User> users = RoomManager.Instance.GetUsers();
        int roomSize = RoomManager.Instance.roomSize.Value;

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
            playerControllerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(user.clientId);
            playerController.user.Value = user;

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

    