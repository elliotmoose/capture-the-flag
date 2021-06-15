using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject magePrefab;
    public GameObject warriorPrefab;
    public GameObject thiefPrefab;
    public GameObject bowmanPrefab;

    public static PlayerSpawner Instance;

    Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();

    PlayerSpawner() : base(){
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Player> GetAllPlayers() {
        return players.Values.ToList<Player>();
    }

    public GameObject SpawnPlayer(User user, int index, int noOfPlayersPerTeam) {
        if(!IsServer) {return null;}

        GameObject characterPrefab;
        
        switch (user.character)
        {
            case Character.Warrior:
                characterPrefab = warriorPrefab;
                break;
            case Character.Thief:
                characterPrefab = thiefPrefab;
                break;
            case Character.Mage:
                characterPrefab = magePrefab;
                break;
            case Character.Bowman:
                characterPrefab = bowmanPrefab;
                break;
            default:
                characterPrefab = warriorPrefab;
                break;
        }

        Team team = user.team;
        
        float teamPosition = 100 * (team == Team.BLUE ? 1 : -1);     
        float distanceBetweenPlayers = 7;   
        float teamIndexPosition = -(noOfPlayersPerTeam-1) * distanceBetweenPlayers/2 + (index * distanceBetweenPlayers);
        Vector3 spawnPosition = new Vector3(teamIndexPosition,0,teamPosition);
        // Debug.Log($"{spawnPosition} {index}");

        Quaternion faceDirection = Quaternion.Euler(0, team == Team.BLUE ? 180 : 0, 0);
        GameObject playerObj = GameObject.Instantiate(characterPrefab, spawnPosition, faceDirection);
        Player player = playerObj.GetComponent<Player>();
        player.spawnPos = spawnPosition;
        player.spawnDir = faceDirection;
        player.SetUser(user);
        player.SetTeam(team);
        playerObj.GetComponent<NetworkObject>().Spawn();
        players[user.clientId] = player;
        return playerObj;
    }
}
