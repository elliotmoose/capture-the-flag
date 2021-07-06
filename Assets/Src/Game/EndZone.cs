using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        LocalPlayer localPlayer = hit.gameObject.GetComponent<LocalPlayer>();
        if(!localPlayer) {return;}
        bool redTeamShouldScore = (localPlayer.team == Team.RED && team == Team.RED && GameManager.Instance.blueTeamFlag.capturer == localPlayer);
        bool blueTeamShouldScore = (localPlayer.team == Team.BLUE && team == Team.BLUE && GameManager.Instance.redTeamFlag.capturer == localPlayer);
        if((redTeamShouldScore || blueTeamShouldScore)) {
            GameManager.Instance.ScorePoint(localPlayer.syncPlayer);
            Debug.LogWarning("TODO: change score to server");
        }
        // Flag flag = hit.gameObject.GetComponent<Flag>();
        // if(flag && flag.GetTeam() != team && flag.capturer != null) {
        //     GameManager.Instance.ScorePoint(flag.capturer);
        // }
    }
}
