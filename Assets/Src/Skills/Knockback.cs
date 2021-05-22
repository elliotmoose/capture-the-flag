using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : Skill
{
    private float knockbackFactor = 10.0f;
    private float radius = 5.0f;

    public Knockback()
    {
        cooldown = 6.0f;
        name = "Knockback";
    }

    public override void UseSkill(Player player)
    {
        Debug.Log(name + " skill is used");
        Collider[] hitColliders = Physics.OverlapSphere(player.transform.position, radius);

        foreach (Collider c in hitColliders)
        {
            Player target = c.gameObject.GetComponent<Player>();

            if (target != null && player != target)
            {
                Vector3 vectorDistance = target.transform.position - player.transform.position;
                Vector3 unitVector = vectorDistance.normalized;
                float magnitude = vectorDistance.magnitude;
                Rigidbody targetBody = target.gameObject.GetComponent<Rigidbody>();
                // the closer the targets are to the player, the stronger the force
                targetBody.AddForce(unitVector * knockbackFactor / magnitude, ForceMode.Impulse);
            }
        }

        

    }
}
