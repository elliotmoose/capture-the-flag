using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerPrefab;

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

    public GameObject SpawnPlayer(ulong playerId, Team team) {
        if(!IsServer) {return null;}
        GameObject playerObj = GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        Player player = playerObj.GetComponent<Player>();
        player.ownerClientId.Value = playerId;
        player.team = team; //TODO: check if team is set on clients
        playerObj.GetComponent<NetworkObject>().Spawn();
        players[playerId] = player;
        return playerObj;
    }
}
