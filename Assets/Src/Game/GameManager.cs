using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public delegate void GameStateEvent();
public delegate void GameEvent(Player player);
public delegate void GameEventInteraction(Player receiver, Player giver);

public enum GameState {
    AWAITING_CONNECTIONS,
    INIT_GAME_SERVER, //init stats, generate summary ui for host player
    SPAWNING, //spawn players and flags
    AWAIT_SPAWN_CONFIRMATION, //spawn players and flags
    TRIGGER_RESET, //reset player positions, effects, flag positions
    COMPLETED_RESET,
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

    public int winScore = 15;
    public float gameTime = 0;
    public int currentRoundNumber => blueTeamScore.Value + redTeamScore.Value + 1;

    // public bool roundInProgress = false;
    // public bool gameInProgress = false;

    public bool roundInProgress => clientState == (GameState.PLAY);
    public bool gameInProgress => (clientState >= GameState.COUNTDOWN) && (clientState != GameState.SCORESCREEN);

    public event GameStateEvent OnRoundStart;
    public event GameEvent OnFlagScored;
    public event GameEvent OnCaughtLastPlayer;
    public event GameEvent OnFlagCaptured;
    public event GameEventInteraction OnFlagPassed;
    public event GameEventInteraction OnPlayerEvade;
    public event GameEventInteraction OnPlayerJailed;
    public event GameEventInteraction OnPlayerFreed;

    [SerializeField]
    GameState serverState = GameState.AWAITING_CONNECTIONS;
    [SerializeField]
    GameState clientState = GameState.AWAITING_CONNECTIONS;
    
    //Spawn PlayerControllers (spawns players) -> Spawn flags -> (INSERT SANITY CHECK FOR PLAYER SPAWN) )Reset Positions 
    public void StartGame() {        
        if(!IsServer) { return; }    
        ServerUpdateGameState(GameState.INIT_GAME_SERVER);           
    }

    [ClientRpc]
    private void ClientResetRoundClientRpc() {
        ClientResetRound();
    }

    private void ClientResetRound() {
        //player positions
        //IMPLEMENTATION NOTE:
        //we need to reset all local players, because the isJailed flag is not synced
        foreach(LocalPlayer player in LocalPlayer.AllPlayers()) {
            player.ResetForRound(); 
        }

        //flag positions
        redTeamFlag.GetComponent<Flag>().ResetPosition();
        blueTeamFlag.GetComponent<Flag>().ResetPosition();
        PlayerController.LocalInstance.playerGameState = GameState.COMPLETED_RESET; //for acknowledgement
        if(IsServer) serverState = GameState.COMPLETED_RESET;
        if(IsClient) clientState = GameState.COMPLETED_RESET;
    }

    private IEnumerator RoundCountdown() {
        CountdownClientRpc(3);
        yield return new WaitForSeconds(1);
        CountdownClientRpc(2);
        yield return new WaitForSeconds(1);
        CountdownClientRpc(1);
        yield return new WaitForSeconds(1);
        CountdownClientRpc(0);        
        if(OnRoundStart != null) OnRoundStart();
        ServerUpdateGameState(GameState.PLAY);
    }

    #region Client RPCS

    [ClientRpc]
    private void CountdownClientRpc(int count) {
        UIManager.Instance.DisplayCountdown(count);        
    } 
    
    private void ServerUpdateGameState(GameState state) {
        Debug.Log($"== GameManager (Server): Setting State: {state}");
        serverState = state;
        UpdateGameStateClientRpc(state);
        OnServerStateChange(state);
    }
    
    [ClientRpc]
    private void UpdateGameStateClientRpc(GameState state) {
        Debug.Log($"== GameManager (Client): Setting State: {state}");
        clientState = state;        
        OnClientStateChange(state);
    } 

    void OnServerStateChange(GameState newState) {
        if(!IsServer) return;
        switch (serverState)
        {
            case GameState.AWAITING_CONNECTIONS:
                break;
            case GameState.INIT_GAME_SERVER:
                // Debug.Log("== GameManager (Server): Initialising Server");
                UIManager.Instance.GenerateGameSummaryUI();
                StatsManager.Instance.Initialise(RoomManager.Instance.GetUsers());  
                ServerUpdateGameState(GameState.SPAWNING);
                break;
            case GameState.SPAWNING:
                // Debug.Log("== GameManager (Server): Spawning Players");
                SpawnPlayerControllers();
                ServerUpdateGameState(GameState.AWAIT_SPAWN_CONFIRMATION);
                break;
            case GameState.AWAIT_SPAWN_CONFIRMATION:
                ServerUpdateGameState(GameState.TRIGGER_RESET); //sends out an RPC requesting reset
                break;
            case GameState.TRIGGER_RESET:
                break;
            case GameState.COMPLETED_RESET: //completed by ClientResetRound
                break;
            case GameState.COUNTDOWN:
                StartCoroutine(RoundCountdown());
                break;
            case GameState.PLAY:
                // Debug.Log("== GameManager: IN PLAY");
                break;
            default:
                break;
        }

    }

    void OnClientStateChange(GameState newState) {
        if(!IsClient) return;
        PlayerController.LocalInstance.playerGameState = newState; 

        switch (newState)
        {
            case GameState.TRIGGER_RESET:
                ClientResetRound();
                break;
            default:
                break;
        }
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
    
    public void FlagPassed(Player player, Player by) {
        if(OnFlagPassed != null) OnFlagPassed(player, by);
    }
    
    public void ScorePoint(Player player) {
        if(!IsServer) {return;}

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
            ServerUpdateGameState(GameState.TRIGGER_RESET);
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        gameTime += Time.deltaTime;
        if(IsServer) {
            if(serverState == GameState.AWAIT_SPAWN_CONFIRMATION) {
                
            }
            
            if(serverState == GameState.COMPLETED_RESET) {
                bool allClientsCompletedReset = true;
                foreach (PlayerController controller in PlayerController.AllControllers())
                {
                    if(controller.playerGameState != GameState.COMPLETED_RESET) {
                        Debug.Log($"Waiting for user: {controller.GetUser().username}");
                        allClientsCompletedReset = false;
                    }
                    else {
                        Debug.Log($"User successfully reset: {controller.GetUser().username}");
                    }
                }

                if(allClientsCompletedReset) {
                    ServerUpdateGameState(GameState.COUNTDOWN); //start round
                }
            }
        }
    }

    void GameOver(Team winningTeam) {
        ServerUpdateGameState(GameState.SCORESCREEN);
        StatsManager.Instance.PublishStats();
        GameOverClientRpc(winningTeam);
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

    #endregion

    #region Event Triggers

    public void TriggerOnFlagScored(Player player) {
        if(!IsServer) return;
        if(OnFlagScored != null) OnFlagScored(player);
        ScorePoint(player);
    }

    public void TriggerOnPlayerJailed(Player playerJailed, Player playerCatcher) {
        if(!IsServer) return;
        if(OnPlayerJailed!=null) OnPlayerJailed(playerJailed, playerCatcher);

        //check if any free players
        List<LocalPlayer> jailedPlayerAllies = LocalPlayer.PlayersFromTeam(playerJailed.team);

        bool shouldContinueRound = false;
        foreach(LocalPlayer ally in jailedPlayerAllies) {
            Debug.Log($"{ally.gameObject.name} isJailed: {ally.isJailed}");
            if(!ally.isJailed) {
                //as long as one ally is not jailed, the round continues
                shouldContinueRound = true;
                break;        
            }
        }

        if(!shouldContinueRound) {
            //end round
            if(OnCaughtLastPlayer != null) OnCaughtLastPlayer(playerCatcher);
            ScorePoint(playerCatcher);
        }        
    }

    public void TriggerOnPlayerEvade(Player evadedPlayer, Player catcher) {
        if(!IsServer) return;
        if(OnPlayerEvade != null) OnPlayerEvade(evadedPlayer, catcher);
    }
    
    public void TriggerOnPlayerFreed(Player playerFreed, Player playerFreedBy) {
        if(!IsServer) return;
        if(OnPlayerFreed!=null) OnPlayerFreed(playerFreed, playerFreedBy);
    }

    #endregion

    public bool PlayerHasFlag(LocalPlayer player) {
        if(player.team == Team.BLUE && redTeamFlag.capturer == player) return true;
        if(player.team == Team.RED && blueTeamFlag.capturer == player) return true;
        return false;
    }

    public Flag FlagForPlayer(LocalPlayer player) {
        if(!PlayerHasFlag(player)) return null;

        if(player.team == Team.BLUE) return redTeamFlag;
        else return blueTeamFlag;
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

    