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

    public GameObject SpawnPlayer(ulong playerId, Team team, Character character, int index, int noOfPlayersPerTeam) {
        if(!IsServer) {return null;}

        GameObject characterPrefab;
        
        switch (character)
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

        float teamPosition = 120 * (team == Team.BLUE ? 1 : -1);     
        float distanceBetweenPlayers = 7;   
        float teamIndexPosition = -(noOfPlayersPerTeam-1) * distanceBetweenPlayers/2 + (index * distanceBetweenPlayers);
        Vector3 spawnPosition = new Vector3(teamIndexPosition,0,teamPosition);
        // Debug.Log($"{spawnPosition} {index}");

        Quaternion faceDirection = Quaternion.Euler(0, team == Team.BLUE ? 180 : 0, 0);
        GameObject playerObj = GameObject.Instantiate(characterPrefab, spawnPosition, faceDirection);
        Player player = playerObj.GetComponent<Player>();
        player.ownerClientId.Value = playerId;
        player.team = team; //TODO: check if team is set on clients
        playerObj.GetComponent<NetworkObject>().Spawn();
        players[playerId] = player;
        return playerObj;
    }
}
