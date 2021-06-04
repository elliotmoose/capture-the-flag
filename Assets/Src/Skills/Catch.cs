using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catch : Skill
{
    private float radius;
    public Catch(float radius)
    {
        cooldown = 1.0f;
        name = "Catch";
        this.radius = radius;
        
    }

    public override void UseSkill(Player player)
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, radius);

        foreach (Collider c in hitColliders)
        {
            Player target = c.gameObject.GetComponent<Player>();
            
            if (target != null){
                if (player.GetTeam() != target.GetTeam())
                {
                    if(target.IsCatchable()) {
                        GameManager.Instance.Imprison(target, player);
                    }
                    // else if (player.IsCatchable()){ //i can imprison myself
                    //     player.GetJail().Imprison(player);
                    // }
                }
                else
                {
                    GameManager.Instance.Release(target, player);                    
                }
            }

        }
    }
}
