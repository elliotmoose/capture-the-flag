using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public delegate void GameEvent(Player player);
public delegate void GameEventInteraction(Player receiver, Player giver);

public enum GameServerState {
    SERVER_INIT, //init stats, generate summary ui for host player
    SPAWNING, //spawn players and flags
    RESETTING, //reset player positions, effects, flag positions
    COUNTDOWN, //countdown
    PLAY,
    SCORESCREEN
}
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public GameObject playerControllerPrefab;

    public Flag redTeamFlag;
    public Flag blueTeamFlag;
    
    public NetworkVariableInt blueTeamScore = new NetworkVariableInt(new NetworkVariableSettings{
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly
    });
    public NetworkVariableInt redTeamScore = new NetworkVariableInt(new NetworkVariableSettings{
        SendTickrate = 0,
        WritePermission = NetworkVariablePermission.ServerOnly
    });

    int winScore = 15;

    public bool roundInProgress = false;
    public bool gameInProgress = false;

    public event GameEvent OnPlayerScored;
    public event GameEvent OnFlagCaptured;
    public event GameEventInteraction OnPlayerJailed;
    public event GameEventInteraction OnPlayerFreed;

    GameServerState serverState = GameServerState.SERVER_INIT;
    
    //Spawn PlayerControllers (spawns players) -> Spawn flags -> (INSERT SANITY CHECK FOR PLAYER SPAWN) )Reset Positions 
    public void StartGame() {        
        if(!IsServer) { return; }
        Debug.Log("== GameManager: Game Started!");
        UIManager.Instance.GenerateGameSummaryUI();
        StatsManager.Instance.Initialise(RoomManager.Instance.GetUsers());    

        SpawnPlayerControllers();
   
        SetGameInProgress(true);
        ResetRoundClientRpc();
        BeginCountdownForRound();        
    }

    [ClientRpc]
    private void ResetRoundClientRpc() {
        //player positions
        foreach(LocalPlayer localPlayer in LocalPlayer.AllPlayers()) {
            localPlayer.ResetForRound();
        }
        
        PlayerController.LocalInstance.ResetFaceAngle();

        //flag positions
        redTeamFlag.GetComponent<Flag>().SetTeam(Team.RED);
        blueTeamFlag.GetComponent<Flag>().SetTeam(Team.BLUE);    
        redTeamFlag.GetComponent<Flag>().ResetPosition();
        blueTeamFlag.GetComponent<Flag>().ResetPosition();
    }

    private void BeginCountdownForRound() {
        SetRoundInProgress(false);
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
        SetRoundInProgress(true);
    }

    private void SetRoundInProgress(bool inProgress) {
        roundInProgress = inProgress;
        RoundStateUpdateClientRpc(inProgress);
    }
    
    private void SetGameInProgress(bool inProgress) {
        gameInProgress = inProgress;
        GameStateUpdateClientRpc(inProgress);
    }

    #region Client RPCS

    [ClientRpc]
    private void CountdownClientRpc(int count) {
        UIManager.Instance.DisplayCountdown(count);        
    } 

    [ClientRpc]
    private void RoundStateUpdateClientRpc(bool state) {
        roundInProgress = state;
    } 
    
    [ClientRpc]
    private void GameStateUpdateClientRpc(bool state) {
        gameInProgress = state;
    } 

    [ClientRpc]
    private void GameOverClientRpc(Team winningTeam) {
        UIManager.Instance.DisplayGameOver(winningTeam);
        UIManager.Instance.FreezeCamera();
    }

    #endregion

    #region Game Methods
    public void FlagCapturedBy(Player player) {
        if(OnFlagCaptured != null) OnFlagCaptured(player);
    }
    
    public void ScorePoint(Player player) {
        if(!IsServer) {return;}
        if(OnPlayerScored != null) OnPlayerScored(player);

        Team team = player.GetTeam();
        if(team == Team.BLUE) {
            blueTeamScore.Value += 1;
        }
        else {
            redTeamScore.Value += 1;
        }        

        if(blueTeamScore.Value >= winScore) {
            GameOver(Team.BLUE);
        }
        else if(redTeamScore.Value >= winScore) {
            GameOver(Team.RED);
        }
        else {
            ResetRoundClientRpc();
            BeginCountdownForRound();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void GameOver(Team winningTeam) {
        SetRoundInProgress(false);
        StatsManager.Instance.PublishStats();
        GameOverClientRpc(winningTeam);
        SetGameInProgress(false);
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
            playerController.SetUser(user);

            //spawn player
            int playerIndex = (user.team == Team.RED ? redTeamIndex : blueTeamIndex);
            GameObject spawnedPlayerGO = PlayerSpawner.Instance.SpawnPlayer(user, playerIndex, roomSize); 

            //link
            // playerController.LinkPlayerReference(spawnedPlayerGO);            

            if(user.team == Team.BLUE) {
                blueTeamIndex += 1;
            }
            else {
                redTeamIndex += 1;
            }
        }
    }

    // void SpawnFlags() {
    //     redTeamFlag = GameObject.Instantiate(PrefabsManager.Instance.flag, Vector3.zero, Quaternion.identity).GetComponent<Flag>();
    //     blueTeamFlag = GameObject.Instantiate(PrefabsManager.Instance.flag, Vector3.zero, Quaternion.identity).GetComponent<Flag>();
    //     redTeamFlag.GetComponent<NetworkObject>().Spawn();
    //     blueTeamFlag.GetComponent<NetworkObject>().Spawn();
    //     redTeamFlag.GetComponent<Flag>().SetTeam(Team.RED);
    //     blueTeamFlag.GetComponent<Flag>().SetTeam(Team.BLUE);        
    // }

    #endregion

    #region Event Triggers

    public void TriggerOnPlayerJailed(Player playerJailed, Player playerCatcher) {
        if(OnPlayerJailed!=null) OnPlayerJailed(playerJailed, playerCatcher);

        //check if any free players
        List<LocalPlayer> jailedPlayerAllies = LocalPlayer.PlayersFromTeam(playerJailed.team);

        bool shouldContinueRound = false;
        foreach(LocalPlayer ally in jailedPlayerAllies) {
            if(!ally.isJailed) {
                //as long as one ally is not jailed, the round continues
                shouldContinueRound = true;
                break;        
            }
        }

        if(!shouldContinueRound) {
            //end round
            ScorePoint(playerCatcher);
        }        
    }
    
    public void TriggerOnPlayerFreed(Player playerFreed, Player playerFreedBy) {
        if(OnPlayerFreed!=null) OnPlayerFreed(playerFreed, playerFreedBy);
    }

    #endregion

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

    