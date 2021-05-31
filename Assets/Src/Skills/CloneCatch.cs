using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneCatch : Catch
{
    private float radius;
    public CloneCatch(float radius) : base(radius)
    {
        name = "Clone Catch";
        this.radius = radius;
    }

    public override void UseSkill(Player player)
    {
        base.UseSkill(player);
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, this.radius);

        foreach (Collider c in hitColliders)
        {
            Player target = c.gameObject.GetComponent<Player>();

            if (target != null)
            {
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

}
