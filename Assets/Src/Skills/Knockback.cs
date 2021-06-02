using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : Skill
{
    private float finalDistance = 15.0f;
    private float timeTaken = 0.3f;
    private float radius = 15.0f;

    public Knockback()
    {
        cooldown = 6.0f;
        name = "Knockback";
    }

    public override void UseSkill(Player player)
    {
        Debug.Log(name + " skill is used");
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, radius);
        Debug.Log(hitColliders.Length);
        foreach (Collider c in hitColliders)
        {   
            Player target = c.gameObject.GetComponent<Player>();
            
            if (target != null && player != target)
            {
                Debug.Log(target.ToString());
                Vector3 direction = target.transform.position - player.transform.position;
                float currentDistance = direction.magnitude;
                float knockbackDistance = finalDistance - currentDistance;
                PushEffect effect = new PushEffect(target, direction, knockbackDistance, timeTaken);
                target.TakeEffect(effect);
            }
        }

        

    }
}
