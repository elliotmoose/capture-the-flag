using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

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

    List<Player> blueTeamPlayers = new List<Player>();
    List<Player> redTeamPlayers = new List<Player>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void NetworkStart() {
        base.NetworkStart();
        if(!IsServer) { return; }
        StartGame();
    }

    public void StartGame() {
        if(!IsServer) { return; }
        
        Debug.Log("== GameManager: Game Started!");
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
    }

    void OnDestroy() {
        Instance = null;
    }
}
