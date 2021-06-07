using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneCatch : Catch
{
    public CloneCatch() : base()
    {
        name = "Clone Catch";
    }

    public override void UseSkill(Player player)
    {
        base.UseSkill(player);
        float radius = player.GetCatchRadius();
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, radius);

        foreach (Collider c in hitColliders)
        {
            Player target = c.gameObject.GetComponent<Player>();

            if (target == null)
            {
                continue;
            }
            
            if(target == player) {
                // Debug.Log("cannot clone self");
                continue;
            }

            if(target.skills.Count < 2) {
                Debug.Log("Target does not have enough skills");
                continue;
            }

            Skill skill = target.skills[1];
            if (player.skills.Count == 2)
            {
                player.skills[1] = skill;
            }
            else if (player.skills.Count == 1)
            {
                player.skills.Add(skill);
            }

            break;
        }
    }

}
