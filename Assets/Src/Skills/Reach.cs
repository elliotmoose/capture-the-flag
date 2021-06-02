using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reach : Skill
{

    private float distance = 10.0f; // distance reach extends to
    private float angle = 10.0f; // angle margin allowed
    

    public Reach()
    {
        cooldown = 6.0f;
        name = "Extended Reach";

    }
    public override void UseSkill(Player player)
    {
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, distance);

        foreach (Collider c in hitColliders)
        {
            Player target = c.gameObject.GetComponent<Player>();

            if (target != null)
            {
                float current_angle = Vector3.Angle(player.transform.forward, target.transform.position - player.transform.position);
                if (current_angle <= angle)
                {
                    if (player.team != target.team)
                    {
                        if (target.IsCatchable())
                        {
                            GameManager.Instance.Imprison(target, player);
                        }
                    }
                    else if (player.team == target.team)
                    {
                        GameManager.Instance.Release(target, player);
                    }
                }
                
            }

        }
    }
}
