using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndZone : MonoBehaviour
{
    public Team team = Team.BLUE;
    float endZoneSize = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        List<Player> players = PlayerSpawner.Instance.GetAllPlayers();
        List<Player> teamPlayers = players.FindAll((Player player)=>player.GetTeam() == team);           
        foreach(Player player in teamPlayers) {
            Vector3 centerToPlayer = (player.transform.position-this.transform.position);
            if(centerToPlayer.magnitude < endZoneSize) {
                Vector3 newPos = this.transform.position + centerToPlayer.normalized*endZoneSize;
                player.transform.position = new Vector3(newPos.x, player.transform.position.y, newPos.z);
            }
        }
    }

    void OnTriggerEnter(Collider hit) {
        Flag flag = hit.gameObject.GetComponent<Flag>();
        if(flag && flag.GetTeam() != team && flag.capturer != null) {
            GameManager.Instance.ScorePoint(flag.capturer);
        }
    }
}
