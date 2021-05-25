using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    GameManager() : base() {
        if(Instance != null) {
            throw new System.Exception("More than one GameManager exists");
        }
        Instance = this;
    }

    public GameObject redTeamFlag;
    public GameObject blueTeamFlag;
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

    // Update is called once per frame
    void Update()
    {
        
    }

    void ResetFlags() {
        if(!IsServer) { return; }
        redTeamFlag.GetComponent<Flag>().SetTeam(0);
        redTeamFlag.GetComponent<Flag>().ResetPosition();
        blueTeamFlag.GetComponent<Flag>().SetTeam(1);
    }
}
