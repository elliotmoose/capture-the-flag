using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Reach : Skill
{

    private float distance = 10.0f; // distance reach extends to
    private float angle = 10.0f; // angle margin allowed
    

    public Reach()
    {
        cooldown = 6.0f;
        name = "Extended Reach";

    }
    public override void UseSkill(LocalPlayer localPlayer)
    {        
        Collider[] hitColliders = Physics.OverlapSphere(localPlayer.transform.position, distance);

        foreach (Collider c in hitColliders)
        {
            LocalPlayer target = c.gameObject.GetComponent<LocalPlayer>();

            if (target != null)
            {
                float current_angle = Vector3.Angle(localPlayer.transform.forward, target.transform.position - localPlayer.transform.position);
                if (current_angle <= angle)
                {
                    target.ClientContact(localPlayer);
                }                
            }

        }
    }
}
