using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class EndZone : MonoBehaviour
{
    public Team team = Team.BLUE;
    // float endZoneSize = 11;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    // void FixedUpdate()
    // {
    //     List<Player> players = PlayerSpawner.Instance.GetAllPlayers();
    //     List<Player> teamPlayers = players.FindAll((Player player)=>player.GetTeam() == team);           
    //     foreach(Player player in teamPlayers) {
    //         Vector3 centerToPlayer = (player.transform.position-this.transform.position);
    //         if(centerToPlayer.magnitude < endZoneSize) {
    //             Vector3 newPos = this.transform.position + centerToPlayer.normalized*endZoneSize;
    //             player.transform.position = new Vector3(newPos.x, player.transform.position.y, newPos.z);
    //         }
    //     }
    // }

    void OnTriggerEnter(Collider hit) {
        if(!NetworkManager.Singleton.IsServer) return;
        Player player = hit.gameObject.GetComponent<Player>();
        if(!player) {return;}
        bool redTeamShouldScore = (player.team == Team.RED && team == Team.RED && GameManager.Instance.blueTeamFlag.capturer == player.localPlayer);
        bool blueTeamShouldScore = (player.team == Team.BLUE && team == Team.BLUE && GameManager.Instance.redTeamFlag.capturer == player.localPlayer);
        if((redTeamShouldScore || blueTeamShouldScore)) {
            GameManager.Instance.ScorePoint(player);
        }
        // Flag flag = hit.gameObject.GetComponent<Flag>();
        // if(flag && flag.GetTeam() != team && flag.capturer != null) {
        //     GameManager.Instance.ScorePoint(flag.capturer);
        // }
    }
}
