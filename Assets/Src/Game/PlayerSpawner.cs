using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerPrefab;

    public static PlayerSpawner Instance;

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

    public GameObject SpawnPlayer(ulong playerId) {
        GameObject playerObj = GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        playerObj.GetComponent<Player>().ownerClientId.Value = playerId;
        playerObj.GetComponent<NetworkObject>().Spawn();
        return playerObj;
    }
}
