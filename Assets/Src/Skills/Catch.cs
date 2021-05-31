using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catch : Skill
{
    private float radius;
    public Catch(float radius)
    {
        cooldown = 3.0f;
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
                if (player.team != target.team && target.IsCatchable())
                {
                    target.GetJail().Imprison(target);

                }
                else if (player.team == target.team)
                {
                    target.GetJail().Release(target);
                }
            }

        }
    }
}
