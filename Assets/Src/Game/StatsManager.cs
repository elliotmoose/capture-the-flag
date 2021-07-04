using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable.Collections;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public delegate void OnStatsDisplayUpdated();

public class StatsManager : NetworkBehaviour
{    
    public static StatsManager Instance;
    Dictionary<ulong, GameStat> stats = new Dictionary<ulong, GameStat>();

    //network variable for sole purpose of sending to clients
    NetworkList<GameStat> displayStats = new NetworkList<GameStat>(new NetworkVariableSettings {
        WritePermission=NetworkVariablePermission.ServerOnly,
        SendTickrate=3
    });

    public OnStatsDisplayUpdated StatsDisplayUpdated;

    void Awake() {
        Instance = this;

        displayStats.OnListChanged += (NetworkListEvent<GameStat> changeEvent)=>{
            if(StatsDisplayUpdated != null) StatsDisplayUpdated();
        };
    }

    public void Initialise(List<User> users) {
        foreach(User user in users) {
            GameStat stat = new GameStat{user=user};
            stats.Add(user.clientId, stat);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(!IsServer) return;
        //subscribe to game events
        GameManager.Instance.OnPlayerJailed += (Player playerJailed, Player playerCatcher)=>{
            User catcherUser = playerCatcher.GetUser();
            stats[catcherUser.clientId] += new GameStat{playersCaptured=1};
            
            User jailedUser = playerJailed.GetUser();
            stats[jailedUser.clientId] += new GameStat{timesInJail=1};
        };
        
        GameManager.Instance.OnPlayerFreed += (Player playerFreed, Player playerFreedBy)=>{            
            User freedUser = playerFreedBy.GetUser();
            stats[freedUser.clientId] += new GameStat{playersFreed=1};
        };
        
        GameManager.Instance.OnPlayerScored += (Player player)=>{
            User user = player.GetUser();
            stats[user.clientId] += new GameStat{flagsScored=1};
        };
    }

    void Update() {
        if(!IsServer) return;
        //time in opponent territory
        List<Player> players = PlayerSpawner.Instance.GetAllPlayers();
        foreach(Player player in players) {
            Jail targetJail = player.GetTeam() == Team.RED ? GameManager.Instance.blueTeamJail : GameManager.Instance.redTeamJail;
            if(player.IsInEnemyTerritory() && !targetJail.GetJailedPlayers().Contains(player)) {
                stats[player.GetUser().clientId] += new GameStat{timeInEnemyTerritory=Time.deltaTime};
            }
        }

        //time with flag
        if(GameManager.Instance.blueTeamFlag != null) {            
            Player blueCapturer = GameManager.Instance.blueTeamFlag.capturer;
            if(blueCapturer != null) {
                User user = blueCapturer.GetUser();
                stats[user.clientId] += new GameStat{timeWithFlag=Time.deltaTime};
            }
        }

        if(GameManager.Instance.redTeamFlag != null) {
            Player redCapturer = GameManager.Instance.redTeamFlag.capturer;
            if(redCapturer != null) {
                User user = redCapturer.GetUser();
                stats[user.clientId] += new GameStat{timeWithFlag=Time.deltaTime};
            }
        }
    }

    public List<GameStat> GetStats() {
        List<GameStat> output = new List<GameStat>();
        foreach(GameStat stat in displayStats) {
            output.Add(stat);
        }
        return output;
    }

    public void PublishStats() {
        //server
        displayStats.Clear();
        List<GameStat> statsList = stats.Values.ToList<GameStat>();
        foreach(GameStat stat in statsList) {
            displayStats.Add(stat);
        }
    }
}
