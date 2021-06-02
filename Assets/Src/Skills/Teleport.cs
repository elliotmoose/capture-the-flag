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
        Debug.Log(name + " skill is used");
        Debug.Log(player.transform.position.ToString());
        player.transform.position += player.transform.forward * teleportDistance;
        Debug.Log(player.transform.position.ToString());
    }
}