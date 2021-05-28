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
        //test
        // NetworkManager.StartHost();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Player> GetAllPlayers() {
        return players.Values.ToList<Player>();
    }

    public GameObject SpawnPlayer(ulong playerId, Team team, Character character) {
        if(!IsServer) {return null;}

        GameObject characterPrefab;
        
        switch (character)
        {
            case Character.Warrior:
                characterPrefab = playerPrefab;
                break;
            case Character.Thief:
                characterPrefab = playerPrefab;
                break;
            case Character.Mage:
                characterPrefab = playerPrefab;
                break;
            default:
                characterPrefab = playerPrefab;
                break;
        }

        GameObject playerObj = GameObject.Instantiate(characterPrefab, Vector3.zero, Quaternion.identity);
        Player player = playerObj.GetComponent<Player>();
        player.ownerClientId.Value = playerId;
        player.team = team; //TODO: check if team is set on clients
        playerObj.GetComponent<NetworkObject>().Spawn();
        players[playerId] = player;
        return playerObj;
    }
}
