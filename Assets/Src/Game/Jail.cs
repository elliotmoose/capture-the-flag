using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jail : MonoBehaviour
{    
    public Color color;
    List<Player> jailed = new List<Player>();
    float jailSize = 10; //jail radius

    public void Imprison(Player player) {
        if(!jailed.Contains(player)) {
            jailed.Add(player);
        }
    }
    
    public void Release(Player player) {
        if(jailed.Contains(player)) {
            jailed.Remove(player);
        }
    }

    public List<Player> GetJailedPlayers()
    {
        return this.jailed;
    }

    public void ReleaseAll() {
        jailed.Clear();
    }


    // Start is called before the first frame update
    void Start()
    {
        ParticleSystem.ShapeModule shape = GetComponentInChildren<ParticleSystem>().shape;
        shape.radius = jailSize;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = GetComponentInChildren<ParticleSystem>().colorOverLifetime;
        colorOverLifetime.color = color;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 jailCenter = this.transform.position;
        foreach(Player player in jailed) {
            Vector3 jailToPlayer = (player.transform.position-jailCenter);
            if(jailToPlayer.magnitude > jailSize) {
                Vector3 newPos = jailCenter + jailToPlayer.normalized*jailSize;
                player.transform.position = new Vector3(newPos.x, player.transform.position.y, newPos.z);
            }
        }
    }
}
