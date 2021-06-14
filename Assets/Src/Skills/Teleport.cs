using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Teleport skill
public class Teleport : Skill
{
    private float teleportDistance = 20.0f;

    public Teleport()
    {
        cooldown = 6.0f;
        name = "Teleport";
    }

    public override void UseSkill(Player player)
    {
        CharacterController cc = player.GetComponent<CharacterController>(); 
        cc.enabled = false;
        player.transform.position += player.transform.forward * teleportDistance;
        cc.enabled = true;
    }
}